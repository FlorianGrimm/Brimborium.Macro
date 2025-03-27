using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void EqualsLines_IdenticalStrings_ReturnsTrue() {
        var text1 = "line1\nline2\nline3";
        var text2 = "line1\nline2\nline3";
        
        Assert.True(MacroParser.EqualsLines(text1, text2));
    }

    [Fact]
    public void EqualsLines_DifferentWhitespace_ReturnsTrue() {
        var text1 = "  line1  \n   line2\nline3   ";
        var text2 = "line1\nline2\nline3";
        
        Assert.True(MacroParser.EqualsLines(text1, text2));
    }

    [Fact]
    public void EqualsLines_DifferentLineEndings_ReturnsTrue() {
        var text1 = "line1\r\nline2\rline3\n";
        var text2 = "line1\nline2\nline3";
        
        Assert.True(MacroParser.EqualsLines(text1, text2));
    }

    [Fact]
    public void EqualsLines_EmptyLines_ReturnsTrue() {
        var text1 = "line1\n\nline3";
        var text2 = "line1\nline3";
        
        Assert.True(MacroParser.EqualsLines(text1, text2));
    }

    [Fact]
    public void EqualsLines_DifferentContent_ReturnsFalse() {
        var text1 = "line1\nline2\nline3";
        var text2 = "line1\ndifferent\nline3";
        
        Assert.False(MacroParser.EqualsLines(text1, text2));
    }

    [Fact]
    public void EqualsLines_DifferentOrder_ReturnsFalse() {
        var text1 = "line1\nline2\nline3";
        var text2 = "line2\nline1\nline3";
        
        Assert.False(MacroParser.EqualsLines(text1, text2));
    }

    [Fact]
    public void EqualsLines_EmptyStrings_ReturnsTrue() {
        var text1 = "";
        var text2 = "";
        
        Assert.True(MacroParser.EqualsLines(text1, text2));
    }

    [Fact]
    public void EqualsLines_WhitespaceOnly_ReturnsFalse() {
        var text1 = "   \n  \t  \n    ";
        var text2 = "";
        
        Assert.True(MacroParser.EqualsLines(text1, text2));
    }
}
