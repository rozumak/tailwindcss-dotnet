﻿// Copied from project Aspire with small modifications
// https://github.com/dotnet/aspire/blob/main/src/Aspire.Hosting/Dcp/Process/ProcessResult.cs
// https://github.com/dotnet/aspire/blob/main/src/Aspire.Hosting/Dcp/Process/ProcessSpec.cs
// https://github.com/dotnet/aspire/blob/main/src/Aspire.Hosting/Dcp/Process/ProcessUtil.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tailwindcss.DotNetTool.Infrastructure;

internal sealed class ProcessResult
{
    public ProcessResult(int exitCode)
    {
        ExitCode = exitCode;
    }

    public int ExitCode { get; }
}

internal sealed class ProcessSpec
{
    public string ExecutablePath { get; }
    public string? WorkingDirectory { get; init; }
    public IDictionary<string, string> EnvironmentVariables { get; init; } = new Dictionary<string, string>();
    public bool InheritEnv { get; init; } = true;
    public string? Arguments { get; init; }
    public Action<string>? OnOutputData { get; init; }
    public Action<string>? OnErrorData { get; init; }
    public Action<int>? OnStart { get; init; }
    public Action<int>? OnStop { get; init; }
    public bool KillEntireProcessTree { get; init; } = true;
    public bool ThrowOnNonZeroReturnCode { get; init; } = true;

    public ProcessSpec(string executablePath)
    {
        ExecutablePath = executablePath;
    }
}

internal static partial class ProcessUtil
{
    #region Native Methods

    [DllImport("libc", SetLastError = true, EntryPoint = "kill")]
    private static extern int sys_kill(int pid, int sig);

    #endregion

    private static readonly TimeSpan s_processExitTimeout = TimeSpan.FromSeconds(5);

    public static (Task<ProcessResult>, IAsyncDisposable) Run(ProcessSpec processSpec)
    {
        var process = new System.Diagnostics.Process()
        {
            StartInfo =
            {
                FileName = processSpec.ExecutablePath,
                WorkingDirectory = processSpec.WorkingDirectory ?? string.Empty,
                Arguments = processSpec.Arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            },
            EnableRaisingEvents = true
        };

        if (!processSpec.InheritEnv)
        {
            process.StartInfo.Environment.Clear();
        }

        foreach (var (key, value) in processSpec.EnvironmentVariables)
        {
            process.StartInfo.Environment[key] = value;
        }

        // Use a reset event to prevent output processing and exited events from running until OnStart is complete.
        // OnStart might have logic that sets up data structures that then are used by these events.
        var startupComplete = new ManualResetEventSlim(false);

        // Note: even though the child process has exited, its children may be alive and still producing output.
        // See https://github.com/dotnet/runtime/issues/29232#issuecomment-1451584094 for how this might affect waiting for process exit.
        // We are going to discard that (grandchild) output by checking process.HasExited.

        if (processSpec.OnOutputData != null)
        {
            process.OutputDataReceived += (_, e) =>
            {
                startupComplete.Wait();

                if (String.IsNullOrEmpty(e.Data))
                {
                    return;
                }

                processSpec.OnOutputData.Invoke(e.Data);
            };
        }

        if (processSpec.OnErrorData != null)
        {
            process.ErrorDataReceived += (_, e) =>
            {
                startupComplete.Wait();
                if (String.IsNullOrEmpty(e.Data))
                {
                    return;
                }

                processSpec.OnErrorData.Invoke(e.Data);
            };
        }

        var processLifetimeTcs = new TaskCompletionSource<ProcessResult>();

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            processSpec.OnStart?.Invoke(process.Id);

            process.WaitForExitAsync().ContinueWith(t =>
            {
                startupComplete.Wait();

                if (processSpec.ThrowOnNonZeroReturnCode && process.ExitCode != 0)
                {
                    processLifetimeTcs.TrySetException(new InvalidOperationException(
                        $"Command {processSpec.ExecutablePath} {processSpec.Arguments} returned non-zero exit code {process.ExitCode}"));
                }
                else
                {
                    processLifetimeTcs.TrySetResult(new ProcessResult(process.ExitCode));
                }
            }, TaskScheduler.Default);
        }
        finally
        {
            startupComplete.Set(); // Allow output/error/exit handlers to start processing data.
        }

        return (processLifetimeTcs.Task, new ProcessDisposable(process, processLifetimeTcs.Task, processSpec.KillEntireProcessTree));
    }

    private sealed class ProcessDisposable : IAsyncDisposable
    {
        private readonly System.Diagnostics.Process _process;
        private readonly Task _processLifetimeTask;
        private readonly bool _entireProcessTree;

        public ProcessDisposable(System.Diagnostics.Process process, Task processLifetimeTask, bool entireProcessTree)
        {
            _process = process;
            _processLifetimeTask = processLifetimeTask;
            _entireProcessTree = entireProcessTree;
        }

        public async ValueTask DisposeAsync()
        {
            if (_process.HasExited)
            {
                return; // nothing to do
            }

            if (OperatingSystem.IsWindows())
            {
                if (!_process.CloseMainWindow())
                {
                    _process.Kill(_entireProcessTree);
                }
            }
            else
            {
                sys_kill(_process.Id, sig: 2); // SIGINT
            }

            await _processLifetimeTask.WaitAsync(s_processExitTimeout).ConfigureAwait(false);
            if (!_process.HasExited)
            {
                // Always try to kill the entire process tree here if all of the above has failed.
                _process.Kill(entireProcessTree: true);
            }
        }
    }
}