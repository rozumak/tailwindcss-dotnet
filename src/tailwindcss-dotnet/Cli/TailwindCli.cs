using System.Runtime.InteropServices;

namespace Tailwindcss.DotNetTool.Cli;

public class UnsupportedPlatformException : Exception
{
    public UnsupportedPlatformException(string message) : base(message)
    {

    }
}

public class TailwindCli
{
    private bool _initialized;
    private string? _binPath;

    public async Task InitializeAsync()
    {
        string version = Upstream.Version;
        string? binName = Upstream.GetNativeExecutableName();

        if (binName == null)
        {
            throw new UnsupportedPlatformException(
                $"dotnet-tailwind does not support the {RuntimeInformation.RuntimeIdentifier} platform\r\n" +
                "Please install tailwindcss following instructions at https://tailwindcss.com/docs/installation");
        }

        _binPath = Path.GetFullPath(binName, DotnetTool.InstallationFolder);

        if (!File.Exists(_binPath))
        {
            //install native dependencies
            var client = new TailwindCliDownloader();
            await client.DownloadAsync(version, binName, _binPath);
        }

        //TODO: check installed version and download new

        _initialized = true;
    }

    public string Executable()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Must be initialized before any other action.");
        }

        return _binPath!;
    }

    public CliExe CompileCommand(string rootPath, bool debug = false)
    {
        IEnumerable<string> args = new[]
        {
            "-i", Path.GetFullPath("styles\\app.tailwind.css", rootPath),
            "-o", Path.GetFullPath("wwwroot\\css\\app.css", rootPath),
            "-c", Path.GetFullPath("tailwind.config.js", rootPath),
        };

        if (!debug)
        {
            args = args.Append("--minify");
        }

        return new CliExe(Executable(), string.Join(' ', args));
    }

    public CliExe WatchCommand(string rootPath, bool debug = false, bool poll = false)
    {
        var exe = CompileCommand(rootPath, debug);

        IEnumerable<string> args = new[]
        {
            exe.Arguments!,
            "-w"
        };

        if (poll)
        {
            args = args.Append("-p");
        }

        return new CliExe(exe.FileName, string.Join(' ', args));
    }
}