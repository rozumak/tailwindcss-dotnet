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

        public async Task<int> RunAsync(CancellationToken cancellationToken = default)
        {
            ProcessSpec procSpec = new ProcessSpec(FileName)
            {
                WorkingDirectory = WorkingDirectory,
                Arguments = Arguments ?? "",
                OnOutputData = Console.Out.Write,
                OnErrorData = Console.Error.Write,
                InheritEnv = false,
            };

            var (resultTask, processDisposable) = ProcessUtil.Run(procSpec);

            cancellationToken.Register(() =>
            {
                // don't need to wait for actual disposal completion because finished task indicates it's completed
                try
                {
                    processDisposable.DisposeAsync();
                }
                catch (Exception) { }
            });
            
            return (await resultTask).ExitCode;
        }

        public override string ToString()
        {
            return $"Execute command: {FileName} {Arguments}";
        }
    }
}
