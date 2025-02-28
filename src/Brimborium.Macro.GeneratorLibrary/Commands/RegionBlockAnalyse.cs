using Brimborium.Macro.Parse;

using Microsoft.CodeAnalysis.CSharp;

using System.Threading.Tasks;

namespace Brimborium.Macro.Commands;

public static class RegionBlockAnalyse {
    public static async Task<RegionBlockInformation> GetRegionBlockInformation(
        RegionBlock regionBlock
        ) {
        {
            if (regionBlock.Start.TryGetSyntaxTrivia(out var syntaxTrivia, out var location)) {
                /*
                var syntaxTriviaToken = syntaxTrivia.Token;

                if (syntaxTriviaToken.GetNextToken(
                    includeZeroWidth: false,
                    includeSkipped: false,
                    includeDirectives: false,
                    includeDocumentationComments: false
                ) is { } nextToken) {
                    if (syntaxTrivia.SyntaxTree is { } syntaxTree) {
                        var root = await syntaxTree.GetRootAsync();
                        var nextNode = root.FindNode(nextToken.Span);
                        var kind = nextNode.Kind();
                    }
                }
                */
                var syntaxTriviaToken = syntaxTrivia.Token;
                var tokenSpan = syntaxTriviaToken.Span;
                if (syntaxTrivia.SyntaxTree is { } syntaxTree) {
                    var root = await syntaxTree.GetRootAsync();
                    var node = root.FindNode(tokenSpan);
                    if (node is null){
                        //
                    } else {
                        //
                        while(node is {}){
                            var nodeParent=node.Parent;
                            if (nodeParent is null){break;}
                            if (nodeParent.Span.Start == tokenSpan.Start){
                                node = nodeParent;
                            } else {
                                break;
                            }
                        }
                    }
                }
            }
        }
        {
            if (regionBlock.Start.TryGetRegionDirective(out var regionDirective, out var location)) {

            }
        }

        {
            if (regionBlock.Start.TryGetLocation(out var location)) {
                var end = location.SourceSpan.End;
                var root = await location.SourceTree!.GetRootAsync();
                //regionBlock.Start.SyntaxTrivia.Value.GetStructure().
            }
        }
        return new RegionBlockInformation(
            regionBlock
        );
    }
}

public record RegionBlockInformation(
    RegionBlock RegionBlock
);