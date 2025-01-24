using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build
{
    internal class Build
    {
        private const string InstallMinver = "install-minver";
        private const string Minver = "minver";

        private const string Clean = "clean";
        private const string Restore = "restore";
        private const string Compile = "compile";
        private const string Test = "test";

        private const string Pack = "pack";
        private const string Publish = "publish";

        private static async Task Main(string[] args)
        {
            const string solutionName = "Tailwindcss.DotnetTool.sln";

            Target("default", DependsOn(Compile));

            Target(InstallMinver, IgnoreIfFailed(() =>
            {
                Run("dotnet", "tool install --global minver-cli --version 4.2.0");
            }));

            string? version = null;
            Target(Minver, DependsOn(InstallMinver), async () =>
            {
                (version, _) = await ReadAsync("minver", "-t v");
                Console.WriteLine("Version: {0}", version);
            });

            Target(Clean, () =>
            {
                EnsureDirectoriesDeleted("./artifacts");
                Run("dotnet", $"clean {solutionName}");
            });

            Target(Restore, () =>
            {
                Run("dotnet", $"restore {solutionName}");
            });

            Target(Compile, DependsOn(Restore), () =>
            {
                Run("dotnet",
                    $"build {solutionName} --no-restore");
            });

            Target("ci", DependsOn("default"));

            string[] nugetProjects = {
                "./src/tailwindcss-dotnet"
            };

            Target(Pack, DependsOn(Compile, Minver), ForEach(nugetProjects), project =>
                Run("dotnet", $"pack {project} -o ./artifacts --configuration Release -p:PackageVersion={version}"));

            await RunTargetsAndExitAsync(args);
        }

        private static void EnsureDirectoriesDeleted(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    DeleteDirectory(dir);
                }
            }
        }

        private static void DeleteDirectory(DirectoryInfo baseDir)
        {
            baseDir.Attributes = FileAttributes.Normal;
            foreach (var childDir in baseDir.GetDirectories())
                DeleteDirectory(childDir);

            foreach (var file in baseDir.GetFiles())
                file.IsReadOnly = false;

            baseDir.Delete(true);
        }

        private static Action IgnoreIfFailed(Action action)
        {
            return () =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            };
        }
    }
}
