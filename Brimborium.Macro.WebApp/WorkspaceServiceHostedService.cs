
namespace Brimborium.Macro.WebApp;

public class WorkspaceServiceHostedService(
        WorkspaceService workspaceService
        ) : BackgroundService    //IHostedService
    {
    private readonly WorkspaceService _WorkspaceService = workspaceService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        await Task.CompletedTask;
        var x = this._WorkspaceService.CreateWorkspace(default, default);
    }
}