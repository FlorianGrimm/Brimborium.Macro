namespace Brimborium.Macro.Model;

public interface IMacroRegionTreeNodeBuilder
    : IMacroRegionNodeBuilder<MacroRegionTreeNode> {
    List<IMacroRegionNodeBuilder> Children { get; set; }
    void AddChild(IMacroRegionNodeBuilder child);
}
