using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManyConsole.Tests
{
    public class TestCommand : ConsoleCommand
    {
        public Func<int> Action = delegate { return 0; };

        public override DefaultCommandResult Run<TSettings>(string[] remainingArguments, ref TSettings settings)
        {
            return new DefaultCommandResult() {ExitCode = Action()};
        }
    }
}
