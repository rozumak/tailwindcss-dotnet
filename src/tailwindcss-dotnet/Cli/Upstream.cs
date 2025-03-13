using System.Runtime.InteropServices;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tailwindcss.DotNetTool.Cli;

public class Upstream
{
    private static readonly HttpClient httpClient = new HttpClient();
    private record Release
    {
        [JsonPropertyName("tag_name")] public string Version { get; set; }
        [JsonPropertyName("prerelease")] public bool IsPrerelease { get; set; }
        [JsonPropertyName("published_at")] public DateTime PublishedAt { get; set; }

    }
    public static async Task<string> GetLatestReleaseVersionAsync()
    {
        var fallbackVersion = "v4.0.12";
        try
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("dotnet-tailwindcss");
            var response = await httpClient.GetAsync("https://api.github.com/repos/tailwindlabs/tailwindcss/releases");
            response.EnsureSuccessStatusCode();



            var responseBody = await response.Content.ReadAsStringAsync();
            var releases = JsonSerializer.Deserialize<Release[]>(responseBody);
            var latestMinorRelease =
                releases?
                    .Where(r => !r.IsPrerelease)
            .Where(r => r.Version.StartsWith("v4."))
            .OrderByDescending(r => r.PublishedAt)
            .FirstOrDefault();
            return latestMinorRelease?.Version ?? fallbackVersion;
        }

        catch
        {
            return fallbackVersion;
        }
    }
    private static readonly Lazy<string> _version = new(() => GetLatestReleaseVersionAsync().GetAwaiter().GetResult());
    public static string Version => _version.Value;

    public static string? GetNativeExecutableName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return RuntimeInformation.ProcessArchitecture is Architecture.Arm64 or Architecture.X64 ? "tailwindcss-windows-x64.exe" : null;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.Arm64 => "tailwindcss-macos-arm64",
                Architecture.X64 => "tailwindcss-macos-x64",
                _ => null
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.Arm64 => "tailwindcss-linux-arm64",
                Architecture.Arm => "tailwindcss-linux-armv7",
                Architecture.X64 => "tailwindcss-linux-x64",
                _ => null
            };
        }

        return null;
    }
}