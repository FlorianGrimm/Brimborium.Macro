#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057

using System.Collections.Immutable;
using System.Text;

namespace Brimborium.Macro.Model;

public sealed record class DocumentRegionTree(
    string FilePath,
    RegionBlock Tree
    ) {
    public void Generate(StringBuilder sbOut) {
        this.Tree.Generate(sbOut);
    }
    /*
    public void Generate(string sourceCode, StringBuilder sbOut) {
        int pos = 0;
        this.Generate(sourceCode, ref pos, sbOut);
        sbOut.Append(sourceCode.AsSpan(pos));
    }

    public void Generate(string sourceCode, ref int pos, StringBuilder sbOut) {
        foreach (var regionBlock in this.Tree) {
            regionBlock.AppendPrefix(sourceCode, ref pos, sbOut);
            regionBlock.Generate(sourceCode, ref pos, sbOut);
        }
    }
    */
}
