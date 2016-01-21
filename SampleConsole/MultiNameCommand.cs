using System;
using ManyConsole;

namespace SampleConsole {
    public class MultiNameCommand : ConsoleCommand {

        public MultiNameCommand()
        {
            IsCommand("oneName|anotherName|sn|thePreviousOneMeansShortName");
        }

        public override int Run(string[] remainingArguments)
        {
            Console.WriteLine($"Look at me, I'm a command with multiple names: {Command}");
            return 0;
        }
    }
}
