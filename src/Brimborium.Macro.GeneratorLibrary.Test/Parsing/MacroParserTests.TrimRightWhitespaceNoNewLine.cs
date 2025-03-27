using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void TrimRightWhitespaceNoNewLine_NoWhitespace_ReturnsFalse() {
        var text = "Test".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceNoNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceNoNewLine_OnlySpaces_ReturnsTrue() {
        var text = "Test   ".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceNoNewLine_OnlyTabs_ReturnsTrue() {
        var text = "Test\t\t".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceNoNewLine_MixedWhitespace_ReturnsTrue() {
        var text = "Test \t \t ".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceNoNewLine_PreservesNewlines_ReturnsTrue() {
        var text = "Test  \t  \r\n".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test\r\n", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceNoNewLine_EmptyString_ReturnsFalse() {
        var text = "".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceNoNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceNoNewLine_OnlyNewlines_ReturnsFalse() {
        var text = "\r\n\n\r".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceNoNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("\r\n\n\r", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceNoNewLine_WhitespaceBeforeText_ReturnsFalse() {
        var text = "   Test".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceNoNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("   Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceNoNewLine_WhitespaceBeforeAndAfterText_ReturnsTrue() {
        var text = "  \t Test  ".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("  \t Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceNoNewLine_WhitespaceBeforeNewline_ReturnsTrue() {
        var text = "Test   \t  \r\n".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test\r\n", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceNoNewLine_NewlineFollowedByWhitespace_ReturnsTrue() {
        var text = "Test\r\n   \t  ".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test\r\n", text.ToString());
    }
}
