using System.Runtime.InteropServices;
using Tailwindcss.DotNetTool.Infrastructure;

namespace Tailwindcss.DotNetTool.Cli;

public class TailwindCli
{
    private bool _initialized;
    private string? _binPath;

    public async Task InitializeAsync(string? version = null)
    {
        string useVersion = version ?? Upstream.Version;
        string? binName = Upstream.GetNativeExecutableName();

        if (binName == null)
        {
            throw new UnsupportedPlatformException(
                $"dotnet-tailwind does not support the {RuntimeInformation.RuntimeIdentifier} platform\r\n" +
                "Please install Tailwind CSS following instructions at https://tailwindcss.com/docs/installation");
        }

        string storeFolderPath = Path.Combine(DotnetTool.InstallationFolder, "runtimes");

        _binPath = Path.GetFullPath(GetStoreBinName(binName, useVersion), storeFolderPath);

        if (!File.Exists(_binPath))
        {
            if (Directory.Exists(storeFolderPath))
            {
                // Remove all temp files if any
                foreach (string filePath in Directory.GetFiles(storeFolderPath, "*.tmp"))
                {
                    File.Delete(filePath);
                }
            }
            else
            {
                Directory.CreateDirectory(storeFolderPath);
            }

            string tempBinPath = Path.GetFullPath($"{Guid.NewGuid():N}.tmp", storeFolderPath);

            // Download native tailwind cli and save to temp file
            var client = new TailwindCliDownloader();
            await client.DownloadAsync(useVersion, binName, tempBinPath);

            // If running on a Unix-based platform give file permission to be executed
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                await ProcessUtil.ExecuteAsync("chmod", "+x " + tempBinPath);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    await ProcessUtil.ExecuteAsync("xattr", "-d com.apple.quarantine " + tempBinPath);
                }
            }

            // Rename file
            File.Move(tempBinPath, _binPath);
        }

        _initialized = true;
    }

    private string GetStoreBinName(string binName, string version)
    {
        string fileName = Path.GetFileNameWithoutExtension(binName);
        string extension = Path.GetExtension(binName) ?? "";
        version = version.Replace('.', '_');

        return $"{fileName}-{version}{extension}";
    }

    public string Executable()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Must be initialized before any other action.");
        }

        return _binPath!;
    }

    public string[] CompileCommand(string rootPath, bool debug = false)
    {
        IEnumerable<string> args = new[]
        {
            Executable(),
            "-i", Path.GetFullPath(Path.Combine("styles", "app.tailwind.css"), rootPath),
            "-o", Path.GetFullPath(Path.Combine("wwwroot", "css", "app.css"), rootPath),
            "-c", Path.GetFullPath("tailwind.config.js", rootPath),
        };

        if (!debug)
        {
            args = args.Append("--minify");
        }

        return args.ToArray();
    }

    public string[] WatchCommand(string rootPath, bool debug = false, bool poll = false)
    {
        var compileCommand = CompileCommand(rootPath, debug);

        IEnumerable<string> result = compileCommand;
        result = result.Append("-w");

        if (poll)
        {
            result = result.Append("-p");
        }

        return result.ToArray();
    }
}