#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057
#pragma warning disable IDE0301 // Simplify collection initialization

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Brimborium.Macro.Model;

public record RegionBlockInformation(
    RegionStart Start,
    ImmutableArray<RegionBlock> Children,
    RegionEnd? End,
    string? Error,
    SyntaxNode? Node,
    SyntaxToken? Identifier,
    TypeSyntax? Type
) {
    public string? IdentifierText => this.Identifier?.ToString();
    public string? TypeText => this.Type?.ToString();

    public bool TryGetPropertyDeclaration(
        [MaybeNullWhen(false)] out PropertyDeclarationSyntax propertyDeclarationSyntax
        ) {
        if (this.Node is PropertyDeclarationSyntax node) {
            propertyDeclarationSyntax = node;
            return true;
        } else {
            propertyDeclarationSyntax = default;
            return false;
        }
    }
}