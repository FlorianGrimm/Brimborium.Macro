

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_NoWhitespace_ReturnsSameIndex() {
        var text = "abcdef";
        var index = 3;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_SingleSpace_MovesOneCharacter() {
        var text = "abc def";
        var index = 4;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_MultipleSpaces_MovesMultipleCharacters() {
        var text = "abc   def";
        var index = 6;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_SingleTab_MovesOneCharacter() {
        var text = "abc\tdef";
        var index = 4;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_MultipleTabs_MovesMultipleCharacters() {
        var text = "abc\t\t\tdef";
        var index = 6;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_MixedWhitespace_MovesAppropriately() {
        var text = "abc \t \tdef";
        var index = 7;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_AtStartOfString_ReturnsZero() {
        var text = "abc";
        var index = 0;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(0, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_AtEndOfString_ReturnsSameIndex() {
        var text = "abc";
        var index = 3;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_WhitespaceAtStart_StopsAtZero() {
        var text = "   abc";
        var index = 3;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(0, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_WhitespaceAtEnd_MovesAppropriately() {
        var text = "abc   ";
        var index = 6;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(3, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_StopsAtNonWhitespace() {
        var text = "abc   def   ";
        var index = 9;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(9, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_PreservesNewlines() {
        var text = "abc  \r\n  def";
        var index = 6;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(6, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_EmptyString_ReturnsZero() {
        var text = "";
        var index = 0;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(0, result);
    }

    [Fact]
    public void GotoLeftWhileWhitespaceNotNewLine_OnlyWhitespace_ReturnsZero() {
        var text = "    \t\t    ";
        var index = 9;
        
        var result = MacroParser.GotoLeftWhileWhitespaceNotNewLine(text, index);
        
        Assert.Equal(0, result);
    }
}
