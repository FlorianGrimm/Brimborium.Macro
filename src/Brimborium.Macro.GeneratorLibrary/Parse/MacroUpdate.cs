using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parse;

public static class MacroUpdate {
    public static DocumentRegionTree UpdateLocationTag(DocumentRegionTree documentRegionTree) {
        var tree = UpdateLocationTag(documentRegionTree.Tree);

        if (ReferenceEquals(tree, documentRegionTree.Tree)) {
            return documentRegionTree;
        } else { 
            return new DocumentRegionTree(documentRegionTree.FilePath, tree);
        }
    }

    public static List<RegionBlock> UpdateLocationTag(List<RegionBlock> tree) {
        var result = new List<RegionBlock>(tree.Count);
        var modified = false;
        foreach (var regionBlock in tree) {
            var newRegionBlock = UpdateLocationTag(regionBlock);
            if (ReferenceEquals(newRegionBlock, regionBlock)) {
                result.Add(regionBlock);
            } else {
                result.Add(newRegionBlock);
                modified = true;
            }
        }
        return modified ? result : tree;
    }

    private static RegionBlock UpdateLocationTag(RegionBlock regionBlock) {
        if (regionBlock.Start.LocationTag.LineIdentifier != regionBlock.Start.Line) {
            regionBlock = regionBlock with { 
                LocationTag = (regionBlock.LocationTag is { } locationTag) 
                ?   locationTag with {
                    LineIdentifier = regionBlock.Start.Line
                }:new LocationTag(
                    FilePath: null, 
                    LineIdentifier: regionBlock.Start.Line)
            };
        }
        return regionBlock;
    }
}
