using System.CommandLine;
using System.CommandLine.Builder;
using Tailwindcss.DotNetTool.Install;

namespace Tailwindcss.DotNetTool.Commands
{
    public class InstallCommand : ICommand
    {
        public string Description => "Install Tailwind CSS into the app";

        public string Name => "install";

        public void Setup(CommandLineBuilder builder, AppInvocationContext appContext)
        {
            var command = new Command(Name, Description);

            builder.Command.Add(command);
            command.SetHandler(async ctx =>
            {
                int result = await Execute(appContext);
                ctx.ExitCode = result;
            });
        }

        public async Task<int> Execute(AppInvocationContext context)
        {
            context.Console.WriteLine("Starting Install command...");

            var installer = new ProjectInstaller();
            await installer.Run(context.GetProjectRoot());

            context.Console.WriteLine("Run initial Tailwind build");

            return await AppCommands.Instance.Build.Execute(context, false);
        }
    }
}
