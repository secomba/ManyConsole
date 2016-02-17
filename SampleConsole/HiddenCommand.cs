using System;
using ManyConsole;

namespace SampleConsole {
    class HiddenCommand : ConsoleCommand {

        public HiddenCommand()
        {
            IsCommand("hidden-command", "this command is hidden so this line should NEVER BE SEEN on the console", hidden:true);

        }
        public override int Run(string[] remainingArguments)
        {
            Console.WriteLine("I was hidden, yet I'm running as you see ;)");
            return 0;
        }
    }
}
