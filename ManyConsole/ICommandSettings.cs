using System.IO;

namespace ManyConsole
{
    public interface ICommandSettings
    {
        TextWriter ConsoleOut { get; set; }
        bool SkipExeInExpectedUsage { get; }
        bool GloballyDisableTraceCommandAfterParse { get; }
    }


    public class DefaultCommandSettings : ICommandSettings
    {
        public DefaultCommandSettings()
        {
        }
        public DefaultCommandSettings(TextWriter consoleOut) {
            ConsoleOut = consoleOut;
        }
        public TextWriter ConsoleOut { get; set; }
        public bool SkipExeInExpectedUsage { get; set; }
        public bool GloballyDisableTraceCommandAfterParse { get; set; }
    }
}