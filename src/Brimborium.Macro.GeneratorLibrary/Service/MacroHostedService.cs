using System;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Brimborium.Macro.Service;

public class MacroHostedService : BackgroundService {
    private readonly BrainstormIdea _BrainstormIdea;
    private readonly SolutionService _SolutionService;
    private readonly IHostApplicationLifetime _ApplicationLifetime;
    private readonly SolutionServiceOptions _SolutionServiceOptions;

    public MacroHostedService(
        BrainstormIdea brainstormIdea,
        SolutionService solutionService,
        IOptions<SolutionServiceOptions> solutionServiceOptions,
        IHostApplicationLifetime applicationLifetime
        ) {
        this._BrainstormIdea = brainstormIdea;
        this._SolutionService = solutionService;
        this._ApplicationLifetime = applicationLifetime;
        this._SolutionServiceOptions = solutionServiceOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken ctStop) {
        if (!(_SolutionServiceOptions.SolutionFilePath is { Length: > 0 })) {
            throw new Exception("SolutionFilePath is not set.");
        }
        
        await _BrainstormIdea.OpenSolutionAsync(_SolutionServiceOptions.SolutionFilePath, ctStop);
        await _BrainstormIdea.ListenForChanges(ctStop);
        await _BrainstormIdea.UpdateAllMacros(ctStop);

        _ApplicationLifetime.StopApplication();
        return;
    }
}