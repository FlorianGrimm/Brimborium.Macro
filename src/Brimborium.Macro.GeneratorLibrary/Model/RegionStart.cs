#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057
#pragma warning disable IDE0301 // Simplify collection initialization

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Diagnostics.CodeAnalysis;

namespace Brimborium.Macro.Model;

/// <summary>
/// Describes the start of a region block.
/// </summary>
/// <param name="Text">The captured text</param>
/// <param name="LocationTag">the last part #line</param>
/// <param name="Kind">the kind</param>
/// <param name="SyntaxTrivia">SyntaxTrivia means comment</param>
/// <param name="RegionDirective">RegionDirective means #region</param>
/// <param name="Location">the location of this text</param>
public record struct RegionStart(
    string? Text,
    LocationTag LocationTag,
    SyntaxNodeType Kind,
    Nullable<SyntaxTrivia> SyntaxTrivia,
    RegionDirectiveTriviaSyntax? RegionDirective,
    AttributeSyntax? Attribute,
    SyntaxNode? SyntaxNode,
    Location? Location
) {
    public static RegionStart Empty
        => new RegionStart(null, new LocationTag(null, 0), SyntaxNodeType.None, null, null, null, null, null);

    /// <summary>
    /// for SyntaxTrivia - comment
    /// </summary>
    /// <param name="Text">The captured text</param>
    /// <param name="LocationTag">the last part #line</param>
    /// <param name="SyntaxTrivia">SyntaxTrivia means comment</param>
    /// <param name="Location">the location of this text</param>
    public RegionStart(
        string? Text,
        LocationTag LocationTag,
        SyntaxTrivia SyntaxTrivia,
        Location? Location = default)
        : this(Text,
              LocationTag,
              SyntaxNodeType.SyntaxTrivia, SyntaxTrivia, default,
              null, null,
              Location ?? SyntaxTrivia.GetLocation()) { }


    /// <summary>
    /// for RegionDirective - #region
    /// </summary>
    /// <param name="Text">The captured text</param>
    /// <param name="LocationTag">the last part #line</param>
    /// <param name="RegionDirective">RegionDirective means #region</param>
    /// <param name="Location">the location of this text</param>
    public RegionStart(
        string? Text,
        LocationTag LocationTag,
        RegionDirectiveTriviaSyntax RegionDirective,
        Location? Location = default)
        : this(Text, LocationTag,
              SyntaxNodeType.RegionDirectiveTriviaSyntax, default, RegionDirective,
              null, null,
              Location ?? RegionDirective.GetLocation()) { }

    public RegionStart(
        string? Text,
        LocationTag LocationTag,
        AttributeSyntax Attribute,
        SyntaxNode SyntaxNode,
        Location? Location = default)
        : this(Text, LocationTag,
              SyntaxNodeType.AttributeSyntax, default, default,
              Attribute, SyntaxNode,
              Location ?? Attribute.GetLocation()) { }

    /// <summary>
    /// Gets the parsed text associated with the start of the text.
    /// </summary>
    public readonly string? ParsedText =>
        this.SyntaxTrivia is { } syntaxTrivia ? syntaxTrivia.ToString()
        : this.RegionDirective is { } regionDirective ? regionDirective.ToString()
        : null;

    /// <summary>
    /// Gets the file path associated with the start of the text.
    /// </summary>
    public readonly string? FilePath => this.Location?.SourceTree?.FilePath;

    /// <summary>
    /// Gets the line number associated with the start of the text.
    /// </summary>
    public readonly int Line => this.Location is { } location
        ? location.GetLineSpan().StartLinePosition.Line + 1
        : 0;

    /// <summary>
    /// Tries to get the syntaxTrivia associated with the start of the region block.
    /// </summary>
    /// <param name="syntaxTrivia">the SyntaxTrivia if kind is SyntaxTrivia.</param>
    /// <param name="location">the location of the SyntaxTrivia</param>
    /// <returns>true if found.</returns>
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
    /// <summary>
    /// Tries to get the end region directive associated with the end of the region block.
    /// </summary>
    /// <param name="regionDirective">The end region directive, if available.</param>
    /// <param name="location">The location of the end region directive, if available.</param>
    /// <returns>True if the end region directive is available; otherwise, false.</returns>
    public bool TryGetRegionDirective(
        [MaybeNullWhen(false)] out RegionDirectiveTriviaSyntax regionDirective,
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
    /// <param name="location">The location, if available.</param>
    /// <returns>True if the location is available; otherwise, false.</returns>
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

    public RegionStart WithLocationTag(LocationTag locationTag) {
        if (this.LocationTag.Equals(locationTag)) {
            return this;
        } else {
            return this with {
                LocationTag = locationTag
            };
        }
    }
    public bool IsEmpty => this.Kind == SyntaxNodeType.None;

    public bool HasValue => !(this.Kind == SyntaxNodeType.None);
}
