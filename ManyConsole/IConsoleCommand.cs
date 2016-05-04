namespace ManyConsole
{
    public interface IConsoleCommand
    {
        int Run(string[] remainingArguments);

        string Command { get; }
        string OneLineDescription { get; }
        bool IsHidden { get; }
        bool TraceCommandAfterParse { get; }
        int? RemainingArgumentsCount { get; }
        string RemainingArgumentsHelpText { get; }
        HideableOptionSet GetActualOptions();
        void CheckRequiredArguments();
        void CheckSubLevelArguments(string[] remainingArguments);
        int? OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments);

    }
}