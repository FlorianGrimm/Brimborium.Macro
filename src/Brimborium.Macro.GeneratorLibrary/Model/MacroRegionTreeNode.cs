using Brimborium.Text;

using Microsoft.CodeAnalysis;

using System.Collections.Immutable;

namespace Brimborium.Macro.Model;

public sealed record class MacroRegionTreeNode(
    ImmutableArray<MacroRegionNode> Children,
    StringSlice Text,
    Location? Location
    ) : MacroRegionNode(Text, Location)
    , IMacroRegionNode<MacroRegionTreeNodeBuilder> {

    public override IMacroRegionNodeBuilder ConvertToBuilder() => this.ToBuilder();
    public MacroRegionTreeNodeBuilder ToBuilder() => new MacroRegionTreeNodeBuilder(this);
}

public sealed class MacroRegionTreeNodeBuilder(MacroRegionTreeNode? source)
    : MacroRegionNodeBuilder<MacroRegionTreeNode>(source) {

    private List<IMacroRegionNodeBuilder>? _Children;

    public List<IMacroRegionNodeBuilder> Children {
        get {
            this.EnsureAwake();
            return (this._Children ??= new());
        }
        set {
            this.EnsureAwake();
            this._Children = value;
        }
    }


    public void AddChild(IMacroRegionNodeBuilder child) {
        this.EnsureAwake();
        if (this._Children is null) {
            this._Children = new();
        }
        this._Children.Add(child);
    }

    protected override void Awake(MacroRegionTreeNode source) {
        base.Awake(source);
        if (0 < source.Children.Length) {
            this._Children = new(source.Children.Length);
            foreach (var instance in source.Children) {
                var builder = instance.ConvertToBuilder();
                this._Children.Add(builder);
            }
        }
    }

    public override MacroRegionTreeNode Build() {

        if (this._Source is { } source) {
            return source;
        } else {
            List<MacroRegionNode> targetChildren=[];
            if (this._Children is { Count:>0 } children) {
                foreach (var child in children) {
                    var instance = child.ConvertToInstance();
                    targetChildren.Add(instance);
                }
            }
            return new MacroRegionTreeNode(targetChildren.ToImmutableArray(), this.Text, this.Location);
        }
    }
}