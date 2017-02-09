using System;
using ManyConsole;

namespace SampleConsole
{
    public class ThrowException : ConsoleCommand
    {
        public ThrowException()
        {
            this.IsCommand("throw-exception", "Throws an exception.");
            this.HasOption("m=", "Error message to be thrown.", v => Message = v);
        }

        public string Message = "Command ThrowException threw an exception with this message.";

        public override DefaultCommandResult Run(string[] remainingArguments, ref DefaultCommandSettings settings) {
            throw new Exception(Message);
        }
    }
}
