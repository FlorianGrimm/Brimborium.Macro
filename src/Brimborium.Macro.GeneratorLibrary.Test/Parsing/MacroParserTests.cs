using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Parsing;
public class MacroParserTests {
    [Fact]
    public void TryGetMultiLineCommentTest1() {
        var sourceCode = "/* comment */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal(" comment ", comment);
    }
    [Fact]
    public void TryGetMultiLineCommentTest2() {
        var sourceCode = "/* comment */";
        var result = MacroParser.TryGetMultiLineComment(sourceCode, out var comment);
        Assert.Equal(1, result);
        Assert.Equal(" comment ", comment);
    }
}
