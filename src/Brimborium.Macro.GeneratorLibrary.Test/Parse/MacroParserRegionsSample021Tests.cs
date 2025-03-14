#pragma warning disable xUnit2004 // Do not use equality check to test for boolean conditions
#pragma warning disable IDE1006 // Naming Styles

using Brimborium.Macro.Commands;
using Brimborium.Macro.GeneratorLibrary.Test;

namespace Brimborium.Macro.Parse;

public class MacroParserRegionsSample021Tests {

    [Fact]
    public async Task ParseRegionsFull() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample021.cs");

        var syntaxTreeRoot = await syntaxTree.GetRootAsync();
        var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, syntaxTreeRoot, semanticModel, sourceCode, null, CancellationToken.None);

        await Verify(regionBlockTree, Defaults.VerifySettings);
    }

    [Fact]
    public async Task ParseFixLocationTag() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample021.cs");

        var syntaxTreeRoot = await syntaxTree.GetRootAsync();
        var regionBlockTree = MacroParserRegions.ParseRegions(filePath, syntaxTree, syntaxTreeRoot, semanticModel, sourceCode, null, CancellationToken.None);

        var documentRegionTree = RegionBlockUpdate.UpdateLocationTag(regionBlockTree.DocumentRegionTree, UpdateLocationTagOptions.Default);
        await Verify(documentRegionTree, Defaults.VerifySettings);
    }
}