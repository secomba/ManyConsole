using System;
using ManyConsole;

namespace SampleConsole {
    public class MultiNameCommand : ConsoleCommand {

        public MultiNameCommand()
        {
            IsCommand("oneName|anotherName|sn|thePreviousOneMeansShortName", "this help text is way to long and very annoying. lets see if my solution can break it down into human readable peaces. hell yeah, even longer now!");
        }

        public override int Run(string[] remainingArguments)
        {
            Console.WriteLine($"Look at me, I'm a command with multiple names: {Command}");
            return 0;
        }
    }
}
