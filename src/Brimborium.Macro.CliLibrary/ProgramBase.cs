#pragma warning disable IDE0350 // Use implicitly typed lambda

using Bullseye;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Brimborium.Macro.CliLibrary;

public class MacroApplication
{
    public Targets AppTargets { get; }

    public MacroApplication()
    {
        this.AppTargets = new Targets();
    }

    public static async Task<int> Run<TProgram>(
        string[] args,
        Action<HostApplicationBuilder>? configureApplication = default,
        Action<CommandLineApplication<TProgram>>? configureCLA = default)
        where TProgram : class
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        builder.Services.AddSingleton<MacroApplication>();
        builder.UseCommandLineApplication<TProgram>(
            args: args,
            configure: (CommandLineApplication<TProgram> cla) =>
        {
            if (configureCLA is { }) { configureCLA(cla); }
            MacroApplication.PostConfigure(args, cla);
        });

        if (configureApplication is { }) { configureApplication(builder); }

        var app = builder.Build();
        var result = await app.RunCommandLineApplicationAsync();
        return result;
    }

    private static void PostConfigure<TProgram>(string[] args, CommandLineApplication<TProgram> cla)
        where TProgram : class
    {
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
            macroApplication.AppTargets.Add("default", ["build"], () => { });
        }


        cla.Argument<string>("targets", "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.", multipleValues: true);
        foreach (var (aliases, description) in Options.Definitions)
        {
            _ = cla.Option(string.Join("|", aliases), description, CommandOptionType.NoValue);
        }

        cla.OnExecuteAsync(async (CancellationToken cancellationToken) =>
        {
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
