using Brimborium.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Brimborium.Macro.Model;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public record class MacroRegionConstant(
    StringSlice Text,
    Location? Location
) : MacroRegionNode(Text, Location)
  , IMacroRegionNode<MacroRegionConstantBuilder> {
    public override IMacroRegionNodeBuilder ConvertToBuilder() => this.ToBuilder();
    public MacroRegionConstantBuilder ToBuilder() => new MacroRegionConstantBuilder(this);

    private string GetDebuggerDisplay() {
        var textLength = this.Text.Length;
        var subText = (textLength < 30) ? this.Text : this.Text.Substring(0, 30);
        return $"Const:{textLength}:{subText}";
    }
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class MacroRegionConstantBuilder : MacroRegionNodeBuilder<MacroRegionConstant> {
    public MacroRegionConstantBuilder(MacroRegionConstant? source = default) : base(source) {
    }

    public MacroRegionConstantBuilder(
        StringSlice text,
        Location? location
    ) : base(null) {
    }

    public override MacroRegionConstant Build() {
        return new MacroRegionConstant(
            Text: this.Text,
            Location: this.Location);
    }

    private string GetDebuggerDisplay() {
        var textLength = this.Text.Length;
        var subText = (textLength < 30) ? this.Text : this.Text.Substring(0, 30);
        return $"Const:{textLength}:{subText}";
    }
}