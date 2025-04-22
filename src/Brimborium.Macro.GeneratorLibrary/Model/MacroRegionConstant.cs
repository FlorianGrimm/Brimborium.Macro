using Brimborium.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Brimborium.Macro.Model;

public record class MacroRegionConstant(
    StringSlice Text,
    Location? Location
) : MacroRegionNode(Text, Location)
  , IMacroRegionNode<MacroRegionConstantBuilder> {
    public override IMacroRegionNodeBuilder ConvertToBuilder() => this.ToBuilder();
    public MacroRegionConstantBuilder ToBuilder() => new MacroRegionConstantBuilder(this);
}

public sealed class MacroRegionConstantBuilder : MacroRegionNodeBuilder<MacroRegionConstant> {
    private readonly StringSlice _Text;
    private readonly Location? _Location;

    public MacroRegionConstantBuilder(MacroRegionConstant? source = default) : base(source) {
    }

    public MacroRegionConstantBuilder(
        StringSlice text,
        Location? location
    ) : base(null) {
        this._Text = text;
        this._Location = location;
    }

    public override MacroRegionConstant Build() {
        return new MacroRegionConstant(
            Text: this.Text,
            Location: this.Location);
    }
}