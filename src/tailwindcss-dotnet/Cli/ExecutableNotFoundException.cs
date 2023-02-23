namespace Tailwindcss.DotNetTool.Cli;

public class ExecutableNotFoundException : Exception
{
    public ExecutableNotFoundException(string message) : base(message)
    {
        
    }
}