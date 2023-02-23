namespace Tailwindcss.DotNetTool.Cli;

public class UnsupportedPlatformException : Exception
{
    public UnsupportedPlatformException(string message) : base(message)
    {

    }
}