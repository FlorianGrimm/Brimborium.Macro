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
    public async Task ParseRegionsFull_Sample001() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");

        var syntaxTreeRoot = await syntaxTree.GetRootAsync();
        var macroRegionScanner = new MacroRegionScanner(sourceCode, syntaxTreeRoot, semanticModel);
        var regionBlockTree = macroRegionScanner.ScanRegions(CancellationToken.None);
            //ParseRegions(filePath, syntaxTree, syntaxTreeRoot, semanticModel, sourceCode, null, CancellationToken.None);

        await Verify(regionBlockTree, Defaults.VerifySettings);
    }
}
