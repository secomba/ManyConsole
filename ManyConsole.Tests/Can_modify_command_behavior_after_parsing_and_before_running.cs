using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NJasmine;

namespace ManyConsole.Tests
{
    public class Can_modify_command_behavior_after_parsing_and_before_running : GivenWhenThenFixture
    {
        public class OverridingCommand : ConsoleCommand<DefaultCommandResult, DefaultCommandSettings>
        {
            public OverridingCommand()
            {
                this.IsCommand("fail-me-maybe");
                this.HasOption<int>("n=", "number", v => Maybe = v);
            }

            public int? Maybe;

            public override DefaultCommandResult OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments, out bool cancel, ref DefaultCommandSettings settings)
            {
                cancel = Maybe != null && Maybe.Value != 0;
                return new DefaultCommandResult() { ExitCode = Maybe ?? 0 };
            }

            public override DefaultCommandResult Run(string[] remainingArguments, ref DefaultCommandSettings settings)
            {
                return new DefaultCommandResult();
            }
        }

        public override void Specify()
        {
            it("can specify a return code and halt execution", () =>
            {
                var output = new StringWriter();
                var command = new OverridingCommand();

                var exitCode = arrange(() => ConsoleCommandDispatcher<DefaultCommandResult, DefaultCommandSettings>.DispatchCommand(command, new[] { "/n", "123" }, output)).ExitCode;

                expect(() => exitCode == 123);
                expect(() => String.IsNullOrEmpty(output.ToString()));
            });
        }
    }
}
