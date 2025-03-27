using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void TrimLeftWhitespaceWithNewLine_NoWhitespace_ReturnsFalse() {
        var text = "Test".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceWithNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceWithNewLine_OnlySpaces_ReturnsTrue() {
        var text = "   Test".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceWithNewLine_OnlyTabs_ReturnsTrue() {
        var text = "\t\tTest".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceWithNewLine_OnlyNewlines_ReturnsTrue() {
        var text = "\r\n\n\rTest".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceWithNewLine_MixedWhitespaceAndNewlines_ReturnsTrue() {
        var text = " \t\r\n \t\n Test".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceWithNewLine_EmptyString_ReturnsFalse() {
        var text = "".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceWithNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceWithNewLine_WhitespaceAfterText_ReturnsFalse() {
        var text = "Test   \r\n".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceWithNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("Test   \r\n", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceWithNewLine_WhitespaceBeforeAndAfterText_ReturnsTrue() {
        var text = "  \t\r\n Test  \r\n".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test  \r\n", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceWithNewLine_DifferentNewlineFormats_ReturnsTrue() {
        var text = "\r\n\n\r \t Test".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceWithNewLine_OnlyCarriageReturn_ReturnsTrue() {
        var text = "\rTest".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceWithNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }
}
