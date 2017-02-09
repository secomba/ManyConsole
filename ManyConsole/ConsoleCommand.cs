using System;
using System.Collections.Generic;
using System.Linq;
using ManyConsole.Internal;
using NDesk.Options;

namespace ManyConsole
{

    public abstract class ConsoleCommand : ConsoleCommand<DefaultCommandResult, DefaultCommandSettings> { }
    public abstract class ConsoleCommand<TResult, TSettings> : ConsoleUtil, IConsoleCommand<TResult, TSettings> where TResult : ICommandResult where TSettings : ICommandSettings
    {
        public ConsoleCommand()
        {
            OneLineDescription = "";
            Options = new HideableOptionSet();
            TraceCommandAfterParse = true;
            RemainingArgumentsCount = 0;
            RemainingArgumentsHelpText = "";
            OptionsHasd = new HideableOptionSet();
            RequiredOptions = new List<RequiredOptionRecord>();
        }

        public bool IsHidden { get; private set; }

        public string Command { get; private set; }
        public string OneLineDescription { get; private set; }
        public string LongDescription { get; private set; }
        public HideableOptionSet Options { get; protected set; }
        public bool TraceCommandAfterParse { get; private set; }
        public bool ShowHelpWithoutFurtherArgs { get; private set; }
        public int? RemainingArgumentsCount { get; private set; }
        public string RemainingArgumentsHelpText { get; private set; }
        private HideableOptionSet OptionsHasd { get; }
        private List<RequiredOptionRecord> RequiredOptions { get; }

        public ConsoleCommand<TResult, TSettings> IsCommand(string command, string oneLineDescription = "", bool hidden = false)
        {
            Command = command;
            OneLineDescription = oneLineDescription;
            IsHidden = hidden;
            return this;
        }

        public ConsoleCommand<TResult, TSettings> HasLongDescription(string longDescription)
        {
            LongDescription = longDescription;
            return this;
        }

        public ConsoleCommand<TResult, TSettings> HasAdditionalArguments(int? count = 0, string helpText = "")
        {
            RemainingArgumentsCount = count;
            RemainingArgumentsHelpText = helpText;
            return this;
        }

        public ConsoleCommand<TResult, TSettings> AllowsAnyAdditionalArguments(string helpText = "")
        {
            HasAdditionalArguments(null, helpText);
            return this;
        }

        public ConsoleCommand<TResult, TSettings> RequiresSubLevelArguments()
        {
            ShowHelpWithoutFurtherArgs = true;
            return this;
        }

        public ConsoleCommand<TResult, TSettings> SkipsCommandSummaryBeforeRunning()
        {
            TraceCommandAfterParse = false;
            return this;
        }

        public ConsoleCommand<TResult, TSettings> HasOption(string prototype, string description, Action<string> action, bool hidden = false)
        {
            OptionsHasd.Add(prototype, description, action, hidden);

            return this;
        }

        public ConsoleCommand<TResult, TSettings> HasRequiredOption(string prototype, string description, Action<string> action)
        {
            HasRequiredOption<string>(prototype, description, action);

            return this;
        }

        public ConsoleCommand<TResult, TSettings> HasOption<T>(string prototype, string description, Action<T> action, bool hidden = false)
        {
            OptionsHasd.Add(prototype, description, action, hidden);
            return this;
        }

        public ConsoleCommand<TResult, TSettings> HasRequiredOption<T>(string prototype, string description, Action<T> action)
        {
            var requiredRecord = new RequiredOptionRecord();

            var previousOptions = OptionsHasd.ToArray();

            OptionsHasd.Add<T>(prototype, description, s =>
            {
                requiredRecord.WasIncluded = true;
                action(s);
            });

            var newOption = OptionsHasd.Single(o => !previousOptions.Contains(o));

            requiredRecord.Name = newOption.GetNames().OrderByDescending(n => n.Length).First();

            RequiredOptions.Add(requiredRecord);

            return this;
        }

        public ConsoleCommand<TResult, TSettings> HasOption(string prototype, string description, OptionAction<string, string> action, bool hidden = false)
        {
            OptionsHasd.Add(prototype, description, action, hidden);
            return this;
        }

        public ConsoleCommand<TResult, TSettings> HasOption<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action, bool hidden = false)
        {
            OptionsHasd.Add(prototype, description, action, hidden);
            return this;
        }

        public virtual void CheckRequiredArguments()
        {
            var missingOptions = this.RequiredOptions
                .Where(o => !o.WasIncluded).Select(o => o.Name).OrderBy(n => n).ToArray();

            if (missingOptions.Any())
            {
                throw new ConsoleHelpAsException("Missing option: " + String.Join(", ", missingOptions));
            }
        }

        public virtual void CheckSubLevelArguments(string[] remainingArguments)
        {
            if (!ShowHelpWithoutFurtherArgs)
            {
                return;
            }

            if (!remainingArguments.Any())
            {
                throw new ConsoleHelpAsException("Command '" + FormatCommandName(this.Command) + "' cannot be run without any further arguments.");
            }
        }

        public virtual TResult OverrideAfterHandlingArgumentsBeforeRun(string[] remainingArguments, out bool cancel)
        {
            cancel = false;
            return default(TResult);
        }

        public abstract TResult Run(string[] remainingArguments, ref TSettings settings);

        public HideableOptionSet GetActualOptions()
        {
            var result = new HideableOptionSet();

            foreach (var option in Options)
                result.Add(option);

            foreach (var option in OptionsHasd)
                result.Add(option);

            return result;
        }
    }
}