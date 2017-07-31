using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ManyConsole.Internal;
using NDesk.Options;

namespace ManyConsole
{

    public class ConsoleCommandDispatcher : ConsoleCommandDispatcher<DefaultCommandResult, DefaultCommandSettings>
    {
    }

    public class ConsoleCommandDispatcher<TResult, TSettings> where TResult: ICommandResult, new() where TSettings : ICommandSettings, new()
    {
        public static TResult DispatchCommand(IConsoleCommand<TResult, TSettings> command, string[] arguments, TextWriter consoleOut)
        {
            return DispatchCommand(new[] {command}, arguments, new TSettings {ConsoleOut = consoleOut});
        }

        public static TResult DispatchCommand(IList<IConsoleCommand<TResult, TSettings>> commands, string[] arguments, TSettings settings, IHelpCommand<TResult, TSettings> customHelpCommand = null ) {
            if (customHelpCommand != null)
            {
                customHelpCommand.SkipExeInExpectedUsage = settings.SkipExeInExpectedUsage;
            }

            IConsoleCommand<TResult, TSettings> selectedCommand = null;

            foreach (var command in commands)
            {
                ValidateConsoleCommand(command);
            }

            try
            {
                List<string> remainingArguments;

                if (commands.Count == 1)
                {
                    selectedCommand = commands.First();
                    // support basic splitting of command arguments like "q|quit" => q, quit
                    if (arguments.Any() && ConsoleUtil.DoesArgMatchCommand(arguments.First(), selectedCommand))
                    {
                        remainingArguments = selectedCommand.GetActualOptions().Parse(arguments.Skip(1));
                    }
                    else
                    {
                        remainingArguments = selectedCommand.GetActualOptions().Parse(arguments);
                    }
                }
                else
                {
                    if (!arguments.Any())
                        throw new ConsoleHelpAsException("No arguments specified.");

                    // to support custom help implementation
                    if (customHelpCommand != null &&
                        ConsoleUtil.DoesArgMatchCommand(arguments.First(), customHelpCommand))
                    {
                        customHelpCommand.HelpExplicitlyCalled = true;
                        var helpRemainingArgs = new string[0];
                        try
                        {
                            helpRemainingArgs = customHelpCommand.GetActualOptions().Parse(arguments.Skip(1)).ToArray();
                        }
                        catch
                        {
                            // ignore parsing errors for help command
                        }
                        return customHelpCommand.Run(helpRemainingArgs, ref settings);
                    }

                    if (arguments.First().Equals("help", StringComparison.InvariantCultureIgnoreCase))
                    {
                        selectedCommand = GetMatchingCommand(commands.Where(it => !it.IsHidden).ToList(),
                            arguments.Skip(1).FirstOrDefault()); // excude hidden commands from help mechanism

                        if (selectedCommand == null)
                        {
                            ConsoleHelp.ShowSummaryOfCommands(commands, settings.ConsoleOut);
                        }
                        else
                        {
                            ConsoleHelp.ShowCommandHelp(selectedCommand, settings.ConsoleOut, settings.SkipExeInExpectedUsage);
                        }
                        return new TResult {ExitCode = -1};
                    }

                    selectedCommand = GetMatchingCommand(commands, arguments.First());

                    if (selectedCommand == null)
                        throw new ConsoleHelpAsException("Command name not recognized.");

                    remainingArguments = selectedCommand.GetActualOptions().Parse(arguments.Skip(1));
                }

                selectedCommand.CheckRequiredArguments();

                selectedCommand.CheckSubLevelArguments(arguments.Skip(1).ToArray());

                CheckRemainingArguments(remainingArguments, selectedCommand.RemainingArgumentsCount);

                bool cancel;
                var preResult = selectedCommand.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments.ToArray(), out cancel, ref settings);

                if (cancel)
                    return preResult;

                if (!settings.GloballyDisableTraceCommandAfterParse)
                {
                    ConsoleHelp.ShowParsedCommand(selectedCommand, settings.ConsoleOut);
                }

                return selectedCommand.Run(remainingArguments.ToArray(), ref settings);
            }
            catch (Exception e)
            {
                if (e is ConsoleHelpAsException || e is OptionException)
                {
                    if (customHelpCommand != null)
                    {
                        customHelpCommand.FailureReason = e;
                        // also use OverrideAfterHandlingArgumentsBeforeRun mechanism for help command
                        var remainingArguments = new string[0];
                        try
                        {
                            remainingArguments = customHelpCommand.GetActualOptions().Parse(arguments).ToArray();

                        } catch {
                            // ignore parsing errors for help command
                        }
                        bool cancel;
                        var preResult = customHelpCommand.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments, out cancel, ref settings);
                        if (cancel)
                            return preResult;

                        // also show manyconsole exception message
                        settings.ConsoleOut.WriteLine();
                        settings.ConsoleOut.WriteLine(e.Message);
                        
                        return customHelpCommand.Run(remainingArguments, ref settings);
                    }
                    return DealWithException(e, settings.ConsoleOut, settings.SkipExeInExpectedUsage, selectedCommand, commands);
                }
                throw;
            }
        }

        private static TResult DealWithException(Exception e, TextWriter console, bool skipExeInExpectedUsage,
            IConsoleCommand<TResult, TSettings> selectedCommand, IEnumerable<IConsoleCommand<TResult, TSettings>> commands)
        {
            if (selectedCommand != null && !selectedCommand.IsHidden)
                // dont show help for hidden command even after exception
            {
                console.WriteLine();
                console.WriteLine(e.Message);
                ConsoleHelp.ShowCommandHelp(selectedCommand, console, skipExeInExpectedUsage);
            }
            else
            {
                ConsoleHelp.ShowSummaryOfCommands(commands, console);
            }
            
            return new TResult {ExitCode = -1};
        }

        private static IConsoleCommand<TResult, TSettings> GetMatchingCommand(IList<IConsoleCommand<TResult, TSettings>> command, string name)
        {
            return command.FirstOrDefault(c => ConsoleUtil.DoesArgMatchCommand(name, c));
        }

        private static void ValidateConsoleCommand(IConsoleCommand<TResult, TSettings> command)
        {
            if (string.IsNullOrEmpty(command.Command))
            {
                throw new InvalidOperationException(string.Format(
                    "Command {0} did not call IsCommand in its constructor to indicate its name and description.",
                    command.GetType().Name));
            }
        }

        private static void CheckRemainingArguments(List<string> remainingArguments, int? parametersRequiredAfterOptions)
        {
            if (parametersRequiredAfterOptions.HasValue)
                ConsoleUtil.VerifyNumberOfArguments(remainingArguments.ToArray(),
                    parametersRequiredAfterOptions.Value);
        }

        public static IList<IConsoleCommand<TResult, TSettings>> FindCommandsInSameAssemblyAs(Type typeInSameAssembly, Func<Type,bool> validateCommandType = null)
        {
            if (typeInSameAssembly == null)
                throw new ArgumentNullException("typeInSameAssembly");

            return FindCommandsInAssembly(typeInSameAssembly.Assembly, validateCommandType);
        }

        public static IList<IConsoleCommand<TResult, TSettings>> FindCommandsInAllLoadedAssemblies(Func<Type, bool> validateCommandType = null)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(it => FindCommandsInAssembly(it, validateCommandType)).ToList();
        }

        public static IList<IConsoleCommand<TResult, TSettings>> FindCommandsInAssembly(Assembly assembly, Func<Type, bool> validateCommandType = null)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var commandTypes = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ConsoleCommand<TResult, TSettings>)))
                .Where(t => !t.IsAbstract)
                .Where(t =>  validateCommandType?.Invoke(t) ?? true)
                .OrderBy(t => t.FullName);

            var result = new List<IConsoleCommand<TResult, TSettings>>();

            foreach (var commandType in commandTypes)
            {
                var constructor = commandType.GetConstructor(new Type[] {});

                if (constructor == null)
                    continue;

                result.Add((ConsoleCommand<TResult, TSettings>) constructor.Invoke(new object[] {}));
            }

            return result;
        }
    }
}