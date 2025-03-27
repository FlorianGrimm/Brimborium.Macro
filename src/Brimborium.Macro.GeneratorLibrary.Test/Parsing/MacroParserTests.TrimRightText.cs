using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
 
public partial class MacroParserTests {
    [Fact]
    public void TrimRightText_ExactMatch_ReturnsTrue() {
        var text = "TestMacro".AsSpan();
        var lookingFor = "Macro".AsSpan();
        
        var result = MacroParser.TrimRightText(ref text, lookingFor);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightText_NoMatch_ReturnsFalse() {
        var text = "MacroTest".AsSpan();
        var lookingFor = "Macro".AsSpan();
        
        var result = MacroParser.TrimRightText(ref text, lookingFor);
        
        Assert.False(result);
        Assert.Equal("MacroTest", text.ToString());
    }

    [Fact]
    public void TrimRightText_CaseInsensitiveMatch_ReturnsTrue() {
        var text = "TestmAcRo".AsSpan();
        var lookingFor = "MacRO".AsSpan();
        
        var result = MacroParser.TrimRightText(ref text, lookingFor);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightText_EmptyText_ReturnsFalse() {
        var text = "".AsSpan();
        var lookingFor = "Macro".AsSpan();
        
        var result = MacroParser.TrimRightText(ref text, lookingFor);
        
        Assert.False(result);
        Assert.Equal("", text.ToString());
    }

    [Fact]
    public void TrimRightText_EmptyLookingFor_ReturnsTrue() {
        var text = "Test".AsSpan();
        var lookingFor = "".AsSpan();
        
        var result = MacroParser.TrimRightText(ref text, lookingFor);
        
        Assert.True(result);
        Assert.Equal("Test", text.ToString());
    }

    [Fact]
    public void TrimRightText_LookingForLongerThanText_ReturnsFalse() {
        var text = "cro".AsSpan();
        var lookingFor = "Macro".AsSpan();
        
        var result = MacroParser.TrimRightText(ref text, lookingFor);
        
        Assert.False(result);
        Assert.Equal("cro", text.ToString());
    }

    [Fact]
    public void TrimRightText_PartialMatch_ReturnsFalse() {
        var text = "FragmentMacro".AsSpan();
        var lookingFor = "TestMacro".AsSpan();
        
        var result = MacroParser.TrimRightText(ref text, lookingFor);
        
        Assert.False(result);
        Assert.Equal("FragmentMacro", text.ToString());
    }

    [Fact]
    public void TrimRightText_ExactMatchWithWhitespace_ReturnsTrue() {
        var text = "Test   Macro".AsSpan();
        var lookingFor = "Macro".AsSpan();
        
        var result = MacroParser.TrimRightText(ref text, lookingFor);
        
        Assert.True(result);
        Assert.Equal("Test   ", text.ToString());
    }
}
