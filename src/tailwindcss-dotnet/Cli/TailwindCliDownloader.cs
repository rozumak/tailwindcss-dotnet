﻿using System.Diagnostics;
using System.Net;
using System.Text;

namespace Tailwindcss.DotNetTool.Cli;

public class TailwindCliDownloader
{
    public async Task DownloadAsync(string version, string binName, string saveBinPath)
    {
        string url = $"https://github.com/tailwindlabs/tailwindcss/releases/download/{version}/{binName}";

        Console.WriteLine($"Downloading tailwind cli {version} from {url}.");

        try
        {
            await DownloadFileAsync(url, saveBinPath);
        }
        catch
        {
            Console.WriteLine("Failed to download and save native tailwind cli.");
            throw;
        }
    }

    private async Task DownloadFileAsync(string url, string saveBinPath)
    {
        using HttpClient client = new HttpClient();
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new ExecutableNotFoundException($"Cannot find the Tailwind CSS executable via url {url}.");
        }

        response.EnsureSuccessStatusCode();

        long? contentLength = response.Content.Headers.ContentLength;
        var reporter = new ConsoleProgressReporter(contentLength);

        await using var stream = await client.GetStreamAsync(url);

        try
        {
            await using var output = File.Open(saveBinPath, FileMode.Create);
            await CopyToAsync(stream, output, 4096 * 100, reporter);

            reporter.Report(output.Length, true);

            await output.FlushAsync();
        }
        catch
        {
            // Silently try remove created temp file when failed to download
            try
            {
                File.Delete(saveBinPath);
            }
            catch
            {
                // ignored
            }

            throw;
        }
    }

    static async Task CopyToAsync(Stream source, Stream destination, int bufferSize, ConsoleProgressReporter? reporter)
    {
        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead);
            totalBytesRead += bytesRead;
            reporter?.Report(totalBytesRead);
        }
    }

    private class ConsoleProgressReporter
    {
        private readonly long? _totalSize;
        private readonly Stopwatch _stopwatch;

        private int _lastMessageSize;

        public ConsoleProgressReporter(long? totalSize)
        {
            _totalSize = totalSize;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Report(long totalBytesRead, bool final = false)
        {
            if (!final && !Console.IsOutputRedirected)
            {
                double speed = CalculateSpeed();

                if (_totalSize.HasValue)
                {
                    double percentage = (double)totalBytesRead / _totalSize.Value * 100;
                    WriteCore(
                        $"Downloaded {totalBytesRead} bytes out of {_totalSize} ({percentage:F2}%), speed: {speed:F2} MB/s");
                }
                else
                {
                    WriteCore($"Downloaded {totalBytesRead} bytes, speed: {speed:F2} MB/s");
                }
            }
            else
            {
                double speed = CalculateSpeed();
                _stopwatch.Stop();

                WriteCore(
                    $"Download completed in {_stopwatch.Elapsed.TotalSeconds:F2} seconds, average speed: {speed:F2} MB/s",
                    true);
            }

            double CalculateSpeed()
            {
                // Calculate speed in MB/s
                double speed = totalBytesRead / _stopwatch.Elapsed.TotalSeconds / 1024 / 1024;
                return speed;
            }
        }

        private void WriteCore(string message, bool newLine = false)
        {
            int overlapCount = _lastMessageSize - message.Length;
            if (overlapCount > 0)
            {
                var builder = new StringBuilder(message);
                builder.Append(' ', overlapCount);

                message = builder.ToString();
            }

            _lastMessageSize = message.Length;

            if (newLine)
            {
                message += Environment.NewLine;
            }

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(message);
        }
    }
}