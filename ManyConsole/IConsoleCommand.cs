using System.Collections.Generic;

namespace ManyConsole
{
    public interface IConsoleCommand<out TResult, TSettings> where TResult : ICommandResult where TSettings : ICommandSettings
    {
        TResult Run(string[] remainingArguments, ref TSettings settings);

        string Command { get; }
        List<string> Aliases { get; }

        string OneLineDescription { get; }
        string LongDescription { get; }
        bool IsHidden { get; }
        bool TraceCommandAfterParse { get; }

        int? RemainingArgumentsCountMin { get; }
        int? RemainingArgumentsCountMax { get; }
        string RemainingArgumentsHelpText { get; }
        HideableOptionSet GetActualOptions();
        void CheckRequiredArguments();
        void CheckSubLevelArguments(string[] remainingArguments);
        TResult OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments, out bool cancel, ref TSettings settings);
    }
}