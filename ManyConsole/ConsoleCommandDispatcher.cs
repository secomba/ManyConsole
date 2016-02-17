﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole.Internal;

namespace ManyConsole
{
    public class ConsoleCommandDispatcher
    {
        public static int DispatchCommand(ConsoleCommand command, string[] arguments, TextWriter consoleOut)
        {
            return DispatchCommand(new [] {command}, arguments, consoleOut);
        }

        public static int DispatchCommand(IList<ConsoleCommand> commands, string[] arguments, TextWriter consoleOut, bool skipExeInExpectedUsage = false, ConsoleCommand customHelpCommand = null)
        {
            ConsoleCommand selectedCommand = null;

            TextWriter console = consoleOut;

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
                    if (customHelpCommand != null && ConsoleUtil.DoesArgMatchCommand(arguments.First(), customHelpCommand))
                    {
                        customHelpCommand.Run(
                                    customHelpCommand.GetActualOptions().Parse(arguments.Skip(1)).ToArray());
                        return -1;
                    }

                    if (arguments.First().Equals("help", StringComparison.InvariantCultureIgnoreCase))
                    {
                        selectedCommand = GetMatchingCommand(commands.Where(it => !it.IsHidden).ToList(), arguments.Skip(1).FirstOrDefault()); // excude hidden commands from help mechanism

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

                CheckRemainingArguments(remainingArguments, selectedCommand.RemainingArgumentsCount);

                var preResult = selectedCommand.OverrideAfterHandlingArgumentsBeforeRun(remainingArguments.ToArray());

                if (preResult.HasValue)
                    return preResult.Value;

                ConsoleHelp.ShowParsedCommand(selectedCommand, console);

                return selectedCommand.Run(remainingArguments.ToArray());
            }
            catch (ConsoleHelpAsException e)
            {
                return DealWithException(e, console, skipExeInExpectedUsage, selectedCommand, commands);
            }
            catch (NDesk.Options.OptionException e)
            {
                return DealWithException(e, console, skipExeInExpectedUsage, selectedCommand, commands);
            }
        }

        private static int DealWithException(Exception e, TextWriter console, bool skipExeInExpectedUsage, ConsoleCommand selectedCommand, IEnumerable<ConsoleCommand> commands)
        {

            if (selectedCommand != null && !selectedCommand.IsHidden)  // dont show help for hidden command even after exception
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
  
        private static ConsoleCommand GetMatchingCommand(IList<ConsoleCommand> command, string name)
        {
            return command.FirstOrDefault(c => ConsoleUtil.DoesArgMatchCommand(name, c));
        }

        private static void ValidateConsoleCommand(ConsoleCommand command)
        {
            if (string.IsNullOrEmpty(command.Command))
            {
                throw new InvalidOperationException(String.Format(
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

        public static IList<ConsoleCommand> FindCommandsInSameAssemblyAs(Type typeInSameAssembly)
        {
            var assembly = typeInSameAssembly.Assembly;

            var commandTypes = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ConsoleCommand)))
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.FullName);

            List<ConsoleCommand> result = new List<ConsoleCommand>();

            foreach(var commandType in commandTypes)
            {
                var constructor = commandType.GetConstructor(new Type[] { });

                if (constructor == null)
                    continue;

                result.Add((ConsoleCommand)constructor.Invoke(new object[] { }));
            }

            return result;
        }
    }
}
