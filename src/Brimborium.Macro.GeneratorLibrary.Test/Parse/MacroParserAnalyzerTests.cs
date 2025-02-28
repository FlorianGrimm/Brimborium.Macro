#pragma warning disable xUnit2004 // Do not use equality check to test for boolean conditions
#pragma warning disable IDE1006 // Naming Styles

using Brimborium.Macro.GeneratorLibrary.Test;

namespace Brimborium.Macro.Parse;

public class MacroParserAnalyzerTests {

    [Fact]
    public async Task TestSample001AnalyzeSyntaxTree() {
        var (workspace, project, compilation, filePath, sourceCode, syntaxTree, semanticModel)
            = await TestUtils.PrepareDocumentFromFile(
                @"src\Brimborium.Macro.GeneratorLibrary.Test\Sample\Sample001.cs");
        var listRegionStart = MacroParserAnalyzer.AnalyzeSyntaxTree(syntaxTree, semanticModel).ToList();
        await Verify(listRegionStart, Defaults.VerifySettings);
    }
}
