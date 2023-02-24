using Tailwindcss.DotNetTool.Infrastructure;

namespace Tailwindcss.DotNetTool.Cli
{
    public class CliExe
    {
        public string FileName { get; }

        public string? Arguments { get; }

        public string? WorkingDirectory { get; }

        public CliExe(string fileName, string? arguments, string? workingDirectory)
        {
            FileName = fileName;
            Arguments = arguments;
            WorkingDirectory = workingDirectory;
        }

        public async Task<int> RunAsync()
        {
            var result = await ProcessUtil.RunAsync(FileName, Arguments ?? "",
                workingDirectory: WorkingDirectory,
                outputDataReceived: Console.WriteLine, errorDataReceived: Console.WriteLine);
            return result.ExitCode;
        }

        public override string ToString()
        {
            return $"Execute command: {FileName} {Arguments}";
        }
    }
}
