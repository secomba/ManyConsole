using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ManyConsole.Internal;

namespace ManyConsole
{

    public class ConsoleModeCommand : ConsoleModeCommand<DefaultCommandResult, DefaultCommandSettings>
    {
        public ConsoleModeCommand(TextWriter outputStream = null, TextReader inputStream = null, HideableOptionSet options = null) : base(outputStream, inputStream, options)
        {
        }

        [Obsolete("Its preferred to override methods on ConsoleModeCommand and use the shorter constructor.")]

        public ConsoleModeCommand(Func<IEnumerable<IConsoleCommand<DefaultCommandResult, DefaultCommandSettings>>> commandSource, TextWriter outputStream = null, TextReader inputStream = null, string friendlyContinueText = null, HideableOptionSet options = null) : base(commandSource, outputStream, inputStream, friendlyContinueText, options)
        {
        }
    }
    public class ConsoleModeCommand<TResult, TSettings> : ConsoleCommand<TResult, TSettings> where TResult : ICommandResult, new () where TSettings : ICommandSettings, new()
    {
        private readonly TextReader _inputStream;
        private readonly TextWriter _outputStream;
        IConsoleRedirectionDetection _redirectionDetector = new ConsoleRedirectionDetection();
        public static string FriendlyContinuePrompt = "Enter a command or 'x' to exit or '?' for help";
        readonly Func<IEnumerable<IConsoleCommand<TResult, TSettings>>> _commandSource;
        private string _continuePrompt;

        public ConsoleModeCommand(
            TextWriter outputStream = null,
            TextReader inputStream = null,
            HideableOptionSet options = null)
            : this(() => new ConsoleCommand<TResult, TSettings>[0], outputStream, inputStream, null, options)
        {
            _commandSource = () => new ConsoleCommand<TResult, TSettings>[0];
        }

        [Obsolete("Its preferred to override methods on ConsoleModeCommand and use the shorter constructor.")]
        public ConsoleModeCommand(
            Func<IEnumerable<IConsoleCommand<TResult, TSettings>>> commandSource,
            TextWriter outputStream = null,
            TextReader inputStream = null,
            string friendlyContinueText = null,
            HideableOptionSet options = null)
        {
            _inputStream = inputStream ?? Console.In;
            _outputStream = outputStream ?? Console.Out;

            this.IsCommand("run-console", "Run in console mode, treating each line of console input as a command.");

            this.Options = options ?? this.Options;  //  added per request from https://github.com/fschwiet/ManyConsole/issues/7

            _commandSource = () =>
            {
                var commands = commandSource();
                return commands.Where(c => !(c is ConsoleModeCommand<TResult, TSettings>));  // don't cross the beams
            };

            _continuePrompt = friendlyContinueText ?? FriendlyContinuePrompt;
        }

        /// <summary>
        /// Writes to the console to prompt the user for their next command.
        /// Is skipped if commands are being ran without user interaction.
        /// </summary>
        public virtual void WritePromptForCommands()
        {
            if (!string.IsNullOrEmpty(_continuePrompt))
                _outputStream.WriteLine(_continuePrompt);        }

        /// <summary>
        /// Runs to get the next available commands
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IConsoleCommand<TResult, TSettings>> GetNextCommands()
        {
            return _commandSource();
        }

        public override TResult Run(string[] remainingArguments, ref TSettings settings) {
            string[] args;

            bool isInputRedirected = _redirectionDetector.IsInputRedirected();

            if (!isInputRedirected)
            {
                WritePromptForCommands();
            }

            bool haveError = false;
            string input = _inputStream.ReadLine();

            while (!input.Trim().Equals("x"))
            {
                if (input.Trim().Equals("?"))
                {
                    ConsoleHelp.ShowSummaryOfCommands(GetNextCommands(), _outputStream);
                }
                else
                {
                    args = CommandLineParser.Parse(input);

                    var result = ConsoleCommandDispatcher<TResult, TSettings>.DispatchCommand(GetNextCommands().ToList(), args, new TSettings {ConsoleOut = _outputStream});
                    if (result.ExitCode != 0)
                    {
                        haveError = true;

                        if (isInputRedirected)
                            return result;
                    }
                }
                
                if (!isInputRedirected)
                {
                    _outputStream.WriteLine();

                    if (!isInputRedirected)
                    {
                        WritePromptForCommands();
                    }
                }

                input = _inputStream.ReadLine();
            }

            return haveError ? new TResult {ExitCode = -1} : new TResult { ExitCode = 0};
        }

        public void SetConsoleRedirectionDetection(IConsoleRedirectionDetection consoleRedirectionDetection)
        {
            _redirectionDetector = consoleRedirectionDetection;
        }
    }
}
