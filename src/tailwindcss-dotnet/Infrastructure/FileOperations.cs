namespace Tailwindcss.DotNetTool.Infrastructure;

internal static class FileOperations
{
    public static async Task<bool> InsertBeforeAsync(string fileName, string insertText, string beforeText)
    {
        var text = await File.ReadAllTextAsync(fileName);

        int index = text.IndexOf(beforeText, StringComparison.Ordinal);
        if (index == -1)
        {
            return false;
        }

        text = text.Insert(index, insertText);
        await File.WriteAllTextAsync(fileName, text);

        return true;
    }
}