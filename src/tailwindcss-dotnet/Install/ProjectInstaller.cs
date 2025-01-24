using System.Reflection;
using Tailwindcss.DotNetTool.Infrastructure;

namespace Tailwindcss.DotNetTool.Install;

public class ProjectInstaller
{
    private readonly Assembly _executingAssembly = Assembly.GetExecutingAssembly();

    public async Task Run(string projectRoot)
    {
        var tailwindCss = Path.GetFullPath(Path.Combine("styles", "app.tailwind.css"), projectRoot);
        if (!File.Exists(tailwindCss))
        {
            Console.WriteLine("Add default style\\app.tailwind.css");

            Directory.CreateDirectory(Path.GetDirectoryName(tailwindCss)!);
            await CopyTo("app.tailwind.css", tailwindCss);
        }

        string? layoutFile = FindDefaultLayoutPath(projectRoot);
        string stylesheetLink = @"<link rel=""stylesheet"" href=""~/css/app.css""/>";

        if (layoutFile != null)
        {
            //TODO: check if already installed in layout?
            //TODO: change layout file to contain tailwind??
            await FileOperations.InsertBeforeAsync(layoutFile,
                stylesheetLink + Environment.NewLine + Indent(4), "<link");
        }
        else
        {
            Console.WriteLine("Default layout file is missing!");
            Console.WriteLine($"Add {stylesheetLink} within the <head> tag in your custom layout.");
        }

        //TODO: do we need delete this file? we can't empty directory
        string builtCss = Path.GetFullPath(Path.Combine("wwwroot", "css", "app.css"), projectRoot);
        if (File.Exists(builtCss))
        {
            Console.WriteLine("Clean wwwroot\\css");
            File.Delete(builtCss);
        }

        //TODO: write .gitignore to ignore style\\tailwind.css file?
    }

    private async Task CopyTo(string sourceName, string destFileName)
    {
        var resourceName = $"Tailwindcss.DotNetTool.Install.{sourceName}";

        await using Stream? sourceStream = _executingAssembly.GetManifestResourceStream(resourceName);
        if (sourceStream == null)
        {
            throw new Exception($"Cant find '{sourceName}' in embedded resources.");
        }

        await using FileStream destStream = new FileStream(destFileName, FileMode.OpenOrCreate);
        await sourceStream.CopyToAsync(destStream);
    }

    private string? FindDefaultLayoutPath(string projectRoot)
    {
        // "razor" template
        var layoutPath = Path.GetFullPath(Path.Combine("Pages", "Shared", "_Layout.cshtml"), projectRoot);
        if (File.Exists(layoutPath))
        {
            return layoutPath;
        }

        // "mvc" template
        layoutPath = Path.GetFullPath(Path.Combine("Views", "Shared", "_Layout.cshtml"), projectRoot);
        if (File.Exists(layoutPath))
        {
            return layoutPath;
        }

        // "blazorserver" template
        layoutPath = Path.GetFullPath(Path.Combine("Pages", "_Host.cshtml"), projectRoot);
        if (File.Exists(layoutPath))
        {
            return layoutPath;
        }

        // It's named same way don't need to include app.css reference in layout
        // "blazorwasm" template
        //layoutPath = Path.GetFullPath(Path.Combine("wwwroot", "index.html"), projectRoot);
        //if (File.Exists(layoutPath))
        //{
        //    return layoutPath;
        //}

        return null;
    }

    private static string Indent(int count)
    {
        var indent = new string(' ', count);
        return indent;
    }
}