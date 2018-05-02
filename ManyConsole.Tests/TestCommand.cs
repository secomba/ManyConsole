using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManyConsole.Tests
{
    public class TestCommand : ConsoleCommand<DefaultCommandResult, DefaultCommandSettings>
    {
        public Func<int> Action = delegate { return 0; };

        public override DefaultCommandResult Run(string[] remainingArguments, ref DefaultCommandSettings settings)
        {
            return new DefaultCommandResult() { ExitCode = Action() };
        }
    }
}
