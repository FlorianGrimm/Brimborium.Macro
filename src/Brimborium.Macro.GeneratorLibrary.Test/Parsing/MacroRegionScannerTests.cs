using Brimborium.Macro.GeneratorLibrary;

using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
public partial class MacroRegionScannerTests {

    [Fact]
    public async Task ScanRegions_Sample001() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var syntaxTreeRoot = await syntaxTree.GetRootAsync();
        var macroRegionScanner = new MacroRegionScanner(sourceCode, syntaxTreeRoot, semanticModel);
        var listMacroRegion = macroRegionScanner.ScanRegions(CancellationToken.None);
        await Verify(listMacroRegion, Defaults.VerifySettings);
    }
    [Fact]
    public async Task ParseRegions_Sample001() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var syntaxTreeRoot = await syntaxTree.GetRootAsync();
        var macroRegionScanner = new MacroRegionScanner(sourceCode, syntaxTreeRoot, semanticModel);
        var listScannedRegion = macroRegionScanner.ScanRegions(CancellationToken.None);
        var regionBlockTree = macroRegionScanner.ParseRegions(listScannedRegion, CancellationToken.None);
        Assert.NotNull(regionBlockTree);
        //await Verify(regionBlockTree, Defaults.VerifySettings);
    }
}
