using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole.Internal;
using NDesk.Options;

namespace ManyConsole
{
    public class ConsoleCommandDispatcher
    {
        public static int DispatchCommand(IConsoleCommand command, string[] arguments, TextWriter consoleOut)
        {
            return DispatchCommand(new[] {command}, arguments, consoleOut);
        }

        public static int DispatchCommand(IList<IConsoleCommand> commands, string[] arguments, TextWriter consoleOut,
            bool skipExeInExpectedUsage = false, IHelpCommand customHelpCommand = null, bool globallyDisableTraceCommandAfterParse = false)
        {
            if (customHelpCommand != null)
            {
                customHelpCommand.SkipExeInExpectedUsage = skipExeInExpectedUsage;
            }

            IConsoleCommand selectedCommand = null;

            var console = consoleOut;

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
                        return customHelpCommand.Run(helpRemainingArgs);
                    }

                    if (arguments.First().Equals("help", StringComparison.InvariantCultureIgnoreCase))
                    {
                        selectedCommand = GetMatchingCommand(commands.Where(it => !it.IsHidden).ToList(),
                            arguments.Skip(1).FirstOrDefault()); // excude hidden commands from help mechanism

                        if (selectedCommand == null)
                        {
                            ConsoleHelp.ShowSummaryOfCommands(commands, console);
                        }
                        else
                        {
                            ConsoleHelp.ShowCommandHelp(selectedCommand, console, skipExeInExpectedUsage);
                        }
                        return -1;
                    }

                    selectedCommand = GetMatchingCommand(commands, arguments.First());

                    if (selectedCommand == null)
                        throw new ConsoleHelpAsException("Command name not recognized.");

                    remainingArguments = selectedCommand.GetActualOptions().Parse(arguments.Skip(1));
                }

                selectedCommand.CheckRequiredArguments();

                selectedCommand.CheckSubLevelArguments(arguments.Skip(1).ToArray());

                CheckRemainingArguments(remainingArguments, selectedCommand.RemainingArgumentsCount);

                var preResult = selectedCommand.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments.ToArray());

                if (preResult.HasValue)
                    return preResult.Value;

                if (!globallyDisableTraceCommandAfterParse)
                {
                    ConsoleHelp.ShowParsedCommand(selectedCommand, console);
                }

                return selectedCommand.Run(remainingArguments.ToArray());
            }
            catch (Exception e)
            {
                if (e is ConsoleHelpAsException || e is OptionException)
                {
                    if (customHelpCommand != null)
                    {
                        // also use OverrideAfterHandlingArgumentsBeforeRun mechanism for help command
                        var remainingArguments = new string[0];
                        try
                        {
                            remainingArguments = customHelpCommand.GetActualOptions().Parse(arguments).ToArray();

                        } catch {
                            // ignore parsing errors for help command
                        }

                        var preResult = customHelpCommand.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments);
                        if (preResult.HasValue)
                            return preResult.Value;

                        // also show manyconsole exception message
                        console.WriteLine();
                        console.WriteLine(e.Message);
                        
                        return customHelpCommand.Run(remainingArguments);
                    }
                    return DealWithException(e, console, skipExeInExpectedUsage, selectedCommand, commands);
                }
                throw;
            }
        }

        private static int DealWithException(Exception e, TextWriter console, bool skipExeInExpectedUsage,
            IConsoleCommand selectedCommand, IEnumerable<IConsoleCommand> commands)
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

            return -1;
        }

        private static IConsoleCommand GetMatchingCommand(IList<IConsoleCommand> command, string name)
        {
            return command.FirstOrDefault(c => ConsoleUtil.DoesArgMatchCommand(name, c));
        }

        private static void ValidateConsoleCommand(IConsoleCommand command)
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

        public static IList<IConsoleCommand> FindCommandsInSameAssemblyAs(Type typeInSameAssembly, Func<Type,bool> validateCommandType = null)
        {
            var assembly = typeInSameAssembly.Assembly;

            var commandTypes = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof (ConsoleCommand)))
                .Where(t => !t.IsAbstract)
                .Where(t =>  validateCommandType?.Invoke(t) ?? true)
                .OrderBy(t => t.FullName);

            var result = new List<IConsoleCommand>();

            foreach (var commandType in commandTypes)
            {
                var constructor = commandType.GetConstructor(new Type[] {});

                if (constructor == null)
                    continue;

                result.Add((ConsoleCommand) constructor.Invoke(new object[] {}));
            }

            return result;
        }
    }


   
}