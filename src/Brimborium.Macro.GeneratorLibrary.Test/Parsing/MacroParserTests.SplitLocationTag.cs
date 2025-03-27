using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void SplitLocationTag_NoTag_ReturnsFalse() {
        var text = "macro text without tag";
        var result = MacroParser.SplitLocationTag(text, out var macroText, out var locationTag);
        
        Assert.False(result);
        Assert.Equal(text, macroText.ToString());
        Assert.Equal(0, locationTag.LineIdentifier);
        Assert.Null(locationTag.FilePath);
    }

    [Fact]
    public void SplitLocationTag_WithLineNumber_ReturnsTrue() {
        var text = "macro text #123";
        var result = MacroParser.SplitLocationTag(text, out var macroText, out var locationTag);
        
        Assert.True(result);
        Assert.Equal("macro text", macroText.ToString());
        Assert.Equal(123, locationTag.LineIdentifier);
        Assert.Null(locationTag.FilePath);
    }

    [Fact]
    public void SplitLocationTag_WithFilePath_ReturnsTrue() {
        var text = "macro text #file.cs:123";
        var result = MacroParser.SplitLocationTag(text, out var macroText, out var locationTag);
        
        Assert.True(result);
        Assert.Equal("macro text", macroText.ToString());
        Assert.Equal(123, locationTag.LineIdentifier);
        Assert.Equal("file.cs", locationTag.FilePath);
    }

    [Fact]
    public void SplitLocationTag_WithMultipleHashes_UsesLastHash() {
        var text = "macro#with#hash #456";
        var result = MacroParser.SplitLocationTag(text, out var macroText, out var locationTag);
        
        Assert.True(result);
        Assert.Equal("macro#with#hash", macroText.ToString());
        Assert.Equal(456, locationTag.LineIdentifier);
        Assert.Null(locationTag.FilePath);
    }

    [Fact]
    public void SplitLocationTag_EmptyString_ReturnsFalse() {
        var text = "";
        var result = MacroParser.SplitLocationTag(text, out var macroText, out var locationTag);
        
        Assert.False(result);
        Assert.Equal("", macroText.ToString());
        Assert.Equal(0, locationTag.LineIdentifier);
        Assert.Null(locationTag.FilePath);
    }

    [Fact]
    public void SplitLocationTag_InvalidLocationTag_ReturnsTrue() {
        var text = "macro text #invalid";
        var result = MacroParser.SplitLocationTag(text, out var macroText, out var locationTag);
        
        Assert.True(result);
        Assert.Equal(text, macroText.ToString());
        Assert.Equal(0, locationTag.LineIdentifier);
        Assert.Null(locationTag.FilePath);
    }

    [Fact]
    public void SplitLocationTag_WithWhitespace_HandlesWhitespaceCorrectly() {
        var text = "  macro text  #  file.cs:123  ";
        var result = MacroParser.SplitLocationTag(text, out var macroText, out var locationTag);
        
        Assert.True(result);
        Assert.Equal("  macro text  ", macroText.ToString());
        Assert.Equal(123, locationTag.LineIdentifier);
        Assert.Equal("file.cs", locationTag.FilePath);
    }
}
