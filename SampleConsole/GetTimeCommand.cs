using System;
using ManyConsole;

namespace SampleConsole
{
    /// <summary>
    /// As an example of ManyConsole usage, get-time is meant to show the simplest case possible usage.
    /// </summary>
    public class GetTimeCommand : ConsoleCommand<DefaultCommandResult, DefaultCommandSettings>
    {
        public GetTimeCommand()
        {
            this.IsCommand("get-time", "Returns the current system time.");
        }

        public override DefaultCommandResult Run(string[] remainingArguments, ref DefaultCommandSettings settings)
        {
            Console.WriteLine(DateTime.UtcNow);

            return new DefaultCommandResult();
        }
    }
}
