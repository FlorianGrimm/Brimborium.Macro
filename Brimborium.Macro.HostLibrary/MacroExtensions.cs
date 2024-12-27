using Microsoft.Extensions.DependencyInjection;

namespace Brimborium.Macro;

public static class MacroExtensions {
    public static IServiceCollection AddMacro(this IServiceCollection serviceDescriptors) {
        serviceDescriptors.AddOptions<WorkspaceServiceOptions>();
        serviceDescriptors.AddSingleton<WorkspaceService>();
        serviceDescriptors.AddSingleton<SolutionService>();
        return serviceDescriptors;
    }
}