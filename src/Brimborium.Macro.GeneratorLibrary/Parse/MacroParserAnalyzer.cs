#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0057
#pragma warning disable IDE1006 // Naming Styles

using Brimborium.Macro.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Brimborium.Macro.Parse;

public static class MacroParserAnalyzer {

    public static IEnumerable<RegionStart> AnalyzeSyntaxTree(
        SyntaxTree tree,
        SemanticModel semanticModel) {
        string? fullText = default;
        HashSet<Location> hsKnownLocation = new();
        var sourceCode = tree.GetText().ToString();
        if (!sourceCode.Contains("Macro")) {
            yield break;
        }
        var rootNode = tree.GetRoot();
        foreach (var nodeOrToken in rootNode.DescendantNodesAndTokensAndSelf()) {
            if (nodeOrToken.AsNode() is { } node) {
                if (node.IsKind(SyntaxKind.Attribute)
                    && node is AttributeSyntax attributeSyntax) {
                    /*
                    attributeSyntax.Name.GetType().FullName
                    "Microsoft.CodeAnalysis.CSharp.Syntax.QualifiedNameSyntax"

                    attributeSyntax.Name.GetText().ToString()
                    "Brimborium.Macro.Macro"

                     attributeSyntax.ArgumentList.Arguments[0].GetType().FullName
                    "Microsoft.CodeAnalysis.CSharp.Syntax.AttributeArgumentSyntax"
                     */
                    continue;
                } else {
                    foreach (var trivia in node.GetLeadingTrivia()) {
                        if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) {
                            if (fullText is null) {
                                fullText = tree.GetText().ToString() ?? string.Empty;
                            }
                            ReadOnlySpan<char> commentText = fullText.AsSpan(trivia.FullSpan.Start, trivia.FullSpan.Length);

                            if (1 == MacroParser.TryGetMultiLineComment(commentText, out var commentMacroText)) {
                                var location = trivia.GetLocation();
                                if (hsKnownLocation.Add(location)) {
                                    MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                                    yield return new RegionStart(macroText.ToString(), locationTag, trivia, location);
                                } else {
                                    continue;
                                }
                            }

                        } else if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)) {
                            if (trivia.IsDirective) {
                                var location = trivia.GetLocation();
                                if (hsKnownLocation.Contains(location)) {
                                    continue;
                                }
                                var structure = (DirectiveTriviaSyntax)trivia.GetStructure()!;
                                if (structure is RegionDirectiveTriviaSyntax regionDirective) {
                                    if (!regionDirective.EndOfDirectiveToken.IsMissing) {
                                        var regionText = regionDirective.EndOfDirectiveToken.ToFullString().AsSpan();
                                        if (MacroParser.TryGetRegionBlockStart(regionText, out var commentMacroText)) {
                                            location = regionDirective.GetLocation();
                                            if (hsKnownLocation.Add(location)) {
                                                MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                                                yield return new RegionStart(macroText.ToString(), locationTag, regionDirective, location);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}
