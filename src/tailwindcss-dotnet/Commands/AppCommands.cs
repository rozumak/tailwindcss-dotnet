namespace Tailwindcss.DotNetTool.Commands;

public class AppCommands
{
    public static AppCommands Instance { get; } = new();

    public ICommand[] All { get; }

    public InstallCommand Install { get; } = new();

    public BuildCommand Build { get; } = new();

    public WatchCommand Watch { get; } = new();

    public ExecCommand Exec { get; } = new();

    private AppCommands()
    {
        All = new ICommand[] {Install, Build, Watch, Exec};
    }
}