#pragma warning disable IDE0350 // Use implicitly typed lambda

using Bullseye;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Brimborium.Macro.Service;

namespace Brimborium.Macro.CliLibrary;

/// <summary>
/// Represents the main application for the Macro CLI.
/// </summary>
public class MacroApplication {
    /// <summary>
    /// Gets the targets for the application.
    /// </summary>
    public Targets AppTargets { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroApplication"/> class.
    /// </summary>
    public MacroApplication() {
        this.AppTargets = new Targets();
    }

    /// <summary>
    /// Runs the application asynchronously.
    /// </summary>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="args">The command line arguments.</param>
    /// <param name="configureApplication">An optional action to configure the application.</param>
    /// <param name="configureCLA">An optional action to configure the command line application.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the exit code.</returns>
    public static async Task<int> RunAsync<TProgram>(
        string[] args,
        Action<HostApplicationBuilder>? configureApplication = default,
        Action<CommandLineApplication<TProgram>>? configureCLA = default)
        where TProgram : class {
        Microsoft.Extensions.Hosting.IHost? app=null;
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<MacroApplication>();
        builder.Services.AddMacro(builder.Configuration);
        builder.UseCommandLineApplication<TProgram>(
            args: args,
            configure: (CommandLineApplication<TProgram> cla) => {
                if (configureCLA is { }) { configureCLA(cla); }
                MacroApplication.PostConfigure(args, cla);
            });

        if (configureApplication is { }) { configureApplication(builder); }

        app = builder.Build();
        var result = await app.RunCommandLineApplicationAsync();
        return result;
    }

    /// <summary>
    /// Configures the command line application after initial configuration.
    /// </summary>
    /// <typeparam name="TProgram">The type of the program.</typeparam>
    /// <param name="args">The command line arguments.</param>
    /// <param name="cla">The command line application.</param>
    private static void PostConfigure<TProgram>(
        string[] args,
        CommandLineApplication<TProgram> cla)
        where TProgram : class {
        var macroApplication = cla.GetRequiredService<MacroApplication>();

        if (macroApplication.AppTargets.GetTargetByName("parseCore") is null) {
            macroApplication.AppTargets.Add("parseCore", [], () => { });
        }
        if (macroApplication.AppTargets.GetTargetByName("parse") is null) {
            macroApplication.AppTargets.Add("parse", ["parseCore"], () => { });
        }
        if (macroApplication.AppTargets.GetTargetByName("loadData") is null) {
            macroApplication.AppTargets.Add("loadData", [], () => { });
        }
        if (macroApplication.AppTargets.GetTargetByName("transformData") is null) {
            macroApplication.AppTargets.Add("transformData", [], () => { });
        }
        if (macroApplication.AppTargets.GetTargetByName("write") is null) {
            macroApplication.AppTargets.Add("write", [], () => { });
        }
        if (macroApplication.AppTargets.GetTargetByName("build") is null) {
            macroApplication.AppTargets.Add("build", ["parse", "loadData", "transformData", "write"], () => { });
        }
        if (macroApplication.AppTargets.GetTargetByName("watch") is null) {
            macroApplication.AppTargets.Add("watch", ["parse"], () => { });
        }
        if (macroApplication.AppTargets.GetTargetByName("default") is null) {
            macroApplication.AppTargets.Add("default", ["build"], () => { 
                //cla.GetRequiredService<>
            });
        }

        // add the targets to the command line application
        cla.Argument<string>("targets", "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.", multipleValues: true);
        foreach (var (aliases, description) in Options.Definitions) {
            _ = cla.Option(string.Join("|", aliases), description, CommandOptionType.NoValue);
        }

        // set the handler for the command line application
        cla.OnExecuteAsync(async (CancellationToken cancellationToken) => {
            var targets = cla.Arguments[0].Values.OfType<string>();
            var options = new Options(
                Options.Definitions.Select(
                    d => (d.Aliases[0], cla.Options.Single(o => d.Aliases.Contains($"--{o.LongName}")).HasValue())));

            var console = cla.GetRequiredService<IConsole>();
            await macroApplication.AppTargets.RunAndExitAsync(
                args: args,
                messageOnly: null,
                getMessagePrefix: null,
                outputWriter: console.Out,
                diagnosticsWriter: console.Error);
            return 0;
        });
    }
}
