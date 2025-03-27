

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void GotoLeftIfNewline_NoNewline_ReturnsSameIndex() {
        var text = "abcdef";
        var index = 3;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftIfNewline_UnixNewline_MovesOneCharacter() {
        var text = "abc\ndef";
        var index = 4;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftIfNewline_WindowsNewline_MovesTwoCharacters() {
        var text = "abc\r\ndef";
        var index = 5;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftIfNewline_CarriageReturnOnly_MovesOneCharacter() {
        var text = "abc\rdef";
        var index = 4;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftIfNewline_AtStartOfString_ReturnsSameIndex() {
        var text = "\nabc";
        var index = 0;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(0, result);
    }

    [Fact]
    public void GotoLeftIfNewline_AtEndOfString_ReturnsSameIndex() {
        var text = "abc";
        var index = 3;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftIfNewline_NewlineAtEnd_MovesAppropriately() {
        var text = "abc\r\n";
        var index = 5;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftIfNewline_MultipleNewlines_MovesOnlyOnce() {
        var text = "abc\n\n\ndef";
        var index = 6;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(5, result);
    }

    [Fact]
    public void GotoLeftIfNewline_MixedNewlines_HandlesCorrectly() {
        var text = "abc\r\n\r\ndef";
        var index = 7;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(5, result);
    }

    [Fact]
    public void GotoLeftIfNewline_EmptyString_ReturnsZero() {
        var text = "";
        var index = 0;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(0, result);
    }

    [Fact]
    public void GotoLeftIfNewline_WhitespaceAroundNewline_ReturnsSameIndex() {
        var text = "abc  \n  def";
        var index = 7;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(7, result);
    }

    [Fact]
    public void GotoLeftIfNewline_IndexBeforeNewline_ReturnsSameIndex() {
        var text = "abc\ndef";
        var index = 3;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftIfNewline_IndexAfterNewline_MovesAppropriately() {
        var text = "abc\ndef";
        var index = 4;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftIfNewline_OnlyNewlines_MovesAppropriately() {
        var text = "\r\n\n\r";
        var index = 2;
        
        var result = MacroParser.GotoLeftIfNewline(text, index);
        
        Assert.Equal(0, result);
    }
}
