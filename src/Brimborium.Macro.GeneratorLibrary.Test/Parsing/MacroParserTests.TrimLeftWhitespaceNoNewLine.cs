using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void TrimLeftWhitespaceNoNewLine_NoWhitespace_ReturnsFalse() {
        var text = "Test".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceNoNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceNoNewLine_OnlySpaces_ReturnsTrue() {
        var text = "   Test".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceNoNewLine_OnlyTabs_ReturnsTrue() {
        var text = "\t\tTest".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceNoNewLine_MixedWhitespace_ReturnsTrue() {
        var text = " \t \t Test".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceNoNewLine_PreservesNewlines_ReturnsTrue() {
        var text = "  \t  \r\nTest".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("\r\nTest", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceNoNewLine_EmptyString_ReturnsFalse() {
        var text = "".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceNoNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceNoNewLine_OnlyNewlines_ReturnsFalse() {
        var text = "\r\n\n\r".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceNoNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("\r\n\n\r", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceNoNewLine_WhitespaceAfterText_ReturnsFalse() {
        var text = "Test   ".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceNoNewLine(ref text);
        
        Assert.False(result);
        Assert.Equal("Test   ", text.ToString());
    }

    [Fact]
    public void TrimLeftWhitespaceNoNewLine_WhitespaceBeforeAndAfterText_ReturnsTrue() {
        var text = "  \t Test  ".AsSpan();
        
        var result = MacroParser.TrimLeftWhitespaceNoNewLine(ref text);
        
        Assert.True(result);
        Assert.Equal("Test  ", text.ToString());
    }
}
