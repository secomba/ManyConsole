﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ManyConsole.Tests.ConsoleModeCommandSpecs
{
    public class Should_interpret_quotes_properly : ConsoleModeCommandSpecification
    {
        public class AccumulateStringsCommand : ConsoleCommand<DefaultCommandResult, DefaultCommandSettings>
        {
            public List<string> Marked = new List<string>();
            public List<string> Unmarked = new List<string>();

            public AccumulateStringsCommand()
            {
                this.IsCommand("accumulate-strings", "Accumulates strings.");
                this.HasOption("s=", "A string.", v => Marked.Add(v));
                this.AllowsAnyAdditionalArguments("And more strings");
            }

            public override DefaultCommandResult Run(string[] remainingArguments, ref DefaultCommandSettings settings)
            {
                Unmarked.AddRange(remainingArguments);
                return new DefaultCommandResult();
            }
        }

        public override void Specify()
        {
            when("a command is ran with quoted input", delegate ()
                {
                    var command = new AccumulateStringsCommand();

                    arrange(RunConsoleModeCommand(new string[]
                        {
                            "accumulate-strings -s \"one two three\" \"four five six\"",
                            "x",
                        },
                        inputIsFromUser: true, command: command));

                    then("the output contains a helpful prompt", delegate
                    {
                        Assert.That(command.Marked, Is.EquivalentTo(new[] { "one two three" }));
                        Assert.That(command.Unmarked, Is.EquivalentTo(new[] { "four five six" }));
                    });
                });

        }
    }
}
