using Bullseye.Internal;

namespace Bullseye;

/// <summary>
/// Provides methods for defining and running targets.
/// </summary>
public partial class Targets
{
    private readonly TargetCollection targetCollection = [];

    public TargetCollection TargetCollection => this.targetCollection;

    public Target? GetTargetByName(string name) 
        => this.targetCollection.FirstOrDefault(target=>target.Name == name);
}
