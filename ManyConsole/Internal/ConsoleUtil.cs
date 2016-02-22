using System;
using System.Linq;

namespace ManyConsole.Internal
{
    public abstract class ConsoleUtil
    {
        public static void VerifyNumberOfArguments(string[] args, int expectedArgumentCount)
        {
            if (args.Count() < expectedArgumentCount)
                throw new ConsoleHelpAsException(
                    String.Format("Invalid number of arguments-- expected {0} more.", expectedArgumentCount - args.Count()));
            
            if (args.Count() > expectedArgumentCount)
                throw new ConsoleHelpAsException("Extra parameters specified: " + String.Join(", ", args.Skip(expectedArgumentCount).ToArray()));
        }

        public static bool DoesArgMatchCommand(string argument, IConsoleCommand command)
        {
            if (argument == null || command == null)
            {
                return false;
            }
            return
                command.Command.ToLower()
                    .Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries).Select(it => it.Trim()).ToArray()
                    .Contains(argument.ToLower());
        }

        public static string FormatCommandName(string commandName)
        {
            if (!commandName.Contains("|"))
            {
                return commandName.Trim();
            }
            return String.Join(", ", commandName.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries).Select(it => it.Trim()).ToArray());
        }
    }
}