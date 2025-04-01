using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.MSBuild;

using System.Collections.Immutable;

namespace Brimborium.Macro.Service;

/*
 * once if needed
public class WorkspaceServiceOptions {}
*/
public class WorkspaceService {
    public WorkspaceService() {
    }

    /*
        this.Options = new WorkspaceServiceOptions();
    public WorkspaceService(IOptions<WorkspaceServiceOptions> options) {
        this.Options = options.Value;
    }

    public WorkspaceServiceOptions Options { get; set; }
    */

    private VisualStudioInstance? _VisualStudioInstance;

    public VisualStudioInstance EnsureRegisterInstance() {
        if (_VisualStudioInstance is null) {
            lock (this) {
                if (_VisualStudioInstance is null) {
                    _VisualStudioInstance = VisualStudioInstanceUtility.RegisterInstance(
                        // TODO: options to select the VisualStudioInstance
                        );
                }
                if (_VisualStudioInstance is null) {
                    throw new Exception("MSBuild is not registered.");
                }
            }
        }
        return _VisualStudioInstance;
    }

    public MSBuildWorkspace CreateWorkspace(
        IDictionary<string, string>? properties = default,
        Microsoft.CodeAnalysis.Host.HostServices? hostServices = default
        ) {
        // NOTE: Be sure to register an instance with the MSBuildLocator 
        //       before calling MSBuildWorkspace.Create()
        //       otherwise, MSBuildWorkspace won't MEF compose.
        this.EnsureRegisterInstance();
        var result = MSBuildWorkspace.Create(
            properties ?? ImmutableDictionary<string, string>.Empty,
            hostServices ?? MSBuildMefHostServices.DefaultServices
            );
        return result;
    }
}
