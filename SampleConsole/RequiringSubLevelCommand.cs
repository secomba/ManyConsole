using System;
using System.Diagnostics;
using ManyConsole;

namespace SampleConsole {
    class RequiringSubLevelCommand : ConsoleCommand {

        public RequiringSubLevelCommand()
        {
            IsCommand("sub-level-command|slc", "this command does not run without provided sub-level options", hidden:true);
            HasOption("a", "A suboption", s => A = s);
            HasOption("b", "B suboption", s => B = s);
            RequiresSubLevelArguments();
        }

        public string A;
        public string B;
        public override int Run(string[] remainingArguments)
        {
            Debug.Assert((A ?? B) != null);
            Console.WriteLine("I was hidden, yet I'm running as you see ;)");
            return 0;
        }
    }
}
