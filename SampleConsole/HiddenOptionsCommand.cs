using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManyConsole;

namespace SampleConsole {
    public class HiddenOptionsCommand: ConsoleCommand {
        
        public bool NormalOption { get; set; }
        public bool HiddenOption { get; set; }
        public string AnotherHiddenOption { get; set; }

        public HiddenOptionsCommand()
        {
            IsCommand("hio|hidden-options-command");
            HasOption("normal-option|n", "this is a normal visible option", s => NormalOption = true);
            HasOption("hidden-option|h", "this is a hidden option. you shouldn't see this text :D", s => HiddenOption = true, hidden:true);
            HasOption("another-hidden-option|a=", "this is another hidden option. you shouldn't see this text :D", s => AnotherHiddenOption = s, hidden:true);

        }
        public override int Run(string[] remainingArguments)
        {
            Console.WriteLine($"NormalOption: {NormalOption}, HiddenOption: {HiddenOption}, AnotherHiddenOption: {AnotherHiddenOption}");
            return 0;
        }
    }
}
