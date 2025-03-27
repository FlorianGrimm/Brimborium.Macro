using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void TryGetMultiLineComment_BasicComment_Success() {
        var sourceCode = "/* comment */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal(" comment ", comment);
    }

    [Fact]
    public void TryGetMultiLineComment_NoComment_ReturnsZero() {
        var sourceCode = "regular text";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.True(comment.IsEmpty);
    }

    [Fact]
    public void TryGetMultiLineComment_EmptyComment_Success() {
        var sourceCode = "/**/";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.True(comment.IsEmpty);
    }

    [Fact]
    public void TryGetMultiLineComment_MultilineContent_Failed() {
        var sourceCode = @"/* This is a
multi-line
comment */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.Equal(@" This is a
multi-line
comment ", comment);
    }

    [Fact]
    public void TryGetMultiLineComment_IncompleteStart_ReturnsZero() {
        var sourceCode = "/comment*/";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.True(comment.IsEmpty);
    }

    [Fact]
    public void TryGetMultiLineComment_IncompleteEnd_ReturnsZero() {
        var sourceCode = "/*comment/";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.True(comment.IsEmpty);
    }

    [Fact]
    public void TryGetMultiLineComment_WithLeadingWhitespace_Success() {
        var sourceCode = "    /* comment */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal(" comment ", comment);
    }

    [Fact]
    public void TryGetMultiLineComment_WithTrailingContent_Success() {
        var sourceCode = "/* comment */ more text";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal(" comment ", comment);
    }

#if later
    [Fact]
    public void TryGetMultiLineComment_NestedCommentMarkers_Success() {
        var sourceCode = "/* outer /* inner */ */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal(" outer /* inner */ ", comment);
    }
#endif

    [Fact]
    public void TryGetMultiLineComment_WithSpecialCharacters_Success() {
        var sourceCode = "/* !@#$%^&*()_+ */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal(" !@#$%^&*()_+ ", comment);
    }

    [Fact]
    public void TryGetMultiLineComment_EmptyString_ReturnsZero() {
        var sourceCode = "";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.True(comment.IsEmpty);
    }

    [Fact]
    public void TryGetMultiLineComment_OnlyStartMarker_ReturnsZero() {
        var sourceCode = "/*";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.True(comment.IsEmpty);
    }

    [Fact]
    public void TryGetMultiLineComment_OnlyEndMarker_ReturnsZero() {
        var sourceCode = "*/";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.True(comment.IsEmpty);
    }

    [Fact]
    public void TryGetMultiLineComment_WithEscapedQuotes_Success() {
        var sourceCode = """/* Comment with "quoted" text */""";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal(""" Comment with "quoted" text """, comment);
    }

    [Fact]
    public void TryGetMultiLineComment_WithMultipleSpaces_Success() {
        var sourceCode = "/*   multiple   spaces   */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal("   multiple   spaces   ", comment);
    }

    [Fact]
    public void TryGetMultiLineComment_WithTabs_Success() {
        var sourceCode = "/*\tTabbed\tContent\t*/";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal("\tTabbed\tContent\t", comment);
    }

    [Fact]
    public void TryGetMultiLineComment_WithMixedNewlines_Success() {
        var sourceCode = "/* Line1\nLine2\r\nLine3\rLine4 */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal(" Line1\nLine2\r\nLine3\rLine4 ", comment);
    }

    [Fact]
    public void TryGetMultiLineComment_StartsWithAsterisk_ReturnsZero() {
        var sourceCode = "* not a comment */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.True(comment.IsEmpty);
    }
}
