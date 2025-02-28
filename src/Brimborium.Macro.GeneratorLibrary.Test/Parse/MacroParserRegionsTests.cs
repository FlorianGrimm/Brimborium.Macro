#pragma warning disable xUnit2004 // Do not use equality check to test for boolean conditions
#pragma warning disable IDE1006 // Naming Styles

using Brimborium.Macro.Commands;
using Brimborium.Macro.GeneratorLibrary.Test;

using System.Security.Cryptography;
using System.Text;

namespace Brimborium.Macro.Parse;

public class MacroParserRegionsTests {

    [Fact]
    public async Task TestSample001ParseRegionsFull() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, sourceCode, null, CancellationToken.None);

        await Verify(regionBlockTree, Defaults.VerifySettings);
    }

    [Fact]
    public async Task TestSample001ParseRegionsPartial() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var listRegionStart = MacroParserAnalyzer.AnalyzeSyntaxTree(syntaxTree, semanticModel).ToList();
        if (0 == listRegionStart.Count) { throw new Exception("listRegionStart is empty"); }

        var regionStart = listRegionStart[0];
        if (!(regionStart.TryGetLocation(out var location))) { throw new Exception("regionStart.TryGetLocation failed"); }
        var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, sourceCode, null, CancellationToken.None);
        await Verify(regionBlockTree, Defaults.VerifySettings);
    }

    [Fact]
    public async Task TestSample001ParseFixLocationTag() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, sourceCode, null, CancellationToken.None);

        var documentRegionTree = RegionBlockUpdate.UpdateLocationTag(regionBlockTree.DocumentRegionTree, UpdateLocationTagOptions.Default);
        await Verify(documentRegionTree, Defaults.VerifySettings);
    }

    [Fact]
    public async Task TestSample001ParseFixLocationTagAndUpdate() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");


        {
            var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, sourceCode, null, CancellationToken.None);
            var documentRegionTree = RegionBlockUpdate.UpdateLocationTag(regionBlockTree.DocumentRegionTree, new UpdateLocationTagOptions(true));
            var sbOut = new StringBuilder(sourceCode.Length);
            documentRegionTree.Generate(sourceCode, sbOut);
            sourceCode = sbOut.ToString();
        }

        {
            var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, sourceCode, null, CancellationToken.None);
            var documentRegionTree = RegionBlockUpdate.UpdateLocationTag(regionBlockTree.DocumentRegionTree, new UpdateLocationTagOptions(true));
            var sbOut = new StringBuilder(sourceCode.Length);
            documentRegionTree.Generate(sourceCode, sbOut);
            Assert.Equal(sourceCode, sbOut.ToString());
        }
    }


    [Fact]
    public async Task TestSample001ParseHack() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, sourceCode, null, CancellationToken.None);
        var documentRegionTree = RegionBlockUpdate.UpdateLocationTag(regionBlockTree.DocumentRegionTree, new UpdateLocationTagOptions(true));

        var tree = documentRegionTree.Tree;
        for (int idx = 0; idx < tree.Count; idx++) {
            var regionBlock=tree[idx];
            var x = await RegionBlockAnalyse.GetRegionBlockInformation(regionBlock);
            var rb=x.RegionBlock;
        }
    }
}