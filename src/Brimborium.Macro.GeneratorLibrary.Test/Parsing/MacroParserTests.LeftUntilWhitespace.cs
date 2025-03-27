

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void LeftUntilWhitespace_NoWhitespace_ReturnsEntireString() {
        var text = "TestString".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("TestString", result.ToString());
    }

    [Fact]
    public void LeftUntilWhitespace_StartsWithWhitespace_ReturnsEmpty() {
        var text = " TestString".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("", result.ToString());
    }

    [Fact]
    public void LeftUntilWhitespace_StartsWithTab_ReturnsEmpty() {
        var text = "\tTestString".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("", result.ToString());
    }

    [Fact]
    public void LeftUntilWhitespace_StartsWithNewline_ReturnsEmpty() {
        var text = "\r\nTestString".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("", result.ToString());
    }

    [Fact]
    public void LeftUntilWhitespace_WhitespaceInMiddle_ReturnsLeftPart() {
        var text = "Test String".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("Test", result.ToString());
    }

    [Fact]
    public void LeftUntilWhitespace_TabInMiddle_ReturnsLeftPart() {
        var text = "Test\tString".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("Test", result.ToString());
    }

    [Fact]
    public void LeftUntilWhitespace_NewlineInMiddle_ReturnsLeftPart() {
        var text = "Test\r\nString".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("Test", result.ToString());
    }

    [Fact]
    public void LeftUntilWhitespace_MultipleWhitespaceTypes_ReturnsLeftPart() {
        var text = "Test \t\r\nString".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("Test", result.ToString());
    }

    [Fact]
    public void LeftUntilWhitespace_EmptyString_ReturnsEmpty() {
        var text = "".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("", result.ToString());
    }

    [Fact]
    public void LeftUntilWhitespace_OnlyWhitespace_ReturnsEmpty() {
        var text = "  \t\r\n ".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("", result.ToString());
    }

    [Fact]
    public void LeftUntilWhitespace_MultipleWords_ReturnsFirstWord() {
        var text = "First Second Third".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("First", result.ToString());
    }

    [Fact]
    public void LeftUntilWhitespace_SpecialCharacters_ReturnsUntilWhitespace() {
        var text = "Test!@#$ String".AsSpan();
        
        var result = MacroParser.LeftUntilWhitespace(ref text);
        
        Assert.Equal("Test!@#$", result.ToString());
    }
}
