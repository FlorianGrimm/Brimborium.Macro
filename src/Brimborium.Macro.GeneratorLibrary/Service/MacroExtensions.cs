using Brimborium.Macro.Service;

using Microsoft.CodeAnalysis.Host;
using Microsoft.Extensions.DependencyInjection;

namespace Brimborium.Macro.Service;

public static class MacroExtensions {
    public static IServiceCollection AddMacro(
        this IServiceCollection serviceDescriptors,
        Microsoft.Extensions.Configuration.IConfiguration? configuration = null) {
        serviceDescriptors.AddOptions<WorkspaceServiceOptions>();
        serviceDescriptors.AddSingleton<WorkspaceService>();
        serviceDescriptors.AddSingleton<SolutionService>();
        if (configuration is not null) {
            serviceDescriptors.Configure<WorkspaceServiceOptions>(configuration.GetSection("Workspace"));
            serviceDescriptors.Configure<SolutionServiceOptions>(configuration.GetSection("Solution"));
        }
        //serviceDescriptors.AddSingleton<BrainstormIdea>();
        return serviceDescriptors;
    }
}