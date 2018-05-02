using System;
using ManyConsole;

namespace SampleConsole
{
    public class MattsCommand : ConsoleCommand<DefaultCommandResult, DefaultCommandSettings>
    {
        public string Baz;

        public MattsCommand()
        {
            this.IsCommand("matts");
            this.HasOption("b|baz=", "baz", v => Baz = v);
            AllowsAnyAdditionalArguments("<foo1> <foo2> <fooN> where N is bar");
        }
        public override DefaultCommandResult Run(string[] remainingArguments, ref DefaultCommandSettings settings)
        {
            Console.WriteLine("baz is " + (Baz ?? "<null>"));
            Console.WriteLine("foos are: " + String.Join(", ", remainingArguments));
            return new DefaultCommandResult();
        }
    }
}
