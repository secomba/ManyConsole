using System;

namespace ManyConsole
{

    public interface IHelpCommand<out TResult, TSettings> : IConsoleCommand<TResult, TSettings> where TResult : ICommandResult where TSettings : ICommandSettings
    {
        bool HelpExplicitlyCalled { get; set; }
        bool SkipExeInExpectedUsage { get; set; }
        Exception FailureReason { get; set; }
    }
}