using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using Tailwindcss.DotNetTool.Commands;

namespace Tailwindcss.DotNetTool;

internal static class CommandLineBuilderExtensions
{
    public static CommandLineBuilder UseContextInitializer(
        this CommandLineBuilder builder, AppInvocationContext appContext)
    {
        builder.AddMiddleware(async (context, next) =>
        {
            // Initialize cli by downloading required runtime files if needed
            await appContext.Cli.InitializeAsync();
            await next(context);
        });

        return builder;
    }

    public static CommandLineBuilder UseProjectOption(
        this CommandLineBuilder builder, AppInvocationContext appContext)
    {
        var projectOption = new Option<string?>("--project",
            "Relative path to the project folder of the target project. Default value is the current folder.");

        builder.Command.AddGlobalOption(projectOption);

        builder.AddMiddleware(async (context, next) =>
        {
            string? projectPath = null;
            OptionResult? projectOptionResult = context.ParseResult.FindResultFor(projectOption);
            if (projectOptionResult != null)
            {
                if (context.ParseResult.Errors.Any(e => e.SymbolResult?.Symbol == projectOption))
                {
                    // Error will be show via ErrorMiddleware just passing it further
                    await next(context);
                    return;
                }

                projectPath = projectOptionResult.GetValueOrDefault<string?>();
            }

            var projectInfo = ResolveProject(projectPath);
            if (projectInfo?.ProjectRoot == null)
            {
                context.Console.Error.WriteLine("Project file was not found. Use --project option or change current folder.");
                context.ExitCode = 1;
                return;
            }

            appContext.Project = projectInfo;

            await next(context);
        });

        return builder;
    }

    private static ProjectInfo? ResolveProject(string? path)
    {
        if (path == null)
        {
            path = Directory.GetCurrentDirectory();
        }
        else
        {
            path = Path.GetFullPath(path);

            if (File.Exists(path))
            {
                // It's not a directory
                return new ProjectInfo
                {
                    ProjectFilePath = Path.GetDirectoryName(path)!,
                    ProjectRoot = path
                };
            }
        }

        if (!Directory.Exists(path))
            return null;

        var projectFile = Directory
            .EnumerateFiles(path, "*.*proj", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(f => !string.Equals(Path.GetExtension(f), ".xproj", StringComparison.OrdinalIgnoreCase));

        if (projectFile == null)
            return null;

        return new ProjectInfo
        {
            ProjectFilePath = projectFile,
            ProjectRoot = path
        };
    }

    public static CommandLineBuilder UseExecRun(
        this CommandLineBuilder builder, AppInvocationContext appContext)
    {
        builder.AddMiddleware(async (context, next) =>
        {
            // Pass control directly to tailwind cli and return after execution completed
            if (context.ParseResult.CommandResult.Command.Name == "exec")
            {
                string[] args = Environment.GetCommandLineArgs().Skip(2).ToArray();
                int result = await AppCommands.Instance.Exec.Execute(appContext.Cli, args);
                context.ExitCode = result;

                return;
            }

            await next(context);
        });

        return builder;
    }
}