using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tailwindcss.DotNetTool.Infrastructure;

namespace Tailwindcss.DotNetTool.Hosting;

internal class TailwindcssHostedService : IHostedService
{
    private readonly ILogger<TailwindcssHostedService> _logger;

    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private IAsyncDisposable? _tailwindDisposable;

    public TailwindcssHostedService(ILogger<TailwindcssHostedService> logger,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Blocking app start until all deps are downloaded and initialized
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

        EnsureTailwindProcessRunning(appContext);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping tailwind...");

        if (_tailwindDisposable is { } disposable)
        {
            _tailwindDisposable = null;

            try
            {
                await disposable.DisposeAsync().AsTask().ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Failed with timeout, ignore
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Tailwindcss process termination failed with an error.");
            }
        }
    }

    private void EnsureTailwindProcessRunning(AppInvocationContext appContext)
    {
        var watchCommand = appContext.Cli.WatchCommand(appContext.GetProjectRoot(), true);
        _logger.LogDebug("Starting tailwind. Execute command: {Command}.", string.Join(" ", watchCommand));

        var procSpec = CreateTailwindProcSpec(watchCommand, appContext.GetProjectRoot());
        (var resultTask, _tailwindDisposable) = ProcessUtil.Run(procSpec);

        resultTask.ContinueWith(_ =>
        {
            if (!_hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                _logger.LogCritical("Tailwind process died. Stopping the application...");
                _hostApplicationLifetime.StopApplication();
            }
        });
    }

    private ProcessSpec CreateTailwindProcSpec(string[] command, string workingDirectory)
    {
        ProcessSpec procSpec = new ProcessSpec(command[0])
        {
            WorkingDirectory = workingDirectory,
            Arguments = string.Join(" ", command.Skip(0)),
            OnOutputData = Console.Out.Write,
            OnErrorData = Console.Error.Write,
            InheritEnv = false,
            ThrowOnNonZeroReturnCode = false,
        };

        return procSpec;
    }
}