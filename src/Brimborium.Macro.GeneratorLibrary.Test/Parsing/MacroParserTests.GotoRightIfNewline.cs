

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    
    [Fact]
    public void GotoRightIfNewline_NoNewline_ReturnsSameIndex() {
        var text = "abc";
        var index = 1;
        
        var result = MacroParser.GotoRightIfNewline(text, index);
        
        Assert.Equal(1, result);
    }

    [Fact]
    public void GotoRightIfNewline_SingleUnixNewline_MovesOneCharacter() {
        var text = "abc\ndef";
        var index = 3;
        
        var result = MacroParser.GotoRightIfNewline(text, index);
        
        Assert.Equal(4, result);
    }

    [Fact]
    public void GotoRightIfNewline_WindowsNewline_MovesTwoCharacters() {
        var text = "abc\r\ndef";
        var index = 3;
        
        var result = MacroParser.GotoRightIfNewline(text, index);
        
        Assert.Equal(5, result);
    }

    [Fact]
    public void GotoRightIfNewline_CarriageReturnOnly_MovesOneCharacter() {
        var text = "abc\rdef";
        var index = 3;
        
        var result = MacroParser.GotoRightIfNewline(text, index);
        
        Assert.Equal(4, result);
    }

    [Fact]
    public void GotoRightIfNewline_AtEndOfString_ReturnsSameIndex() {
        var text = "abc";
        var index = 3;
        
        var result = MacroParser.GotoRightIfNewline(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoRightIfNewline_NewlineAtEnd_MovesAppropriately() {
        var text = "abc\r\n";
        var index = 3;
        
        var result = MacroParser.GotoRightIfNewline(text, index);
        
        Assert.Equal(5, result);
    }
}
