

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void NeedNewLine_BothStringsEmpty_ReturnsFalse() {
        var result = MacroParser.NeedNewLine("", "");
        
        Assert.False(result);
    }

    [Fact]
    public void NeedNewLine_FirstStringEmpty_ReturnsFalse() {
        var result = MacroParser.NeedNewLine("", "test");
        
        Assert.False(result);
    }

    [Fact]
    public void NeedNewLine_SecondStringEmpty_ReturnsFalse() {
        var result = MacroParser.NeedNewLine("test", "");
        
        Assert.False(result);
    }

    [Fact]
    public void NeedNewLine_FirstStringNull_ReturnsFalse() {
        var result = MacroParser.NeedNewLine(null!, "test");
        
        Assert.False(result);
    }

    [Fact]
    public void NeedNewLine_SecondStringNull_ReturnsFalse() {
        var result = MacroParser.NeedNewLine("test", null!);
        
        Assert.False(result);
    }

    [Fact]
    public void NeedNewLine_BothStringsEndStartWithoutNewline_ReturnsTrue() {
        var result = MacroParser.NeedNewLine("First", "Second");
        
        Assert.True(result);
    }

    [Fact]
    public void NeedNewLine_FirstStringEndsWithUnixNewline_ReturnsFalse() {
        var result = MacroParser.NeedNewLine("First\n", "Second");
        
        Assert.False(result);
    }

    [Fact]
    public void NeedNewLine_FirstStringEndsWithWindowsNewline_ReturnsFalse() {
        var result = MacroParser.NeedNewLine("First\r\n", "Second");
        
        Assert.False(result);
    }

    [Fact]
    public void NeedNewLine_SecondStringStartsWithUnixNewline_ReturnsFalse() {
        var result = MacroParser.NeedNewLine("First", "\nSecond");
        
        Assert.False(result);
    }

    [Fact]
    public void NeedNewLine_SecondStringStartsWithWindowsNewline_ReturnsFalse() {
        var result = MacroParser.NeedNewLine("First", "\r\nSecond");
        
        Assert.False(result);
    }

    [Fact]
    public void NeedNewLine_FirstStringEndsWithCarriageReturn_ReturnsFalse() {
        var result = MacroParser.NeedNewLine("First\r", "Second");
        
        Assert.False(result);
    }

    [Fact]
    public void NeedNewLine_SecondStringStartsWithCarriageReturn_ReturnsFalse() {
        var result = MacroParser.NeedNewLine("First", "\rSecond");
        
        Assert.False(result);
    }

    [Fact]
    public void NeedNewLine_BothStringsHaveNewlinesInMiddle_ReturnsTrue() {
        var result = MacroParser.NeedNewLine("First\ntext", "Second\ntext");
        
        Assert.True(result);
    }

    [Fact]
    public void NeedNewLine_WhitespaceAroundNewlines_ReturnsTrue() {
        var result = MacroParser.NeedNewLine("First  \n  ", "  \nSecond");
        
        Assert.True(result);
    }

    [Fact]
    public void NeedNewLine_MultipleNewlinesAtEndAndStart_ReturnsFalse() {
        var result = MacroParser.NeedNewLine("First\n\r\n", "\r\n\nSecond");
        
        Assert.False(result);
    }
}
