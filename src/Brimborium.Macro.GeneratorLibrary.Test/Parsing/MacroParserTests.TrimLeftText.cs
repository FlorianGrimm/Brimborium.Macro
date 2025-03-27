using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void TrimLeftText_ExactMatch_ReturnsTrue() {
        var text = "MacroTest".AsSpan();
        var lookingFor = "Macro".AsSpan();
        
        var result = MacroParser.TrimLeftText(ref text, lookingFor);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftText_NoMatch_ReturnsFalse() {
        var text = "TestMacro".AsSpan();
        var lookingFor = "Macro".AsSpan();
        
        var result = MacroParser.TrimLeftText(ref text, lookingFor);
        
        Assert.False(result);
        Assert.Equal("TestMacro", text.ToString());
    }

    [Fact]
    public void TrimLeftText_CaseInsensitiveMatch_ReturnsTrue() {
        var text = "mAcRoTest".AsSpan();
        var lookingFor = "MacRO".AsSpan();
        
        var result = MacroParser.TrimLeftText(ref text, lookingFor);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftText_EmptyText_ReturnsFalse() {
        var text = "".AsSpan();
        var lookingFor = "Macro".AsSpan();
        
        var result = MacroParser.TrimLeftText(ref text, lookingFor);
        
        Assert.False(result);
        Assert.Equal("", text.ToString());
    }

    [Fact]
    public void TrimLeftText_EmptyLookingFor_ReturnsTrue() {
        var text = "Test".AsSpan();
        var lookingFor = "".AsSpan();
        
        var result = MacroParser.TrimLeftText(ref text, lookingFor);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimLeftText_LookingForLongerThanText_ReturnsFalse() {
        var text = "Mac".AsSpan();
        var lookingFor = "Macro".AsSpan();
        
        var result = MacroParser.TrimLeftText(ref text, lookingFor);
        
        Assert.False(result);
        Assert.Equal("Mac", text.ToString());
    }

    [Fact]
    public void TrimLeftText_PartialMatch_ReturnsFalse() {
        var text = "MacroFragment".AsSpan();
        var lookingFor = "MacroTest".AsSpan();
        
        var result = MacroParser.TrimLeftText(ref text, lookingFor);
        
        Assert.False(result);
        Assert.Equal("MacroFragment", text.ToString());
    }

    [Fact]
    public void TrimLeftText_ExactMatchWithWhitespace_ReturnsTrue() {
        var text = "Macro   Test".AsSpan();
        var lookingFor = "Macro".AsSpan();
        
        var result = MacroParser.TrimLeftText(ref text, lookingFor);
        
        Assert.True(result);
        Assert.Equal("   Test", text.ToString());
    }
}
