namespace ManyConsole
{
    public interface IHelpCommand : IConsoleCommand
    {
        bool HelpExplicitlyCalled { get; set; }
    }
}