using global::Bullseye;
using global::McMaster.Extensions.CommandLineUtils;
using global::Microsoft.Extensions.DependencyInjection;
using global::Microsoft.Extensions.Hosting;
using global::Brimborium.Macro.CliLibrary;

namespace Brimborium.Macro.CliLibrary;

public class Program
{
    public static async Task<int> Main(string[] args)
        => await MacroApplication.RunAsync<Program>(
            args: args,
            configureApplication: (builder) =>
            {
            },
            configureCLA: (cla) =>
            {
                var macroApplication = cla.GetRequiredService<MacroApplication>();
                macroApplication.AppTargets.Add("default", ["hello"], () => System.Console.WriteLine("Hello default!"));
                macroApplication.AppTargets.Add("hello", () => System.Console.WriteLine("Hello World!"));
            });
}
