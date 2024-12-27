using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;

using System.Runtime.CompilerServices;

namespace Brimborium.Macro.Cmdline;

public class Program {
    public static async Task Main(string[] args) {
        var root = await JupiterUtlity.Create().Build()
            .OpenSolutionAsync(args.FirstOrDefault()?? @"D:\github.com\FlorianGrimm\Brimborium.Macro\Sample\Sample.sln");
#if false
        var builder=Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
        builder.Services.AddOptions<WorkspaceServiceOptions>();
        builder.Services.AddSingleton<WorkspaceService>();
        var app = builder.Build();
        // NOTE: Be sure to register an instance with the MSBuildLocator 
        //       before calling MSBuildWorkspace.Create()
        //       otherwise, MSBuildWorkspace won't MEF compose.
        var workspaceService = app.Services.GetRequiredService<WorkspaceService>();
        var visualStudioInstance = app.Services.GetRequiredService<WorkspaceService>().EnsureRegisterInstance();

        //var workspaceService = new WorkspaceService();
        //var visualStudioInstance = workspaceService.EnsureRegisterInstance();
        Console.WriteLine($"Using MSBuild at '{visualStudioInstance.MSBuildPath}' to load projects.");
        

        using (var workspace = workspaceService.CreateWorkspace()) {
            // Print message for WorkspaceFailed event to help diagnosing project load failures.
            workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

            var solutionPath = args[0];
            Console.WriteLine($"Loading solution '{solutionPath}'");

            // Attach progress reporter so we print projects as they are loaded.
            var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
            Console.WriteLine($"Finished loading solution '{solutionPath}'");

            // TODO: Do analysis on the projects in the loaded solution
        }
    }

    private static VisualStudioInstance SelectHighestVisualStudioInstance(VisualStudioInstance[] visualStudioInstances) {
        var result = visualStudioInstances[0];
        for (int i = 1; i < visualStudioInstances.Length; i++) {
            var item = visualStudioInstances[i];
            if (result.Version < item.Version) {
                result = item;
            }
        }
        return result;
    }

    private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances) {
        Console.WriteLine("Multiple installs of MSBuild detected please select one:");
        for (int i = 0; i < visualStudioInstances.Length; i++) {
            Console.WriteLine($"Instance {i + 1}");
            Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
            Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
            Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
        }

        while (true) {
            var userResponse = Console.ReadLine();
            if (int.TryParse(userResponse, out int instanceNumber) &&
                instanceNumber > 0 &&
                instanceNumber <= visualStudioInstances.Length) {
                return visualStudioInstances[instanceNumber - 1];
            }
            Console.WriteLine("Input not accepted, try again.");
        }
#endif
    }

    private class ConsoleProgressReporter : IProgress<ProjectLoadProgress> {
        public void Report(ProjectLoadProgress loadProgress) {
            var projectDisplay = Path.GetFileName(loadProgress.FilePath);
            if (loadProgress.TargetFramework != null) {
                projectDisplay += $" ({loadProgress.TargetFramework})";
            }

            Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
        }
    }
}
