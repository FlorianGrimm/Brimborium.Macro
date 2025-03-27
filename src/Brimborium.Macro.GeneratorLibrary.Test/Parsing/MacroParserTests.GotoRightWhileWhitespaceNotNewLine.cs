

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void GotoRightWhileWhitespace_NoWhitespace_ReturnsSameIndex() {
        var text = "abcdef";
        var index = 3;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_SingleSpace_MovesOneCharacter() {
        var text = "abc def";
        var index = 3;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(4, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_MultipleSpaces_MovesMultipleCharacters() {
        var text = "abc   def";
        var index = 3;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(6, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_SingleTab_MovesOneCharacter() {
        var text = "abc\tdef";
        var index = 3;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(4, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_MultipleTabs_MovesMultipleCharacters() {
        var text = "abc\t\t\tdef";
        var index = 3;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(6, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_MixedWhitespace_MovesAppropriately() {
        var text = "abc \t \tdef";
        var index = 3;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(7, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_AtStartOfString_ReturnsSameIndex() {
        var text = "abc";
        var index = 0;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(0, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_AtEndOfString_ReturnsSameIndex() {
        var text = "abc";
        var index = 3;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_WhitespaceAtStart_MovesAppropriately() {
        var text = "   abc";
        var index = 0;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_WhitespaceAtEnd_StopsAtEnd() {
        var text = "abc   ";
        var index = 3;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(6, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_StopsAtNonWhitespace() {
        var text = "abc   def   ";
        var index = 3;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(6, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_PreservesNewlines() {
        var text = "abc  \r\n  def";
        var index = 3;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(5, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_EmptyString_ReturnsZero() {
        var text = "";
        var index = 0;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(0, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_OnlyWhitespace_MovesToEnd() {
        var text = "    \t\t    ";
        var index = 0;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(10, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_StartFromMiddleOfWhitespace_MovesAppropriately() {
        var text = "abc     def";
        var index = 5;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(8, result);
    }

    [Fact]
    public void GotoRightWhileWhitespace_IndexAtNonWhitespace_ReturnsSameIndex() {
        var text = "abc   def";
        var index = 2;
        
        var result = MacroParser.GotoRightWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(2, result);
    }
}
