using System;
using ManyConsole;

namespace SampleConsole
{
    class HiddenCommand : ConsoleCommand<DefaultCommandResult, DefaultCommandSettings>
    {

        public HiddenCommand()
        {
            IsCommand("hidden-command", "this command is hidden so this line should NEVER BE SEEN on the console", hidden: true);

        }
        public override DefaultCommandResult Run(string[] remainingArguments, ref DefaultCommandSettings settings)
        {
            Console.WriteLine("I was hidden, yet I'm running as you see ;)");
            return new DefaultCommandResult();
        }
    }
}
