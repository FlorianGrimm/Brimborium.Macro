#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

namespace Brimborium.Macro.Model;

public class MacroRegionBlockBuilderTests {
    [Fact]
    public void MacroRegionBlockBuilderTest1() {
        var builder = MacroRegionBlockBuilder.Empty
            .WithStart(MacroRegionStart.Empty)
            .WithEnd(MacroRegionEnd.Empty)
            .WithAddChild(
                MacroRegionBlockBuilder.Empty
                    .WithStart(MacroRegionStart.Empty)
                    .WithEnd(MacroRegionEnd.Empty))
            .WithAddChild(
                MacroRegionBlockBuilder.Empty
                    .WithStart(MacroRegionStart.Empty)
                    .WithEnd(MacroRegionEnd.Empty))
            ;
        var macroRegionBlock = builder.Build();
        Assert.Equal(builder.Children.Count, macroRegionBlock.Children.Length);
        Assert.Equal(builder.Children[0].Children.Count, macroRegionBlock.Children[0].Children.Length);
    }

    [Fact]
    public void MacroRegionBlockBuilderTest2() {
        var innderBuilder =
                MacroRegionBlockBuilder.Empty
                    .WithStart(MacroRegionStart.Empty)
                    .WithEnd(MacroRegionEnd.Empty);
        var builder = MacroRegionBlockBuilder.Empty
            .WithStart(MacroRegionStart.Empty)
            .WithEnd(MacroRegionEnd.Empty)
            .WithAddChild(innderBuilder)
            ;
        { 
            var macroRegionBlock = builder.Build();
            Assert.Equal(1, macroRegionBlock.Children.Length);
            Assert.Equal(1, macroRegionBlock.Children[0].Children.Length);
        }
        {
            innderBuilder.WithAddChild(
                MacroRegionBlockBuilder.Empty
                    .WithStart(MacroRegionStart.Empty)
                    .WithEnd(MacroRegionEnd.Empty));
            var macroRegionBlock = builder.Build();
            Assert.Equal(1, macroRegionBlock.Children.Length);
            Assert.Equal(1, macroRegionBlock.Children[0].Children.Length);
        }
    }
}
