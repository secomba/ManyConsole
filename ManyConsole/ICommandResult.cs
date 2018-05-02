namespace ManyConsole
{
    public interface ICommandResult
    {
        int ExitCode { get; set; }
    }

    public class DefaultCommandResult : ICommandResult
    {
        public int ExitCode { get; set; }
    }
}