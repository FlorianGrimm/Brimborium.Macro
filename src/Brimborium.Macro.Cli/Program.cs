using global::Bullseye;
using global::McMaster.Extensions.CommandLineUtils;
using global::Microsoft.Extensions.DependencyInjection;
using global::Microsoft.Extensions.Hosting;
using global::Brimborium.Macro.CliLibrary;
using global::Mediator;

namespace Brimborium.Macro.CliLibrary;

public class Program
{
    public static async Task<int> Main(string[] args)
        => await MacroApplication.RunAsync<Program>(
            args: args,
            configureApplication: (builder) =>
            {
                Microsoft.Extensions.DependencyInjection.MediatorDependencyInjectionExtensions.AddMediator(builder.Services, (options) => {
                    options.Namespace = "Brimborium.Macro.CliLibrary";
                    options.ServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton;
                });
            },
            configureCLA: (cla) =>
            {
                var macroApplication = cla.GetRequiredService<MacroApplication>();
                macroApplication.AppTargets.Add("hello", () => System.Console.WriteLine("Hello World!"));
            });
}
