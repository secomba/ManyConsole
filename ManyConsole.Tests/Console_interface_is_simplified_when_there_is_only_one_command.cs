using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NJasmine;

namespace ManyConsole.Tests
{
    public class Console_interface_is_simplified_when_there_is_only_one_command : GivenWhenThenFixture
    {
        private const int Success = 999;

        public class ExampleCommand : ConsoleCommand<DefaultCommandResult, DefaultCommandSettings>
        {
            public ExampleCommand()
            {
                this.IsCommand("Example");
                this.HasOption("f|foo=", "This foo to use.", v => Foo = v);
                this.SkipsCommandSummaryBeforeRunning();
            }

            public override DefaultCommandResult Run(string[] remainingArguments, ref DefaultCommandSettings settings)
            {
                return new DefaultCommandResult { ExitCode = Success };
            }

            public string Foo { get; set; }
        }

        public override void Specify()
        {
            given("exactly one command is loaded", () =>
            {
                var exampleCommand = new ExampleCommand();

                when("no parameters are specified", () =>
                {
                    var output = new StringWriter();
                    var exitCode = arrange(() => ConsoleCommandDispatcher<DefaultCommandResult, DefaultCommandSettings>
                        .DispatchCommand(exampleCommand, new string[0], output).ExitCode);

                    then_the_command_runs_without_tracing_parameter_information(output, exitCode);

                    then("the command's property is not set", () =>
                    {
                        expect(() => exampleCommand.Foo == null);
                    });
                });

                when("the only parameter specified is the command", () =>
                {
                    var output = new StringWriter();
                    var exitCode = arrange(() => ConsoleCommandDispatcher<DefaultCommandResult, DefaultCommandSettings>.DispatchCommand(exampleCommand, new[] { "Example" }, output).ExitCode);

                    then_the_command_runs_without_tracing_parameter_information(output, exitCode);

                    then("the command's property is not set", () =>
                    {
                        expect(() => exampleCommand.Foo == null);
                    });
                });

                when("the only parameter specified is not the command", () =>
                {
                    var output = new StringWriter();
                    var exitCode = arrange(() => ConsoleCommandDispatcher<DefaultCommandResult, DefaultCommandSettings>.DispatchCommand(exampleCommand, new[] { "/f=bar" }, output).ExitCode);

                    then_the_command_runs_without_tracing_parameter_information(output, exitCode);

                    then("the command's property is set", () =>
                    {
                        expect(() => exampleCommand.Foo == "bar");
                    });
                });

                when("both the command and an extra parameter are specified", () =>
                {
                    var output = new StringWriter();
                    var exitCode = arrange(() => ConsoleCommandDispatcher<DefaultCommandResult, DefaultCommandSettings>.DispatchCommand(exampleCommand, new[] { "Example", "/f=bar" }, output).ExitCode);

                    then_the_command_runs_without_tracing_parameter_information(output, exitCode);

                    then("the command's property is set", () =>
                    {
                        expect(() => exampleCommand.Foo == "bar");
                    });
                });
            });
        }

        private void then_the_command_runs_without_tracing_parameter_information(StringWriter output, int exitCode)
        {
            then("the output is empty", () =>
            {
                expect(() => string.IsNullOrEmpty(output.ToString().Trim()));
            });

            then("the exit code indicates the call succeeded", () =>
            {
                expect(() => exitCode == Success);
            });
        }
    }
}
