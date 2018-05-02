using System;
using System.Collections.Generic;
using ManyConsole;

namespace SampleConsole
{
    public class StatefulConsoleModeCommand : ConsoleModeCommand
    {
        public int Count = 0;

        public StatefulConsoleModeCommand()
        {
            this.IsCommand("stateful", "Starts a stateful console interface that allows multiple commands to be run.");
        }

        public override void WritePromptForCommands()
        {
            Console.WriteLine("You have seen this console {0} times.", Count++);

            base.WritePromptForCommands();
        }

        public override IEnumerable<IConsoleCommand<DefaultCommandResult, DefaultCommandSettings>> GetNextCommands()
        {
            return new ConsoleCommand<DefaultCommandResult, DefaultCommandSettings>[] { new GetTimeCommand(), new MattsCommand(), new DumpEmlFilesCommand(), new DumpEmlFilesCommand() };
        }
    }
}
