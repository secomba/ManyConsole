﻿using System.Collections.Generic;
using System.Linq;
using ManyConsole;

namespace SampleConsole
{
    public class SimpleConsoleModeCommand : ConsoleModeCommand
    {
        public SimpleConsoleModeCommand()
        {
            this.IsCommand("console-mode", "Starts a console interface that allows multiple commands to be run.");
        }

        public override IEnumerable<IConsoleCommand<DefaultCommandResult, DefaultCommandSettings>> GetNextCommands()
        {
            return Program.GetCommands().Where(c => !(c is ConsoleModeCommand<DefaultCommandResult, DefaultCommandSettings>)) as IEnumerable<IConsoleCommand<DefaultCommandResult, DefaultCommandSettings>>;
        }
    }
}
