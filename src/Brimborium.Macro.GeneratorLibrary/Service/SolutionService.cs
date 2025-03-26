using Brimborium.Macro.Service;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Brimborium.Macro.Service;

public class SolutionService {
    public SolutionService(WorkspaceService? workspaceService) {
        this.WorkspaceService = workspaceService;
    }

    public WorkspaceService? WorkspaceService { get; set; }

    public MSBuildWorkspace? Workspace { get; set; }

    public Solution? Solution { get; set; }

    /// <summary>
    /// Open a solution file and all referenced projects.
    /// </summary>
    /// <param name="solutionFilePath">The path to the solution file to be opened. This may be an absolute path or a path relative to the current working directory.</param>
    /// <param name="progress">An optional <see cref="IProgress{T}"/> that will receive updates as the solution is opened.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to allow cancellation of this operation.</param>
    public async Task<Solution> OpenSolutionAsync(
        string solutionFilePath,
        IProgress<ProjectLoadProgress>? progress = null,
        CancellationToken cancellationToken = default) {
        var workspace = (this.Workspace ??= (
                this.WorkspaceService ?? throw new InvalidOperationException("WorkspaceService is null")
                ).CreateWorkspace());
        var solution = await workspace.OpenSolutionAsync(solutionFilePath, progress, cancellationToken);
        this.Solution = solution;
        return solution;
    }
}
public class SolutionServiceOptions {
    // Macro:Solution:SolutionFilePath "D:\github.com\FlorianGrimm\Brimborium.Angular\Brimborium.Angular\Brimborium.Angular.sln"
    public string? SolutionFilePath { get; set; }
}