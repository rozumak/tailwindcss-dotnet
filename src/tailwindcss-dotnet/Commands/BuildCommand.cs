using System.CommandLine;
using System.CommandLine.Builder;
using Tailwindcss.DotNetTool.Cli;
using Tailwindcss.DotNetTool.Infrastructure;

namespace Tailwindcss.DotNetTool.Commands;

public class BuildCommand : ICommand
{
    public string Description => "Build your Tailwind CSS";

    public string Name => "build";

    public void Setup(CommandLineBuilder builder, AppInvocationContext appContext)
    {
        var command = new Command(Name, Description);

        var optionDebug = new Option<bool>("--debug");
        command.AddOption(optionDebug);

        builder.Command.Add(command);
        command.SetHandler(async ctx =>
        {
            bool debug = ctx.ParseResult.GetValueForOption(optionDebug);

            int result = await Execute(appContext, debug);
            ctx.ExitCode = result;
        });
    }

    public async Task<int> Execute(AppInvocationContext context, bool debug)
    {
        string rootPath = context.GetProjectRoot();
        var command = context.Cli.CompileCommand(rootPath, debug);

        Console.WriteLine("Starting Build command...");
        Console.WriteLine("Execute command: {0}.", string.Join(" ", command));

        return await ProcessUtil.ExecuteAsync(command, rootPath);
    }
}