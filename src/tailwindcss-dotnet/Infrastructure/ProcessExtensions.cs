namespace Tailwindcss.DotNetTool.Infrastructure;

internal static partial class ProcessUtil
{
    public static Task<int> ExecuteAsync(string[] command, string? workingDirectory = null)
    {
        return ExecuteAsync(command[0], string.Join(" ", command.Skip(0)), workingDirectory);
    }

    public static async Task<int> ExecuteAsync(string fileName, string? arguments = null, string? workingDirectory = null)
    {
        ProcessSpec procSpec = new ProcessSpec(fileName)
        {
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
            Arguments = arguments ?? "",
            OnOutputData = Console.Out.Write,
            OnErrorData = Console.Error.Write,
            InheritEnv = false,
        };

        var (result, _) = ProcessUtil.Run(procSpec);
        return (await result).ExitCode;
    }
}