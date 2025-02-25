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

/// <summary>
/// Describes a region block in a source file - a start and a end optionally children within.
/// </summary>
/// <param name="Start">The start information of the region block.</param>
/// <param name="Children">The list of child region blocks.</param>
/// <param name="End">The end information of the region block.</param>
/// <param name="LocationTag">the location tag (#locationtag)</param>
/// <param name="Error">The error message associated with the region block.</param>
public record class RegionBlock(
    RegionStart Start,
    List<RegionBlock> Children,
    RegionEnd? End,
    LocationTag? LocationTag,
    string? Error
    ) {
    public RegionBlock(RegionStart Start) 
        : this(Start, [], default, default, default){
    }
    /// <summary>
    /// Gets or sets the error message associated with the region block.
    /// </summary>
    //public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the start information of the region block.
    /// </summary>
    //public RegionStart Start { get; set; }

    /// <summary>
    /// Gets the list of child region blocks.
    /// </summary>
    //public List<RegionBlock> Children { get; } = new();

    /// <summary>
    /// Gets or sets the end information of the region block.
    /// </summary>
    //public RegionEnd End { get; set; }

    /// <summary>
    /// Gets or sets the line identifier of the region block.
    /// </summary>
    //public LocationTag LocationTag { get; set; }

    //public RegionBlock CloneFlat() {
    //    var result = new RegionBlock() {
    //        Error = this.Error,
    //        Start = this.Start,
    //        End = this.End,
    //        LocationTag = this.LocationTag
    //    };
    //    result.Children.AddRange(this.Children);
    //    return result;
    //}
}

public record struct LocationTag(
    string? FilePath,
    int LineIdentifier);

/// <summary>
/// Describes a region syntax used.
/// </summary>
public enum ParserNodeOrTriviaKind {
    None,
    SyntaxTrivia,
    RegionDirectiveTriviaSyntax,
    AttributeSyntax,
};


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
    ParserNodeOrTriviaKind Kind,
    SyntaxTrivia? SyntaxTrivia,
    RegionDirectiveTriviaSyntax? RegionDirective,
    Location? Location
) {
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
              ParserNodeOrTriviaKind.SyntaxTrivia, SyntaxTrivia, default,
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
              ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax, default, RegionDirective,
              Location ?? RegionDirective.GetLocation()) { }

    /// <summary>
    /// Gets the parsed text associated with the start of the text.
    /// </summary>
    public readonly string? ParsedText =>
        (this.SyntaxTrivia is { } syntaxTrivia) ? syntaxTrivia.ToString()
        : (this.RegionDirective is { } regionDirective) ? regionDirective.ToString()
        : null;

    /// <summary>
    /// Gets the file path associated with the start of the text.
    /// </summary>
    public readonly string? FilePath => this.Location?.SourceTree?.FilePath;

    /// <summary>
    /// Gets the line number associated with the start of the text.
    /// </summary>
    public readonly int Line => (this.Location is { } location)
        ? (location.GetLineSpan().StartLinePosition.Line + 1)
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
        if (this.Kind == ParserNodeOrTriviaKind.SyntaxTrivia) {
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
        if (this.Kind == ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax) {
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
        if (this.Kind == ParserNodeOrTriviaKind.SyntaxTrivia) {
            if (this.SyntaxTrivia is { } thisSyntaxTrivia) {
                location = this.Location ?? thisSyntaxTrivia.GetLocation();
                return true;
            } else {
                throw new Exception();
            }
        } else if (this.Kind == ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax) {
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
}

/// <summary>
/// Describes the end of a region block.
/// </summary>
/// <param name="Text">The text associated with the end of the region block.</param>
/// <param name="Kind">The kind of this</param>
/// <param name="SyntaxTrivia">The syntax trivia associated with the end of the region block.</param>
/// <param name="RegionDirective">The EndRegionDirectiveTriviaSyntax - if Kind is RegionDirectiveTriviaSyntax</param>
/// <param name="Location">The location of the end of the region block.</param>
public record struct RegionEnd(
    string Text,
    LocationTag LocationTag,
    ParserNodeOrTriviaKind Kind,
    SyntaxTrivia? SyntaxTrivia,
    EndRegionDirectiveTriviaSyntax? RegionDirective,
    Location Location
) {
    /// <summary>
    /// Initializes a new instance of the <see cref="RegionEnd"/> struct with syntax trivia.
    /// </summary>
    /// <param name="Text">The text associated with the end of the region block.</param>
    /// <param name="SyntaxTrivia">The syntax trivia associated with the end of the region block.</param>
    /// <param name="Location">The location of the end of the region block.</param>
    public RegionEnd(string Text, LocationTag LocationTag, SyntaxTrivia SyntaxTrivia, Location? Location = default)
        : this(Text, LocationTag, ParserNodeOrTriviaKind.SyntaxTrivia, SyntaxTrivia, default, Location ?? SyntaxTrivia.GetLocation()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegionEnd"/> struct with an end region directive.
    /// </summary>
    /// <param name="Text">The text associated with the end of the region block.</param>
    /// <param name="RegionDirective">The end region directive trivia syntax.</param>
    /// <param name="Location">The location of the end of the region block.</param>
    public RegionEnd(string Text, LocationTag LocationTag, EndRegionDirectiveTriviaSyntax RegionDirective, Location? Location = default)
        : this(Text, LocationTag, ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax, default, RegionDirective, Location ?? RegionDirective.GetLocation()) { }


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
        if (this.Kind == ParserNodeOrTriviaKind.SyntaxTrivia) {
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
        if (this.Kind == ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax) {
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
        if (this.Kind == ParserNodeOrTriviaKind.SyntaxTrivia) {
            if (this.SyntaxTrivia is { } thisSyntaxTrivia) {
                location = this.Location ?? thisSyntaxTrivia.GetLocation();
                return true;
            } else {
                throw new Exception();
            }
        } else if (this.Kind == ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax) {
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
}

