using Brimborium.Macro.Parsing;
using Brimborium.Macro.Service;

using Microsoft.CodeAnalysis.Host;
using Microsoft.Extensions.DependencyInjection;

namespace Brimborium.Macro.Service;

public static class MacroExtensions {
    public static IServiceCollection AddMacro(
        this IServiceCollection serviceDescriptors,
        Microsoft.Extensions.Configuration.IConfiguration? configuration = null) {
        // serviceDescriptors.AddOptions<WorkspaceServiceOptions>();
        serviceDescriptors.AddSingleton<SolutionService>();
        if (configuration is not null) {
            //serviceDescriptors.Configure<WorkspaceServiceOptions>(configuration.GetSection("Workspace"));
            serviceDescriptors.AddOptions<SolutionServiceOptions>()
                .BindConfiguration("Solution");
            //serviceDescriptors.Configure<SolutionServiceOptions>(configuration.GetSection("Solution"));
        }
        WorkspaceService workspaceService = new WorkspaceService();
        workspaceService.EnsureRegisterInstance();
        serviceDescriptors.AddSingleton<WorkspaceService>(workspaceService);
        serviceDescriptors.AddSingleton<MacroRegionScanner>();
        
        //serviceDescriptors.AddSingleton<BrainstormIdea>();
        return serviceDescriptors;
    }
}