using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.Model;
public class MacroRegionConstantTests {
    [Fact]
    public void MacroRegionConstantTest() {
        var builder = new MacroRegionConstantBuilder("a", null);
        var act1 = builder.Build();
        builder.Text = "b";
        var act2 = builder.Build();
        Assert.Equal("a", act1.Text);
        Assert.Equal("b", act2.Text);
    }
}
