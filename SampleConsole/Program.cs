using System;
using System.Collections.Generic;
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
            Environment.ExitCode = ConsoleCommandDispatcher<DefaultCommandResult, DefaultCommandSettings>.DispatchCommand(commands, args, new DefaultCommandSettings(Console.Out), new HelpCommand()).ExitCode;
            return Environment.ExitCode;
        }

        public static IList<IConsoleCommand<DefaultCommandResult, DefaultCommandSettings>> GetCommands()
        {
            return ConsoleCommandDispatcher<DefaultCommandResult, DefaultCommandSettings>.FindCommandsInSameAssemblyAs(typeof(Program));
        }
    }


    class HelpCommand : ConsoleCommand<DefaultCommandResult, DefaultCommandSettings>, IHelpCommand<DefaultCommandResult, DefaultCommandSettings>
    {
        public HelpCommand()
        {
            IsCommand("h|help");
            AllowsAnyAdditionalArguments();
        }
        public bool HelpExplicitlyCalled { get; set; }
        public bool SkipExeInExpectedUsage { get; set; }
        public Exception FailureReason { get; set; }

        public override DefaultCommandResult Run(string[] remainingArguments, ref DefaultCommandSettings settings)
        {
            Console.WriteLine(string.Join(", ", remainingArguments));
            return new DefaultCommandResult { ExitCode = 4 };
        }
    }
}
