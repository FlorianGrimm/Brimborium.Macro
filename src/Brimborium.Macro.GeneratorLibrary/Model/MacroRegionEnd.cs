
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Diagnostics.CodeAnalysis;

namespace Brimborium.Macro.Model;

public record struct MacroRegionEnd(
    string? Text,
    LocationTag LocationTag,
    SyntaxNodeType Kind,
    SyntaxTrivia? SyntaxTrivia,
    EndRegionDirectiveTriviaSyntax? RegionDirective,
    Location? Location
) {
    public static MacroRegionEnd Empty
        => new MacroRegionEnd(null, new LocationTag(null, 0), SyntaxNodeType.None, null, null, null);

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroRegionEnd"/> struct with syntax trivia.
    /// </summary>
    /// <param name="Text">The text associated with the end of the region block.</param>
    /// <param name="LocationTag">The location tag</param>
    /// <param name="SyntaxTrivia">The syntax trivia associated with the end of the region block.</param>
    /// <param name="Location">The location of the end of the region block.</param>
    public MacroRegionEnd(string? Text, LocationTag LocationTag, SyntaxTrivia SyntaxTrivia, Location? Location = default)
        : this(Text, LocationTag, SyntaxNodeType.SyntaxTrivia, SyntaxTrivia, default, Location ?? SyntaxTrivia.GetLocation()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroRegionEnd"/> struct with an end region directive.
    /// </summary>
    /// <param name="Text">The text associated with the end of the region block.</param>
    /// <param name="LocationTag">The location tag</param>
    /// <param name="RegionDirective">The end region directive trivia syntax.</param>
    /// <param name="Location">The location of the end of the region block.</param>
    public MacroRegionEnd(string? Text, LocationTag LocationTag, EndRegionDirectiveTriviaSyntax RegionDirective, Location? Location = default)
        : this(Text, LocationTag, SyntaxNodeType.RegionDirectiveTriviaSyntax, default, RegionDirective, Location ?? RegionDirective.GetLocation()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroRegionEnd"/> struct with an end region directive.
    /// </summary>
    /// <param name="Text">The text associated with the end of the region block.</param>
    /// <param name="LocationTag">The location tag</param>
    /// <param name="Location">The location of the end of the region block.</param>
    public MacroRegionEnd(string? Text, LocationTag LocationTag, Location Location)
        : this(Text, LocationTag, SyntaxNodeType.AttributeSyntax, default, default, Location) { }

    /// <summary>
    /// Tries to get the syntax trivia associated with the end of the region block.
    /// </summary>
    /// <param name="syntaxTrivia">The syntax trivia, if available.</param>
    /// <param name="location">The location of the syntax trivia, if available.</param>
    /// <returns>True if the syntax trivia is available; otherwise, false.</returns>
    public bool TryGetSyntaxTrivia(
        [MaybeNullWhen(false)] out SyntaxTrivia syntaxTrivia,
        [MaybeNullWhen(false)] out Location location
        ) {
        if (this.Kind == SyntaxNodeType.SyntaxTrivia) {
            if (this.SyntaxTrivia is { } thisSyntaxTrivia) {
                syntaxTrivia = thisSyntaxTrivia;
                location = this.Location ?? thisSyntaxTrivia.GetLocation();
                return true;
            } else {
                throw new Exception();
            }
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
        if (this.Kind == SyntaxNodeType.RegionDirectiveTriviaSyntax) {
            if (this.RegionDirective is { } thisRegionDirective) {
                regionDirective = thisRegionDirective!;
                location = this.Location ?? thisRegionDirective.GetLocation();
                return true;
            } else {
                throw new Exception();
            }
        } else {
            regionDirective = default;
            location = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the location associated with the end of the region block.
    /// </summary>
    /// <param name="location">the location</param>
    /// <returns>true if found</returns>
    public bool TryGetLocation([MaybeNullWhen(false)] out Location location) {
        if (this.Kind == SyntaxNodeType.SyntaxTrivia) {
            if (this.SyntaxTrivia is { } thisSyntaxTrivia) {
                location = this.Location ?? thisSyntaxTrivia.GetLocation();
                return true;
            } else {
                throw new Exception();
            }
        } else if (this.Kind == SyntaxNodeType.RegionDirectiveTriviaSyntax) {
            if (this.RegionDirective is { } thisRegionDirective) {
                location = this.Location ?? thisRegionDirective.GetLocation();
                return true;
            } else {
                throw new Exception();
            }
        } else {
            location = this.Location;
            return null != this.Location;
        }
    }

    public MacroRegionEnd WithLocationTag(LocationTag locationTag) {
        if (this.LocationTag.Equals(locationTag)) {
            return this;
        } else {
            return this with {
                LocationTag = locationTag
            };
        }
    }

    internal MacroRegionEnd ToBuilder() {
        throw new NotImplementedException();
    }

    public bool HasValue => !(this.Kind == SyntaxNodeType.None);
    public bool IsEmpty => this.Kind == SyntaxNodeType.None;
}
