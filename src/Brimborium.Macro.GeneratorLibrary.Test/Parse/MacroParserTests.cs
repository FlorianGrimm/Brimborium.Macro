#pragma warning disable xUnit2004 // Do not use equality check to test for boolean conditions
#pragma warning disable IDE1006 // Naming Styles

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System.Text;
using Xunit;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.Testing;

namespace Brimborium.Macro.Parse;

public class MacroParserTests {
    [Fact]
    public void TrimLeftTextTest() {
        var a = "1234".AsSpan();
        MacroParser.TrimLeftText(ref a, "12".AsSpan());
        Assert.Equal("34", a.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceNoNewLineTest() {
        var a = "  12".AsSpan();
        MacroParser.TrimLeftWhitespaceNoNewLine(ref a);
        Assert.Equal("12", a.ToString());
        MacroParser.TrimLeftWhitespaceNoNewLine(ref a);
        Assert.Equal("12", a.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceWithNewLineTest() {
        var a = "\r\n  12".AsSpan();
        MacroParser.TrimLeftWhitespaceWithNewLine(ref a);
        Assert.Equal("12", a.ToString());
        MacroParser.TrimLeftWhitespaceWithNewLine(ref a);
        Assert.Equal("12", a.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceNoNewLineTest() {
        var a = "12  ".AsSpan();
        MacroParser.TrimRightWhitespaceNoNewLine(ref a);
        Assert.Equal("12", a.ToString());
        MacroParser.TrimRightWhitespaceNoNewLine(ref a);
        Assert.Equal("12", a.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceWithNewLineTest() {
        var a = "\r\n12  \r\n".AsSpan();
        MacroParser.TrimRightWhitespaceWithNewLine(ref a);
        Assert.Equal("\r\n12", a.ToString());
        MacroParser.TrimRightWhitespaceWithNewLine(ref a);
        Assert.Equal("\r\n12", a.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceOrNewLineTest() {
        var a = "\r\n12 34".AsSpan();
        MacroParser.TrimLeftWhitespaceWithNewLine(ref a);
        Assert.Equal("12 34", a.ToString());
        MacroParser.TrimLeftWhitespaceWithNewLine(ref a);
        Assert.Equal("12 34", a.ToString());
    }

    [Fact]
    public void LeftUntilWhitespaceTest() {
        var a = "12 AB 34 CD".AsSpan();

        var b = MacroParser.LeftUntilWhitespace(ref a);
        Assert.Equal("AB 34 CD", a.ToString());
        Assert.Equal("12", b.ToString());

        var c = MacroParser.LeftUntilWhitespace(ref a);
        Assert.Equal("34 CD", a.ToString());
        Assert.Equal("AB", c.ToString());

        var d = MacroParser.LeftUntilWhitespace(ref a);
        Assert.Equal("CD", a.ToString());
        Assert.Equal("34", d.ToString());

        var e = MacroParser.LeftUntilWhitespace(ref a);
        Assert.Equal("", a.ToString());
        Assert.Equal("CD", e.ToString());
    }

    [Fact]
    public void TryGetMultiLineCommentTest() {
        {
            var result = MacroParser.TryGetMultiLineComment(" /* */", out var macroText);
            Assert.Equal(0, result);
        }
        {
            var result = MacroParser.TryGetMultiLineComment(" /* Macro */ ", out var macroText);
            Assert.Equal(0, result);
        }
        {
            var result = MacroParser.TryGetMultiLineComment(" /* EndMacro */ ", out var macroText);
            Assert.Equal(2, result);
            Assert.Equal("", macroText.ToString());
        }


        {
            var result = MacroParser.TryGetMultiLineComment(" /* Macro ABC*/", out var macroText);
            Assert.Equal(1, result);
            Assert.Equal("ABC", macroText.ToString());
        }
        {
            var result = MacroParser.TryGetMultiLineComment(" /* EndMacro ABC*/", out var macroText);
            Assert.Equal(2, result);
            Assert.Equal("ABC", macroText.ToString());
        }

        {
            var result = MacroParser.TryGetMultiLineComment(" /* Macro ABC */ ", out var macroText);
            Assert.Equal(1, result);
            Assert.Equal("ABC", macroText.ToString());
        }
        {
            var result = MacroParser.TryGetMultiLineComment(" /* EndMacro ABC */ ", out var macroText);
            Assert.Equal(2, result);
            Assert.Equal("ABC", macroText.ToString());
        }
    }

    [Fact]
    public void TryGetRegionBlockStartTest() {
        {
            Assert.Equal(false, MacroParser.TryGetRegionBlockStart("", out var macroText));
            Assert.Equal("", macroText.ToString());
        }
        {
            Assert.Equal(false, MacroParser.TryGetRegionBlockStart("Macro", out var macroText));
            Assert.Equal("", macroText.ToString());
        }
        {
            Assert.Equal(true, MacroParser.TryGetRegionBlockStart("Macro ABC", out var macroText));
            Assert.Equal("ABC", macroText.ToString());
        }
    }

    [Fact]
    public void EqualsLinesTest() {
        Assert.Equal(true, MacroParser.EqualsLines("", ""));
        Assert.Equal(true, MacroParser.EqualsLines("", " "));
        Assert.Equal(true, MacroParser.EqualsLines(" ", ""));
        Assert.Equal(true, MacroParser.EqualsLines(" ", " "));

        Assert.Equal(true, MacroParser.EqualsLines(" \r\n", ""));
        Assert.Equal(true, MacroParser.EqualsLines("", " \r\n"));

        Assert.Equal(true, MacroParser.EqualsLines(" a\r\n", "a"));
        Assert.Equal(true, MacroParser.EqualsLines("a", " a\r\n"));
        Assert.Equal(true, MacroParser.EqualsLines("  a  ", " a\r\n"));
        Assert.Equal(true, MacroParser.EqualsLines(" a\r\n", "  a  "));

        Assert.Equal(true, MacroParser.EqualsLines("a\r\nb", "a\r\nb"));
        Assert.Equal(true, MacroParser.EqualsLines("a\r\nb", "a\r\n b"));
        Assert.Equal(true, MacroParser.EqualsLines("     a\r\nb", "a\r\n b"));
        Assert.Equal(true, MacroParser.EqualsLines("     a\r\n    b", "a\r\n b"));
        Assert.Equal(true, MacroParser.EqualsLines("     a\r\n    b\r\n", "a\r\n b"));
    }
    //public static required string GetIsGenerated { get; set; }
    [Fact]
    public async Task Test01() {
        var sourceCode = """
namespace Brimborium.Macro.Sample1;

[Brimborium.Macro.Macro("hugo")]
internal partial class Data {
    [Brimborium.Macro.Macro("hugo")]
    public required string Name { get; set; }

    /* Macro TestMe */
    public required int Age { get; set; }
    /* EndMacro */

    /* Macro TestMe #10 */
    public required System.DateTime Birthday { get; set; }
    /* EndMacro #10 */

    #region Macro TestMe #20
    public required string Nickname { get; set; }
    #endregion #20
}
""";
        var parseOptions = new CSharpParseOptions(languageVersion: LanguageVersion.Latest, documentationMode: DocumentationMode.Parse, kind: SourceCodeKind.Regular);

        AdhocWorkspace adhocWorkspace = new AdhocWorkspace();
        //var solution=adhocWorkspace.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Default));
        var projectId = ProjectId.CreateNewId();
        var metadataReferences = new MetadataReference[]
               {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(File).Assembly.Location)
               };
        var projectInfo = ProjectInfo.Create(
            id: projectId,
            version: VersionStamp.Default,
            name: "TestProject",
            assemblyName: "TestProject",
            language: LanguageNames.CSharp,
            compilationOptions: new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable),
            metadataReferences: metadataReferences);
        adhocWorkspace.AddProject(projectInfo);

        var pathMacroAttribute = System.IO.Path.Combine(GetSolutionFolder(), @"src\Brimborium.Macro\MacroAttribute.cs");
        var sourceMacroAttribute = System.IO.File.ReadAllText(pathMacroAttribute);

        var documentMacroAttribute = adhocWorkspace.AddDocument(projectId, "MacroAttribute.cs", SourceText.From(sourceMacroAttribute));
        var document = adhocWorkspace.AddDocument(projectId, "TestDocument.cs", SourceText.From(sourceCode));
        var project = adhocWorkspace.CurrentSolution.Projects.FirstOrDefault(project => project.Id == projectId);
        if (project is null) { throw new System.Exception("project is null"); }

        //var documentMacroAttribute = project.AddDocument("MacroAttribute.cs", sourceMacroAttribute, filePath: @"MacroAttribute.cs");
        //var document = project.AddDocument("TestDocument.cs", sourceCode, filePath: @"TestDocument.cs");

        var compliation = await project.GetCompilationAsync();
        if (compliation is null) { throw new System.Exception("compliation is null"); }
        var listDiagnostics = compliation.GetDiagnostics();

        var syntaxTree = compliation.SyntaxTrees.FirstOrDefault(
            syntaxTree => syntaxTree.FilePath == "TestDocument.cs")
            ;
        if (syntaxTree is null) { throw new System.Exception("syntaxTree is null"); }

        var semanticModel = compliation.GetSemanticModel(syntaxTree);
        if (semanticModel is null) { throw new System.Exception("semanticModel is null"); }

        var listRegionStart = MacroParser.AnalyzeSyntaxTree(syntaxTree, semanticModel).ToList();
        Assert.NotNull(listRegionStart);
        Assert.Equal(3, listRegionStart.Count());
    }

    private static string? _SolutionFolder;
    private static string GetSolutionFolder([CallerFilePath] string? callerFilePath = null) {
        if (_SolutionFolder is not null) {
            return _SolutionFolder;
        }
        var directory = new System.IO.DirectoryInfo(callerFilePath ?? System.IO.Directory.GetCurrentDirectory());
        while (directory != null && !System.IO.Directory.Exists(System.IO.Path.Combine(directory.FullName, ".git"))) {
            directory = directory.Parent;
        }
        if (directory is null) {
            throw new System.Exception("Solution folder not found.");
        }
        return _SolutionFolder = directory.FullName;
    }
}