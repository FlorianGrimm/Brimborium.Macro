using System.Diagnostics;

namespace Bullseye.Internal;

public class ActionTarget(string name, string description, IEnumerable<string> dependencies, Func<Task> action)
        : Target(name, description, dependencies)
{
    private readonly Func<Task> action = action;

    public override async Task RunAsync(bool dryRun, bool parallel, SemaphoreSlim parallelTargets, Output output,
        Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath)
    {
        if (parallel)
        {
            await parallelTargets.WaitAsync().Tax();
            try
            {
                await this.RunAsync(dryRun, output, messageOnly, dependencyPath).Tax();
            }
            finally
            {
                _ = parallelTargets.Release();
            }
        }
        else
        {
            await this.RunAsync(dryRun, output, messageOnly, dependencyPath).Tax();
        }
    }

    private async Task RunAsync(bool dryRun, Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath)
    {
        await output.BeginGroup(this).Tax();

        try
        {
            await output.Starting(this, dependencyPath).Tax();

            var stopWatch = new Stopwatch();

            if (!dryRun)
            {
                await this.RunAsync(output, messageOnly, dependencyPath, stopWatch).Tax();
            }

            await output.Succeeded(this, dependencyPath, stopWatch.Elapsed).Tax();
        }
        finally
        {
            await output.EndGroup().Tax();
        }
    }

    private async Task RunAsync(Output output, Func<Exception, bool> messageOnly, IReadOnlyCollection<Target> dependencyPath, Stopwatch stopWatch)
    {
        stopWatch.Start();

        try
        {
            await this.action().Tax();
        }
        catch (Exception ex)
        {
            var duration = stopWatch.Elapsed;

            if (!messageOnly(ex))
            {
                await output.Error(this, ex).Tax();
            }

            await output.Failed(this, ex, duration, dependencyPath).Tax();

            throw new TargetFailedException($"Target '{this.Name}' failed.", ex);
        }
    }
}
