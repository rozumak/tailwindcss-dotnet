using System.CommandLine;
using System.CommandLine.Builder;
using Tailwindcss.DotNetTool.Cli;

namespace Tailwindcss.DotNetTool.Commands;

public class WatchCommand : ICommand
{
    public string Description => "Watch and build your Tailwind CSS on file changes";

    public string Name => "watch";

    public void Setup(CommandLineBuilder builder, AppInvocationContext appContext)
    {
        var command = new Command(Name, Description);

        var optionDebug = new Option<bool>("--debug");
        var optionPoll = new Option<bool>("--poll");
        command.AddOption(optionDebug);
        command.AddOption(optionPoll);

        builder.Command.Add(command);
        command.SetHandler(async (ctx) =>
        {
            bool debug = ctx.ParseResult.GetValueForOption(optionDebug);
            bool poll = ctx.ParseResult.GetValueForOption(optionPoll);

            int result = await Execute(appContext, debug, poll);
            ctx.ExitCode = result;
        });
    }

    public async Task<int> Execute(AppInvocationContext context, bool debug, bool poll)
    {
        CliExe cliExe = context.Cli.WatchCommand(context.GetProjectRoot(), debug, poll);

        context.Console.WriteLine("Starting Watch command...");
        context.Console.WriteLine(cliExe.ToString());

        return await cliExe.RunAsync();
    }
}