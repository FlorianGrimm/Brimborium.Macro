using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Brimborium.Macro;
using Brimborium.Macro.Parse;

namespace Brimborium.Macro.Commands;
public static class RegionBlockUpdate {
    public static DocumentRegionTree UpdateLocationTag(
        DocumentRegionTree documentRegionTree,
        UpdateLocationTagOptions options) {
        var tree = UpdateLocationTag(documentRegionTree.Tree, options);

        if (ReferenceEquals(tree, documentRegionTree.Tree)) {
            return documentRegionTree;
        } else {
            return new DocumentRegionTree(documentRegionTree.FilePath, tree);
        }
    }

    public static List<RegionBlock> UpdateLocationTag(
        List<RegionBlock> tree,
        UpdateLocationTagOptions options
        ) {
        var result = new List<RegionBlock>(tree.Count);
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
        return modified ? result : tree;
    }

    private static RegionBlock UpdateLocationTag(
        RegionBlock regionBlock,
        UpdateLocationTagOptions options) {
        if (regionBlock.Start.LocationTag.DoesExists()) {
            if (regionBlock.Start.LocationTag.LineIdentifier != regionBlock.Start.Line) {
                regionBlock = regionBlock.WithStartLocationTag(
                    (regionBlock.Start.LocationTag is { } locationTag)
                    ? locationTag with { 
                        LineIdentifier = regionBlock.Start.Line }
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
        if (regionBlock.End is {} end){
            regionBlock = regionBlock.WithEndLocationTag(
                new LocationTag(FilePath:null, LineIdentifier: regionBlock.Start.Line));
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
