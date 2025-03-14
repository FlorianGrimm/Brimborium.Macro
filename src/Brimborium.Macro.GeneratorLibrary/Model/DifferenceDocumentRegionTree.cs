#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057

using System.Collections.Immutable;

namespace Brimborium.Macro.Model;

public sealed record class DifferenceDocumentRegionTree(
    string FilePath,
    DifferenceRegionBlock Tree
    ) {
}

public record class DifferenceRegionBlock(
    DifferenceRegionStart Start,
    ImmutableArray<DifferenceRegionBlock> Children,
    DifferenceRegionEnd End
    ) {
}
public record struct DifferenceRegionStart(
    RegionStart Previous,
    RegionStart? Next
) {
}

public record struct DifferenceRegionEnd(
    RegionEnd Previous,
    RegionEnd? Next
) {
}