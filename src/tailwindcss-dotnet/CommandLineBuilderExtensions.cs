using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Text.RegularExpressions;
using Tailwindcss.DotNetTool.Cli;
using Tailwindcss.DotNetTool.Commands;

namespace Tailwindcss.DotNetTool;

internal static class CommandLineBuilderExtensions
{
    public static CommandLineBuilder UseContextInitializer(
        this CommandLineBuilder builder, AppInvocationContext appContext)
    {
        const string pattern = @"^v3\.\d+\.\d+$";

        string? tailwindVersion = null;
        var tailwindVersionOption = new Option<string?>("--tailwindcss",
            "Specify tailwind cli version to use in format 'v3.x.x'.");
        builder.Command.AddGlobalOption(tailwindVersionOption);

        builder.AddMiddleware(async (context, next) =>
        {
            OptionResult? optionResult = context.ParseResult.FindResultFor(tailwindVersionOption);
            if (optionResult != null)
            {
                if (context.ParseResult.Errors.Any(e => e.SymbolResult?.Symbol == tailwindVersionOption))
                {
                    // Error will be show via ErrorMiddleware just passing it further
                    await next(context);
                    return;
                }

                tailwindVersion = optionResult.GetValueOrDefault<string?>();
                if (string.IsNullOrWhiteSpace(tailwindVersion) || !Regex.IsMatch(tailwindVersion, pattern))
                {
                    context.Console.Error.WriteLine("Invalid Tailwind CSS version format.");
                    context.ExitCode = 1;

                    return;
                }
            }

            try
            {
                // Initialize cli by downloading required runtime files if needed
                await appContext.Cli.InitializeAsync(tailwindVersion);
            }
            catch (Exception e) when (e is ExecutableNotFoundException ||
                                      e is UnsupportedPlatformException)
            {
                context.Console.Error.WriteLine(e.Message);
                context.ExitCode = 1;
                return;
            }

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

            var projectInfo = DotnetTool.ResolveProject(projectPath);
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

    public static CommandLineBuilder UseExecRun(
        this CommandLineBuilder builder, AppInvocationContext appContext)
    {
        builder.AddMiddleware(async (context, next) =>
        {
            // Pass control directly to tailwind cli and return after execution completed
            if (context.ParseResult.CommandResult.Command.Name == "exec")
            {
                // Project option is ignored
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