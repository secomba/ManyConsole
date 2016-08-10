using System;

namespace ManyConsole
{
    public interface IHelpCommand : IConsoleCommand
    {
        bool HelpExplicitlyCalled { get; set; }
        bool SkipExeInExpectedUsage { get; set; }
        Exception FailureReason { get; set; }
    }
}