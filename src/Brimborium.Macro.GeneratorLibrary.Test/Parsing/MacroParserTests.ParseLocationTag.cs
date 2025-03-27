using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void ParseLocationTag_OnlyLineNumber_Success() {
        var sourceCode = "123";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Null(result.FilePath);
        Assert.Equal(123, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_FilePathAndLineNumber_Success() {
        var sourceCode = "file.cs:123";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Equal("file.cs", result.FilePath);
        Assert.Equal(123, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_EmptyString_ReturnsDefaultValues() {
        var sourceCode = "";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Null(result.FilePath);
        Assert.Equal(0, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_InvalidFormat_ReturnsDefaultValues() {
        var sourceCode = "invalid";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Null(result.FilePath);
        Assert.Equal(0, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_WhitespaceOnly_ReturnsDefaultValues() {
        var sourceCode = "   ";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Null(result.FilePath);
        Assert.Equal(0, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_WithLeadingWhitespace_Success() {
        var sourceCode = "  123";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Null(result.FilePath);
        Assert.Equal(123, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_WithTrailingWhitespace_Success() {
        var sourceCode = "123  ";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Null(result.FilePath);
        Assert.Equal(123, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_FilePathWithSpaces_Success() {
        var sourceCode = "my file.cs:123";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Equal("my file.cs", result.FilePath);
        Assert.Equal(123, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_ComplexFilePath_Success() {
        var sourceCode = "src/folder/file.cs:123";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Equal("src/folder/file.cs", result.FilePath);
        Assert.Equal(123, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_WindowsStylePath_Success() {
        var sourceCode = @"src\folder\file.cs:123";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Equal(@"src\folder\file.cs", result.FilePath);
        Assert.Equal(123, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_MissingLineNumber_ReturnsDefaultValues() {
        var sourceCode = "file.cs:";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Null(result.FilePath);
        Assert.Equal(0, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_InvalidLineNumber_ReturnsDefaultValues() {
        var sourceCode = "file.cs:abc";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Null(result.FilePath);
        Assert.Equal(0, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_MultipleColons_UsesLastNumber() {
        var sourceCode = "file:cs:123";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Equal("file:cs", result.FilePath);
        Assert.Equal(123, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_ZeroLineNumber_Success() {
        var sourceCode = "file.cs:0";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Equal("file.cs", result.FilePath);
        Assert.Equal(0, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_NegativeLineNumber_ReturnsDefaultValues() {
        var sourceCode = "file.cs:-123";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Null(result.FilePath);
        Assert.Equal(0, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_VeryLargeLineNumber_Success() {
        var sourceCode = "file.cs:999999";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Equal("file.cs", result.FilePath);
        Assert.Equal(999999, result.LineIdentifier);
    }

    [Fact]
    public void ParseLocationTag_OnlyColon_ReturnsDefaultValues() {
        var sourceCode = ":";
        var result = MacroParser.ParseLocationTag(sourceCode);
        Assert.Null(result.FilePath);
        Assert.Equal(0, result.LineIdentifier);
    }
}
