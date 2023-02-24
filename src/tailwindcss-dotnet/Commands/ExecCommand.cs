using System.CommandLine;
using System.CommandLine.Builder;
using Tailwindcss.DotNetTool.Cli;

namespace Tailwindcss.DotNetTool.Commands;

public class ExecCommand : ICommand
{
    public string Description => "Execute Tailwind CSS platform-specific executable with your own build options";

    public string Name => "exec";

    public void Setup(CommandLineBuilder builder, AppInvocationContext appContext)
    {
        var command = new Command(Name, Description);

        builder.Command.Add(command);
    }

    public Task<int> Execute(TailwindCli cli, string[] args)
    {
        string binPath = cli.Executable();

        var exe = new CliExe(binPath, string.Join(' ', args), null);
        return exe.RunAsync();
    }
}