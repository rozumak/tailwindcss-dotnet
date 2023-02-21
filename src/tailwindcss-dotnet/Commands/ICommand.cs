using System.CommandLine.Builder;

namespace Tailwindcss.DotNetTool.Commands;

public interface ICommand
{
    public string Description { get; }

    public string Name { get; }

    public void Setup(CommandLineBuilder builder, AppInvocationContext appContext);
}