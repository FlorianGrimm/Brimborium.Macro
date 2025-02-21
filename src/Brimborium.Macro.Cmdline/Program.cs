using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Scriban;
using Scriban.Runtime;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Cmdline;

public class Program {
    static async Task Main(string[] args) {

#if false
        var template = Scriban.Template.Parse(@"
<ul id='products'>
  {{ for product in products }}
    <li>
      <h2>{{ product }}</h2>
    </li>
  {{ end }}
</ul>
", @"D:\github.com\FlorianGrimm\Brimborium.Macro\src\Brimborium.Macro.Cmdline\Program.sbncs",
new Scriban.Parsing.ParserOptions(),
new Scriban.Parsing.LexerOptions()
);
        //var result = await template.RenderAsync(new { Products = new string[] { "1", "two", "21" } }, member => member.Name);

        var root = new { Products = new string[] { "1", "two", "21" } };
        TemplateContext context = new TemplateContext();
        context.MemberRenamer = member => member.Name;
        var scriptObject = new ScriptObject();
        scriptObject.Import(
            obj: root, 
            filter: null,
            renamer: context.MemberRenamer);
        context.PushGlobal(scriptObject);
        await template.RenderAsync(context).ConfigureAwait(false);
#endif
        //context.MemberFilter
        //context.TemplateLoader = new Scriban.TemplateLoader();

        var builder = Host.CreateApplicationBuilder();
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            ;
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
        builder.Services.AddMacro(builder.Configuration.GetSection("Macro"));
        builder.Services.AddHostedService<MacroHostedService>();
        using var host = builder.Build();
        await host.RunAsync();
    }

#if false
    static async Task Main2(string[] args) {
        // Attempt to set the version of MSBuild.
        var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
        var instance = visualStudioInstances.Length == 1
            // If there is only one instance of MSBuild on this machine, set that as the one to use.
            ? visualStudioInstances[0]
            // Handle selecting the version of MSBuild you want to use.
            : SelectVisualStudioInstance(visualStudioInstances);

        System.Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

        // NOTE: Be sure to register an instance with the MSBuildLocator 
        //       before calling MSBuildWorkspace.Create()
        //       otherwise, MSBuildWorkspace won't MEF compose.
        MSBuildLocator.RegisterInstance(instance);

        using (var workspace = MSBuildWorkspace.Create()) {
            // Print message for WorkspaceFailed event to help diagnosing project load failures.
            workspace.WorkspaceFailed += (o, e) => System.Console.WriteLine(e.Diagnostic.Message);

            var solutionPath = args[0];
            System.Console.WriteLine($"Loading solution '{solutionPath}'");

            // Attach progress reporter so we print projects as they are loaded.
            var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());
            System.Console.WriteLine($"Finished loading solution '{solutionPath}'");

            // TODO: Do analysis on the projects in the loaded solution
        }
    }

    private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances) {
        System.Console.WriteLine("Multiple installs of MSBuild detected please select one:");
        for (int i = 0; i < visualStudioInstances.Length; i++) {
            System.Console.WriteLine($"Instance {i + 1}");
            System.Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
            System.Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
            System.Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
        }

        while (true) {
            var userResponse = System.Console.ReadLine();
            if (int.TryParse(userResponse, out int instanceNumber) &&
                instanceNumber > 0 &&
                instanceNumber <= visualStudioInstances.Length) {
                return visualStudioInstances[instanceNumber - 1];
            }
            System.Console.WriteLine("Input not accepted, try again.");
        }
    }
#endif

    private class ConsoleProgressReporter : IProgress<ProjectLoadProgress> {
        public void Report(ProjectLoadProgress loadProgress) {
            var projectDisplay = Path.GetFileName(loadProgress.FilePath);
            if (loadProgress.TargetFramework != null) {
                projectDisplay += $" ({loadProgress.TargetFramework})";
            }

            System.Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
        }
    }
}

internal class MacroHostedService : BackgroundService {
    private readonly BrainstormIdea _BrainstormIdea;
    private readonly SolutionService _SolutionService;
    private readonly IHostApplicationLifetime _ApplicationLifetime;
    private readonly SolutionServiceOptions _SolutionServiceOptions;

    public MacroHostedService(
        BrainstormIdea brainstormIdea,
        SolutionService solutionService,
        IOptions<SolutionServiceOptions> solutionServiceOptions,
        IHostApplicationLifetime applicationLifetime
        ) {
        this._BrainstormIdea = brainstormIdea;
        this._SolutionService = solutionService;
        this._ApplicationLifetime = applicationLifetime;
        this._SolutionServiceOptions = solutionServiceOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken ctStop) {
        if (!(_SolutionServiceOptions.SolutionFilePath is { Length: > 0 })) {
            throw new Exception("SolutionFilePath is not set.");
        }
        
        await _BrainstormIdea.OpenSolutionAsync(_SolutionServiceOptions.SolutionFilePath, ctStop);
        await _BrainstormIdea.ListenForChanges(ctStop);
        await _BrainstormIdea.UpdateAllMacros(ctStop);

        _ApplicationLifetime.StopApplication();
        return;
    }
}