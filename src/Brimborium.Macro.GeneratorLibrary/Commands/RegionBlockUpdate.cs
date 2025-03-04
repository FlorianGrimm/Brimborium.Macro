using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Brimborium.Macro;
using Brimborium.Macro.Parse;
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
        var modified = false;
        foreach (var regionBlock in tree) {
            var newRegionBlock = UpdateLocationTag(regionBlock, options);
            if (ReferenceEquals(newRegionBlock, regionBlock)) {
                result.Add(regionBlock);
            } else {
                result.Add(newRegionBlock);
                modified = true;
            }
        }

        if (modified) {
            return (true, result.ToImmutableArray());
        } else {
            return (false, tree);
        }
    }

    private static RegionBlock UpdateLocationTag(
        RegionBlock regionBlock,
        UpdateLocationTagOptions options) {
        if (regionBlock.Start.LocationTag.DoesExists()) {
            if (regionBlock.Start.LocationTag.LineIdentifier != regionBlock.Start.Line) {
                regionBlock = regionBlock.WithStartLocationTag(
                    (regionBlock.Start.LocationTag is { } locationTag)
                    ? locationTag with {
                        LineIdentifier = regionBlock.Start.Line
                    }
                    : new LocationTag(
                        FilePath: null,
                        LineIdentifier: regionBlock.Start.Line));
            }
        } else if (options.AddMissingLocationTag) {
            regionBlock = regionBlock.WithStartLocationTag(
                    new LocationTag(
                        FilePath: null,
                        LineIdentifier: regionBlock.Start.Line));
        }
        if (regionBlock.End is { } end) {
            regionBlock = regionBlock.WithEndLocationTag(
                new LocationTag(FilePath: null, LineIdentifier: regionBlock.Start.Line));
        }
        return regionBlock;
    }
}

public record UpdateLocationTagOptions(
    bool AddMissingLocationTag
) {
    public static UpdateLocationTagOptions Default { get; set; }
    = new UpdateLocationTagOptions(AddMissingLocationTag: true);
}
