using Brimborium.Macro.Model;
using Brimborium.Macro.Service;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.CliLibrary.Service;
public class StateService {
    private readonly SolutionService _SolutionService;
    private readonly SolutionServiceOptions _SolutionServiceOptions;

    public StateService(
        SolutionService solutionService,
        IOptions<SolutionServiceOptions> solutionServiceOptions
        ) {
        this._SolutionService = solutionService;
        this._SolutionServiceOptions = solutionServiceOptions.Value;
    }

    public async Task<Solution?> GetSolution() {
        var solution = this._SolutionService.Solution;
        if (solution is null) {
            if (!(this._SolutionServiceOptions.SolutionFilePath is { Length: > 0 } solutionFilePath)) {
                return null;
            }

            solution = await this._SolutionService.OpenSolutionAsync(solutionFilePath);
        }
        return solution;
    }

    public void SetProjectInfo(ImmutableDictionary<ProjectId, MacroProjectInfo> imdictProjectInfo, ImmutableArray<MacroDocumentFileInfo> imlistDocumentFileInfo) {
    }
}
