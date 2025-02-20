#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;

namespace Brimborium.Macro.Parse;
public class RegionBlock {
    public string? Error { get; set; }
    public RegionStart Start { get; set; }
    public List<RegionBlock> Children { get; } = new();
    public RegionEnd End { get; set; }
}

public enum ParserNodeOrTriviaKind { None, SyntaxTrivia, RegionDirectiveTriviaSyntax };

public record struct RegionStart(
    string? Text,
    ParserNodeOrTriviaKind Kind,
    SyntaxTrivia SyntaxTrivia,
    RegionDirectiveTriviaSyntax? RegionDirective,
    Location? Location
) {
    public RegionStart(
        string? Text,
        SyntaxTrivia SyntaxTrivia,
        Location? Location = default)
        : this(Text,
              ParserNodeOrTriviaKind.SyntaxTrivia, SyntaxTrivia, default,
              Location ?? SyntaxTrivia.GetLocation()) { }

    public RegionStart(
        string? Text,
        RegionDirectiveTriviaSyntax RegionDirective,
        Location? Location = default)
        : this(Text,
              ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax, default, RegionDirective,
              Location ?? RegionDirective.GetLocation()) { }

    public bool TryGetSyntaxTrivia(
        [MaybeNullWhen(false)] out SyntaxTrivia syntaxTrivia,
        [MaybeNullWhen(false)] out Location location
        ) {
        if (this.Kind == ParserNodeOrTriviaKind.SyntaxTrivia) {
            syntaxTrivia = this.SyntaxTrivia;
            location = this.Location ?? this.SyntaxTrivia.GetLocation();
            return true;
        } else {
            syntaxTrivia = default;
            location = default;
            return false;
        }
    }
    public bool TryGetRegionDirective(
        [MaybeNullWhen(false)] out RegionDirectiveTriviaSyntax regionDirective,
        [MaybeNullWhen(false)] out Location location
        ) {
        if (this.Kind == ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax) {
            regionDirective = this.RegionDirective!;
            location = this.Location ?? this.RegionDirective!.GetLocation();
            return true;
        } else {
            regionDirective = default;
            location = default;
            return false;
        }
    }

    public bool TryGetLocation([MaybeNullWhen(false)] out Location location) {
        if (this.Kind == ParserNodeOrTriviaKind.SyntaxTrivia) {
            location = this.Location ?? this.SyntaxTrivia.GetLocation();
            return true;
        } else if (this.Kind == ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax) {
            location = this.Location ?? this.RegionDirective!.GetLocation();
            return true;
        } else {
            location = this.Location;
            return null != this.Location;
        }
    }
}

public record struct RegionEnd(
    string Text,
    ParserNodeOrTriviaKind Kind,
    SyntaxTrivia SyntaxTrivia,
    EndRegionDirectiveTriviaSyntax? RegionDirective,
    Location Location
) {
    public RegionEnd(string Text, SyntaxTrivia SyntaxTrivia, Location? Location = default)
        : this(Text, ParserNodeOrTriviaKind.SyntaxTrivia, SyntaxTrivia, default, Location ?? SyntaxTrivia.GetLocation()) { }

    public RegionEnd(string Text, EndRegionDirectiveTriviaSyntax RegionDirective, Location? Location = default)
        : this(Text, ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax, default, RegionDirective, Location?? RegionDirective.GetLocation()) { }


    public bool TryGetSyntaxTrivia(
        [MaybeNullWhen(false)] out SyntaxTrivia syntaxTrivia,
        [MaybeNullWhen(false)] out Location location
        ) {
        if (this.Kind == ParserNodeOrTriviaKind.SyntaxTrivia) {
            syntaxTrivia = this.SyntaxTrivia;
            location = this.Location ?? this.SyntaxTrivia.GetLocation();
            return true;
        } else {
            syntaxTrivia = default;
            location = default;
            return false;
        }
    }

    public bool TryGetRegionDirective(
        [MaybeNullWhen(false)] out EndRegionDirectiveTriviaSyntax regionDirective,
        [MaybeNullWhen(false)] out Location location
        ) {
        if (this.Kind == ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax) {
            regionDirective = this.RegionDirective!;
            location = this.Location ?? this.RegionDirective!.GetLocation();
            return true;
        } else {
            regionDirective = default;
            location = default;
            return false;
        }
    }

    public bool TryGetLocation([MaybeNullWhen(false)] out Location location) {
        if (this.Kind == ParserNodeOrTriviaKind.SyntaxTrivia) {
            location = this.Location ?? this.SyntaxTrivia.GetLocation();
            return true;
        } else if (this.Kind == ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax) {
            location = this.Location ?? this.RegionDirective!.GetLocation();
            return true;
        } else {
            location = this.Location;
            return null != this.Location;
        }
    }
}

