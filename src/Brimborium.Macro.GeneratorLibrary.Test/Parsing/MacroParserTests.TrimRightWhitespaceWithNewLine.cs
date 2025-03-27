using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void TrimRightWhitespaceWithNewLine_NoWhitespace_ReturnsFalse() {
        var text = "Test".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceWithNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceWithNewLine_OnlySpaces_ReturnsTrue() {
        var text = "Test   ".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceWithNewLine_OnlyTabs_ReturnsTrue() {
        var text = "Test\t\t".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceWithNewLine_OnlyNewlines_ReturnsTrue() {
        var text = "Test\r\n\n\r".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceWithNewLine_MixedWhitespaceAndNewlines_ReturnsTrue() {
        var text = "Test \t\r\n \t\n ".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceWithNewLine_EmptyString_ReturnsFalse() {
        var text = "".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceWithNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceWithNewLine_WhitespaceBeforeText_ReturnsFalse() {
        var text = "   \r\nTest".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceWithNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("   \r\nTest", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceWithNewLine_WhitespaceBeforeAndAfterText_ReturnsTrue() {
        var text = "  \t\r\n Test  \r\n".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("  \t\r\n Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceWithNewLine_DifferentNewlineFormats_ReturnsTrue() {
        var text = "Test\r\n\n\r \t ".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceWithNewLine_OnlyCarriageReturn_ReturnsTrue() {
        var text = "Test\r".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightWhitespaceWithNewLine_WhitespaceNewlineWhitespace_ReturnsTrue() {
        var text = "Test   \r\n   ".AsSpan();
        
        var result = MacroParser.TrimRightWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }
}
