namespace Tailwindcss.DotNetTool;

public class DotnetTool
{
    public static string InstallationFolder { get; }

    static DotnetTool()
    {
        InstallationFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
    }

    public static ProjectInfo? ResolveProject(string? path)
    {
        if (path == null)
        {
            path = Directory.GetCurrentDirectory();
        }
        else
        {
            path = Path.GetFullPath(path);

            if (File.Exists(path))
            {
                // It's not a directory
                return new ProjectInfo
                {
                    ProjectFilePath = Path.GetDirectoryName(path)!,
                    ProjectRoot = path
                };
            }
        }

        if (!Directory.Exists(path))
            return null;

        var projectFile = Directory
            .EnumerateFiles(path, "*.*proj", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(f => !string.Equals(Path.GetExtension(f), ".xproj", StringComparison.OrdinalIgnoreCase));

        if (projectFile == null)
            return null;

        return new ProjectInfo
        {
            ProjectFilePath = projectFile,
            ProjectRoot = path
        };
    }
}