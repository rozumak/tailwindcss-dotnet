@echo off
FOR /f %%v IN ('dotnet --version') DO set version=%%v
set target_framework=
IF "%version:~0,2%"=="9." (set target_framework=net8.0)

IF [%target_framework%]==[] (
    echo "BUILD FAILURE: .NET 9 SDK required to run build"
    exit /b 1
)

dotnet run --project build/build.csproj -f %target_framework% -c Release -- %*
