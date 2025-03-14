using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Brimborium.Macro.Model;
using System.Collections.Immutable;

namespace Brimborium.Macro.Commands;

public static class RegionBlockAnalyse {
    /// <summary>
    /// Get the information about the region block.
    /// </summary>
    /// <param name="regionBlock">the region block</param>
    /// <returns>the additional information</returns>
    public static RegionBlockInformation GetRegionBlockInformation(
        RegionBlock regionBlock,
        SyntaxTree syntaxTree,
        SyntaxNode root
        ) {
        {
            if (regionBlock.Start.TryGetSyntaxTrivia(out var syntaxTrivia, out var location)) {
                var syntaxTriviaToken = syntaxTrivia.Token;
                var tokenSpan = syntaxTriviaToken.Span;
                var node = root.FindNode(tokenSpan);
                if (node is null) {
                    //
                    return CreateEmpty();
                } else {
                    return Create(regionBlock, node);
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
                //regionBlock.Start.SyntaxTrivia.Value.GetStructure().
            }
        }

        return CreateEmpty();

        RegionBlockInformation CreateEmpty() {
            return new RegionBlockInformation(
                Start: regionBlock.Start,
                Children: [], // TODO regionBlock.Children,
                End: regionBlock.End,
                Error:regionBlock.Error,
                Node: default,
                Identifier: default,
                Type: default
            );
        }
    }

    public static RegionBlockInformation Create(
        RegionBlock regionBlock,
        Microsoft.CodeAnalysis.SyntaxNode node) {
        if (node is PropertyDeclarationSyntax propertyDeclarationSyntax) {
            var identifier = propertyDeclarationSyntax.Identifier;
            return new RegionBlockInformation(
                        Start: regionBlock.Start,
                        Children: [], // TODO regionBlock.Children,
                        End: regionBlock.End,
                        Error: regionBlock.Error,
                        Node: propertyDeclarationSyntax,
                        Identifier: identifier,
                        Type: propertyDeclarationSyntax.Type
                    );
        }
        return new RegionBlockInformation(
                Start: regionBlock.Start,
                Children: [], // TODO regionBlock.Children,
                End: regionBlock.End,
                Error: regionBlock.Error,
            Node: default,
            Identifier: default,
            Type: default
        );
    }
}
