﻿using System.Runtime.InteropServices;

namespace Tailwindcss.DotNetTool.Cli;

public class Upstream
{
    public static string Version => "v4.0.0";

    public static string? GetNativeExecutableName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.Arm64 => "tailwindcss-windows-arm64.exe",
                Architecture.X64 => "tailwindcss-windows-x64.exe",
                _ => null
            };
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