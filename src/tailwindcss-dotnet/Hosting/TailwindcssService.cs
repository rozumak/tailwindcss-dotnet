using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tailwindcss.DotNetTool.Cli;

namespace Tailwindcss.DotNetTool.Hosting;

internal class TailwindcssService : BackgroundService
{
    private readonly ILogger<TailwindcssService> _logger;

    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public TailwindcssService(ILogger<TailwindcssService> logger,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //TODO: is all should be inside try catch? When to rethrow exception witch will cause process start failure?
        try
        {
            ProjectInfo? projectInfo = DotnetTool.ResolveProject(null);

            if (projectInfo == null)
            {
                throw new ArgumentNullException(nameof(projectInfo));
            }

            var appContext = new AppInvocationContext
            {
                Project = projectInfo
            };

            await appContext.Cli.InitializeAsync();
            CliExe cliExe = appContext.Cli.WatchCommand(appContext.GetProjectRoot(), true);
            _logger.LogDebug($"Starting Watch command. {cliExe}");

            await cliExe.RunAsync(stoppingToken);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Tailwindcss watch process failed. Stopping the application...");
            _hostApplicationLifetime.StopApplication();
        }
    }
}