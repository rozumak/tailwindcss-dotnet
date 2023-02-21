namespace Tailwindcss.DotNetTool;

public class DotnetTool
{
    public static string InstallationFolder { get; }

    static DotnetTool()
    {
        InstallationFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
    }
}