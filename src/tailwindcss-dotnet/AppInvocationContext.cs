using System.CommandLine;
using System.CommandLine.IO;
using Tailwindcss.DotNetTool.Cli;

namespace Tailwindcss.DotNetTool;

public class AppInvocationContext
{
    public TailwindCli Cli { get; } = new ();

    public IConsole Console { get; } = new SystemConsole();

    public ProjectInfo? Project { get; set; }

    public string GetProjectRoot()
    {
        if (Project == null || Project.ProjectRoot == null)
            throw new InvalidOperationException("Project info is not initialized.");

        return Project.ProjectRoot;
    }
}