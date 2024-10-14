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

namespace Brimborium.Macro;

public class MacroParserTests
{
    [Fact]
    public void TrimLeftTextTest(){
        var a= "1234".AsSpan();
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
}
