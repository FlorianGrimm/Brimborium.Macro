#pragma warning disable xUnit2004 // Do not use equality check to test for boolean conditions
#pragma warning disable IDE1006 // Naming Styles

using Brimborium.Macro.Commands;
using Brimborium.Macro.GeneratorLibrary.Test;

using System.Text;

namespace Brimborium.Macro.Parse;

public class MacroParserRegionsSample001Tests {

    [Fact]
    public async Task ParseRegionsFull() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var syntaxTreeRoot = await syntaxTree.GetRootAsync();
        var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, syntaxTreeRoot, semanticModel, sourceCode, null, CancellationToken.None);

        await Verify(regionBlockTree, Defaults.VerifySettings);
    }

    [Fact]
    public async Task ParseRegionsPartial() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var listRegionStart = MacroParserAnalyzer.AnalyzeSyntaxTree(syntaxTree, semanticModel).ToList();
        if (0 == listRegionStart.Count) { throw new Exception("listRegionStart is empty"); }

        var regionStart = listRegionStart[0];
        if (!(regionStart.TryGetLocation(out var location))) { throw new Exception("regionStart.TryGetLocation failed"); }
        var syntaxTreeRoot = await syntaxTree.GetRootAsync();
        var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, syntaxTreeRoot, semanticModel, sourceCode, null, CancellationToken.None);
        await Verify(regionBlockTree, Defaults.VerifySettings);
    }

    [Fact]
    public async Task ParseFixLocationTag() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var syntaxTreeRoot = await syntaxTree.GetRootAsync();
        var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, syntaxTreeRoot, semanticModel, sourceCode, null, CancellationToken.None);

        var documentRegionTree = RegionBlockUpdate.UpdateLocationTag(regionBlockTree.DocumentRegionTree, UpdateLocationTagOptions.Default);
        await Verify(documentRegionTree, Defaults.VerifySettings);
    }

    [Fact]
    public async Task ParseGenerate() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var syntaxTreeRoot = await syntaxTree.GetRootAsync();
        var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, syntaxTreeRoot, semanticModel, sourceCode, null, CancellationToken.None);

        var documentRegionTree = RegionBlockUpdate.UpdateLocationTag(regionBlockTree.DocumentRegionTree, UpdateLocationTagOptions.Default);

        var sbOut = new StringBuilder(sourceCode.Length);
        documentRegionTree.Generate(sbOut);
        var sourceCodeAfter = sbOut.ToString();

        await Verify(sourceCodeAfter, Defaults.VerifySettings);
    }

    [Fact]
    public async Task ParseFixLocationTagTwice() {
        string sourceCode1Before;
        string sourceCode1After;
        {
            var (workspace1, project1, compilation1, filePath1, sourceCode1, syntaxTree1, semanticModel1)
                = await TestUtils.PrepareDocumentFromFile(
                    @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");
            sourceCode1Before = sourceCode1;
            var syntaxTreeRoot1 = await syntaxTree1.GetRootAsync();

            var regionBlockTree1 = MacroParserRegions.ParseRegions(filePath1, syntaxTree1, syntaxTreeRoot1, semanticModel1, sourceCode1, null, CancellationToken.None);
            var documentRegionTree = RegionBlockUpdate.UpdateLocationTag(regionBlockTree1.DocumentRegionTree, new UpdateLocationTagOptions(true));
            var sbOut = new StringBuilder(sourceCode1.Length);
            documentRegionTree.Generate(sbOut);
            sourceCode1After = sbOut.ToString();
        }

        {
            var (workspace2, project2, compilation2, filePath2, sourceCode2, syntaxTree2, semanticModel2)
                = await TestUtils.PrepareDocumentFromSourceCode(
                    sourceCode1After);

            var syntaxTreeRoot2 = await syntaxTree2.GetRootAsync();
            var regionBlockTree2 = MacroParserRegions.ParseRegions(filePath2, syntaxTree2, syntaxTreeRoot2, semanticModel2, sourceCode2, null, CancellationToken.None);
            var documentRegionTree = RegionBlockUpdate.UpdateLocationTag(regionBlockTree2.DocumentRegionTree, new UpdateLocationTagOptions(true));
            var sbOut = new StringBuilder(sourceCode2.Length);
            documentRegionTree.Generate(sbOut);
            var sourceCode2After = sbOut.ToString();
            await Verify(new string[] { sourceCode1After, sourceCode2After }, Defaults.VerifySettings);
            Assert.Equal(sourceCode1After, sourceCode2After);
        }
    }


    [Fact]
    public async Task RegionBlockInformation() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var syntaxTreeRoot = await syntaxTree.GetRootAsync();
        var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, syntaxTreeRoot, semanticModel, sourceCode, null, CancellationToken.None);
        Assert.Null(regionBlockTree.Error);
        var documentRegionTree = regionBlockTree.DocumentRegionTree;
        documentRegionTree = RegionBlockUpdate.UpdateLocationTag(documentRegionTree, new UpdateLocationTagOptions(true));
        await Verify(documentRegionTree, Defaults.VerifySettings);
    }
}
