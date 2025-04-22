using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void TryGetMultiLineComment_BasicComment_Fail() {
        var sourceCode = "/* comment */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.Equal("", comment);
    }

    [Fact]
    public void TryGetMultiLineComment_NoComment_ReturnsZero() {
        var sourceCode = "regular text";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.True(comment.IsEmpty);
    }

    [Fact]
    public void TryGetMultiLineComment_EmptyComment_Failed() {
        var sourceCode = "/**/";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
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
    public void TryGetMultiLineComment_WithLeadingWhitespace_Failed() {
        var sourceCode = "    /* comment */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.Equal("", comment);
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
    public void TryGetMultiLineComment_WithMultipleSpaces_Failed() {
        var sourceCode = "/*   multiple   spaces   */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.Equal("", comment);
    }

    [Fact]
    public void TryGetMultiLineComment_WithTabs_Failed() {
        var sourceCode = "/*\tTabbed\tContent\t*/";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.Equal("", comment);
    }

    [Fact]
    public void TryGetMultiLineComment_StartsWithAsterisk_ReturnsZero() {
        var sourceCode = "* not a comment */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(0, result);
        Assert.True(comment.IsEmpty);
    }

    [Fact]
    public void TryGetMultiLineComment_Macro_Success() {
        var sourceCode = "/* Macro Text */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal("Text", comment.ToString());
    }

    [Fact]
    public void TryGetMultiLineComment_EndMacro_Success() {
        var sourceCode = "/* EndMacro Text */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(2, result);
        Assert.Equal("Text", comment.ToString());
    }

}
