

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void IsWhitespaceNotNewLine_Space_ReturnsTrue() {
        var result = MacroParser.IsWhitespaceNotNewLine(' ');
        
        Assert.True(result);
    }

    [Fact]
    public void IsWhitespaceNotNewLine_Tab_ReturnsTrue() {
        var result = MacroParser.IsWhitespaceNotNewLine('\t');
        
        Assert.True(result);
    }

    [Fact]
    public void IsWhitespaceNotNewLine_CarriageReturn_ReturnsFalse() {
        var result = MacroParser.IsWhitespaceNotNewLine('\r');
        
        Assert.False(result);
    }

    [Fact]
    public void IsWhitespaceNotNewLine_LineFeed_ReturnsFalse() {
        var result = MacroParser.IsWhitespaceNotNewLine('\n');
        
        Assert.False(result);
    }

    [Fact]
    public void IsWhitespaceNotNewLine_RegularCharacter_ReturnsFalse() {
        var result = MacroParser.IsWhitespaceNotNewLine('a');
        
        Assert.False(result);
    }

    [Fact]
    public void IsWhitespaceNotNewLine_Number_ReturnsFalse() {
        var result = MacroParser.IsWhitespaceNotNewLine('5');
        
        Assert.False(result);
    }

    [Fact]
    public void IsWhitespaceNotNewLine_SpecialCharacter_ReturnsFalse() {
        var result = MacroParser.IsWhitespaceNotNewLine('@');
        
        Assert.False(result);
    }

    [Fact]
    public void IsWhitespaceNotNewLine_NonBreakingSpace_ReturnsTrue() {
        var result = MacroParser.IsWhitespaceNotNewLine('\u00A0');
        
        Assert.True(result);
    }

    [Fact]
    public void IsWhitespaceNotNewLine_EmSpace_ReturnsTrue() {
        var result = MacroParser.IsWhitespaceNotNewLine('\u2003');
        
        Assert.True(result);
    }

    [Fact]
    public void IsWhitespaceNotNewLine_ZeroWidthSpace_ReturnsFalse() {
        var result = MacroParser.IsWhitespaceNotNewLine('\u200B');
        
        Assert.False(result);
    }

    [Fact]
    public void IsWhitespaceNotNewLine_VerticalTab_ReturnsTrue() {
        var result = MacroParser.IsWhitespaceNotNewLine('\v');
        
        Assert.True(result);
    }

    [Fact]
    public void IsWhitespaceNotNewLine_FormFeed_ReturnsTrue() {
        var result = MacroParser.IsWhitespaceNotNewLine('\f');
        
        Assert.True(result);
    }
}
