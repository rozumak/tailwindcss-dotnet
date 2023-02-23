using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Tailwindcss.DotNetTool;
using Tailwindcss.DotNetTool.Cli;
using Tailwindcss.DotNetTool.Commands;

var appContext = new AppInvocationContext();
CommandLineBuilder builder = new CommandLineBuilder
{
    Command =
    {
        Description = $"A dotnet tool for installing and invoking Tailwind CSS. Default version of Tailwind CSS CLI is {Upstream.Version}"
    }
};
foreach (var command in AppCommands.Instance.All)
{
    command.Setup(builder, appContext);
}

builder.UseDefaults();

builder.UseContextInitializer(appContext);
builder.UseExecRun(appContext);
builder.UseProjectOption(appContext);

var parser = builder.Build();

return await parser.InvokeAsync(args, appContext.Console);
