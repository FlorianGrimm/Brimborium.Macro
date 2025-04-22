using global::Bullseye;
using global::McMaster.Extensions.CommandLineUtils;
using global::Microsoft.Extensions.DependencyInjection;
using global::Microsoft.Extensions.Hosting;
using global::Brimborium.Macro.CliLibrary;
using Microsoft.Extensions.Configuration;

namespace TargetMacroCli;

public class Program {
    public static async Task<int> Main(string[] args)
        => await MacroApplication.RunAsync<Program>(
            args: args,
            configureApplication: (builder) => {
                builder.Configuration.AddUserSecrets("Brimborium.Macro");
                builder.Services.AddMediator();
            },
            configureCLA: (cla) => {
                var macroApplication = cla.GetRequiredService<MacroApplication>();
                //macroApplication.AppTargets.Add("default", ["hello"], () => System.Console.WriteLine("Hello default!"));
                macroApplication.AppTargets.Add("hello", () => System.Console.WriteLine("Hello World!"));
            });
}
