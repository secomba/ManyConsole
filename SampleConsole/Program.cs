using System;
using System.Collections.Generic;
using System.Linq;
using ManyConsole;

namespace SampleConsole
{
    class Program
    {
        static int Main(string[] args)
        {
            // locate any commands in the assembly (or use an IoC container, or whatever source)
            var commands = GetCommands();

            // then run them.
            Environment.ExitCode =  ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out, customHelpCommand: new HelpCommand());
            return Environment.ExitCode;
        }

        public static IList<IConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }
    }


    class HelpCommand : ConsoleCommand, IHelpCommand
    {

        public HelpCommand()
        {
            IsCommand("h|help");
            AllowsAnyAdditionalArguments();
        }
        public bool HelpExplicitlyCalled { get; set; }
        public bool SkipExeInExpectedUsage { get; set; }
        public Exception FailureReason { get; set; }

        public override int Run(string[] remainingArguments)
        {
            Console.WriteLine(string.Join(", ", remainingArguments));
            return 4;
        }
    }
}
