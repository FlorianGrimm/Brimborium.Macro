using Brimborium.Macro.Model;

using System.Collections.Immutable;

namespace Brimborium.Macro.Commands;
public static class RegionBlockUpdate {
    public static DocumentRegionTree UpdateLocationTag(
        DocumentRegionTree documentRegionTree,
        UpdateLocationTagOptions options) {
        var treeOld = documentRegionTree.Tree;
        var (modified, treeNext) = UpdateLocationTag(treeOld, options);

        if (modified) {
            return new DocumentRegionTree(documentRegionTree.FilePath, treeNext);
        } else {
            return documentRegionTree;
        }
    }

    public static (bool Modified, ImmutableArray<RegionBlock> Tree) UpdateLocationTag(
        ImmutableArray<RegionBlock> tree,
        UpdateLocationTagOptions options
        ) {
        var result = new List<RegionBlock>(tree.Length);
        var treeModified = false;
        foreach (var regionBlock in tree) {
            if ((regionBlock.Start.Kind == SyntaxNodeType.None)
                || (regionBlock.Start.Kind == SyntaxNodeType.Constant)) {
                result.Add(regionBlock);
            } else {
                var (newModified, newRegionBlock) = UpdateLocationTag(regionBlock, options);
                if (newModified) {
                    result.Add(newRegionBlock);
                    treeModified = true;
                } else {
                    result.Add(regionBlock);
                }
            }
        }

        if (treeModified) {
            return (true, result.ToImmutableArray());
        } else {
            return (false, tree);
        }
    }

    public static (bool modified, RegionBlock) UpdateLocationTag(
        RegionBlock regionBlock,
        UpdateLocationTagOptions options) {
        var oldRegionBlock = regionBlock;
        var locationTag = new LocationTag(FilePath: null, LineIdentifier: regionBlock.Start.Line);
        if (regionBlock.Start.HasValue) {
            if (regionBlock.Start.LocationTag.HasValue()) {
                regionBlock = regionBlock.WithStart(UpdateLocationTag(regionBlock.Start, options));
            } else if (options.AddMissingLocationTag) {
                regionBlock = regionBlock.WithStartLocationTag(locationTag);
            }
        }
        regionBlock = regionBlock.WithEndLocationTag(locationTag);
        if (0 < regionBlock.Children.Length) {
            var (modified, children) = UpdateLocationTag(regionBlock.Children, options);
            if (modified) {
                regionBlock = regionBlock with { Children = children };
            }
        }
        return (!ReferenceEquals(oldRegionBlock, regionBlock), regionBlock);
    }

    public static RegionStart UpdateLocationTag(
        RegionStart regionStart,
        UpdateLocationTagOptions options) {
        if (regionStart.LocationTag.HasValue()) {
            var line = regionStart.Line;
            if (regionStart.LocationTag.LineIdentifier != line) {
                if (regionStart.LocationTag is { } locationTag) {
                    return regionStart.WithLocationTag(
                        locationTag with {
                            LineIdentifier = line
                        });
                } else {
                    return regionStart.WithLocationTag(new LocationTag(
                        FilePath: null,
                        LineIdentifier: line));
                }
            }
        }
        return regionStart;
    }

    public static RegionEnd UpdateLocationTag(
        RegionEnd regionEnd,
        RegionStart regionStart,
        UpdateLocationTagOptions options) {
        if (regionEnd.LocationTag.HasValue()) {
            var line = regionStart.Line;
            if (regionEnd.LocationTag.LineIdentifier != line) {
                if (regionEnd.LocationTag is { } locationTag) {
                    return regionEnd.WithLocationTag(
                        locationTag with {
                            LineIdentifier = line
                        });
                } else {
                    return regionEnd.WithLocationTag(new LocationTag(
                        FilePath: null,
                        LineIdentifier: line));
                }
            }
        }
        return regionEnd;
    }
}

public record UpdateLocationTagOptions(
    bool AddMissingLocationTag
) {
    public static UpdateLocationTagOptions Default { get; set; }
    = new UpdateLocationTagOptions(AddMissingLocationTag: true);
}
