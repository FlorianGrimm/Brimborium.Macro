#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057

using Brimborium.Macro.Parse;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Primitives;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Text;

namespace Brimborium.Macro.Model;

/// <summary>
/// Describes a region block in a source file - a start and a end optionally children within.
/// </summary>
/// <param name="Start">The start information of the region block.</param>
/// <param name="Children">The list of child region blocks.</param>
/// <param name="End">The end information of the region block.</param>
/// <param name="Error">The error message associated with the region block.</param>
public record class RegionBlock(
    RegionStart Start,
    ImmutableArray<RegionBlock> Children,
    RegionEnd? End,
    string? Error,
    RegionBlockInformation? Information
    ) {
    public RegionBlock(RegionStart Start)
        : this(Start, [], default, default, default) {
    }

    public void AppendPrefix(string sourceCode, ref int pos, StringBuilder sbOut) {
        if (this.Start.TryGetLocation(out var startLocation)) {
            var startSourceSpan = startLocation.SourceSpan;
            var length = startSourceSpan.Start - pos;
            if (0 < length) {
                sbOut.Append(sourceCode.AsSpan(pos, length));
            }
            pos += startSourceSpan.Length;
        }
    }

    public void Generate(string sourceCode, ref int pos, StringBuilder sbOut) {
        if (this.Start.Kind == ParserNodeOrTriviaKind.SyntaxTrivia) {
            sbOut.Append("/* Macro ");
            var startText = this.Start.Text.AsSpan();
            MacroParser.TrimLeftWhitespaceNoNewLine(ref startText);
            MacroParser.TrimRightWhitespaceNoNewLine(ref startText);
            sbOut.Append(startText);
            this.Start.LocationTag.Generate(sbOut);
            sbOut.Append(" */");

            if (0 == this.Children.Length) {
                // insert content
                if (this.Start.TryGetLocation(out var startLocation)
                    && this.End is { } end
                    && end.TryGetLocation(out var endLocation)) {
                    sbOut.Append(sourceCode.AsSpan(
                        startLocation.SourceSpan.End,
                        endLocation.SourceSpan.Start - startLocation.SourceSpan.End));
                }
            } else {
                // insert content until the first child
                if (this.Start.TryGetLocation(out var startLocation)
                   && this.End is { } end
                   && end.TryGetLocation(out var endLocation)) {
                    sbOut.Append(sourceCode.AsSpan(
                        startLocation.SourceSpan.End,
                        endLocation.SourceSpan.Start - startLocation.SourceSpan.End));
                }
            }

            sbOut.Append("/* EndMacro");
            this.End?.LocationTag.Generate(sbOut);
            sbOut.Append(" */");

        } else if (this.Start.Kind == ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax) {
            sbOut.Append("#region Macro ");
            var startText = this.Start.Text.AsSpan();
            MacroParser.TrimLeftWhitespaceNoNewLine(ref startText);
            MacroParser.TrimRightWhitespaceNoNewLine(ref startText);
            sbOut.Append(startText);
            this.Start.LocationTag.Generate(sbOut);
            sbOut.AppendLine();

            if (0 == this.Children.Length) {
                // insert content
                if (this.Start.TryGetLocation(out var startLocation)
                    && this.End is { } end
                    && end.TryGetLocation(out var endLocation)) {
                    sbOut.Append(sourceCode.AsSpan(
                        startLocation.SourceSpan.End,
                        endLocation.SourceSpan.Start - startLocation.SourceSpan.End));
                }
            } else {
                // insert content until the first child
                if (this.Start.TryGetLocation(out var startLocation)
                     && this.End is { } end
                     && end.TryGetLocation(out var endLocation)) {
                    sbOut.Append(sourceCode.AsSpan(
                        startLocation.SourceSpan.End,
                        endLocation.SourceSpan.Start - startLocation.SourceSpan.End));
                }
            }

            sbOut.Append("#endregion");
            this.End?.LocationTag.Generate(sbOut);
            sbOut.AppendLine();
        } else if (this.Start.Kind == ParserNodeOrTriviaKind.AttributeSyntax) {
            // this.Start
        } else {
        }
        {
            if (this.End is { } end
                && end.TryGetLocation(out var endLocation)) {
                pos = endLocation.SourceSpan.End;
            }
        }
    }

    public RegionBlock WithAddChild(RegionBlock regionBlock) {
        return this with {
            Children = this.Children.Add(regionBlock)
        };
    }

    public RegionBlock WithStartLocationTag(LocationTag locationTag) {
        if (this.Start.LocationTag.Equals(locationTag)) {
            return this;
        } else {
            return this with {
                Start = this.Start with {
                    LocationTag = locationTag
                }
            };
        }
    }

    public RegionBlock WithEndLocationTag(LocationTag locationTag) {
        if (this.End is { } end) {
            if (end.LocationTag.Equals(locationTag)) {
                return this;
            } else {
                return this with {
                    End = end with {
                        LocationTag = locationTag
                    }
                };
            }
        } else {
            return this;
        }
    }

    public RegionBlock WithReplaceLastChild(RegionBlock valueOld, RegionBlock valueNew) {
        if (this.Children.Length == 0) {
            throw new InvalidOperationException();
        }
        if (this.Children[^1].Start != valueOld.Start) {
            throw new InvalidOperationException();
        }

        return this with {
            Children = this.Children.RemoveAt(this.Children.Length - 1).Add(valueNew)
        };
    }
}

public record struct LocationTag(
    string? FilePath,
    int LineIdentifier) {
    public void Generate(StringBuilder sbOut) {
        if (this.FilePath is { } filePath && 0 < this.LineIdentifier) {
            sbOut.Append(" #");
            sbOut.Append(filePath);
        } else if (this.FilePath is null && 0 < this.LineIdentifier) {
            sbOut.Append(" #");
            sbOut.Append(this.LineIdentifier);
        }
    }

    public bool DoesExists() => 0 < this.LineIdentifier;
}

/// <summary>
/// Describes a region syntax used.
/// </summary>
public enum ParserNodeOrTriviaKind {
    None,
    Constant,
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
    AttributeSyntax? Attribute,
    SyntaxNode? SyntaxNode,
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
              ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax, default, RegionDirective,
              null, null,
              Location ?? RegionDirective.GetLocation()) { }

    public RegionStart(
        string? Text,
        LocationTag LocationTag,
        AttributeSyntax Attribute,
        SyntaxNode SyntaxNode,
        Location? Location = default)
        : this(Text, LocationTag,
              ParserNodeOrTriviaKind.AttributeSyntax, default, default,
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
    string? Text,
    LocationTag LocationTag,
    ParserNodeOrTriviaKind Kind,
    SyntaxTrivia? SyntaxTrivia,
    EndRegionDirectiveTriviaSyntax? RegionDirective,
    Location? Location
) {
    /// <summary>
    /// Initializes a new instance of the <see cref="RegionEnd"/> struct with syntax trivia.
    /// </summary>
    /// <param name="Text">The text associated with the end of the region block.</param>
    /// <param name="LocationTag">The location tag</param>
    /// <param name="SyntaxTrivia">The syntax trivia associated with the end of the region block.</param>
    /// <param name="Location">The location of the end of the region block.</param>
    public RegionEnd(string? Text, LocationTag LocationTag, SyntaxTrivia SyntaxTrivia, Location? Location = default)
        : this(Text, LocationTag, ParserNodeOrTriviaKind.SyntaxTrivia, SyntaxTrivia, default, Location ?? SyntaxTrivia.GetLocation()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegionEnd"/> struct with an end region directive.
    /// </summary>
    /// <param name="Text">The text associated with the end of the region block.</param>
    /// <param name="LocationTag">The location tag</param>
    /// <param name="RegionDirective">The end region directive trivia syntax.</param>
    /// <param name="Location">The location of the end of the region block.</param>
    public RegionEnd(string? Text, LocationTag LocationTag, EndRegionDirectiveTriviaSyntax RegionDirective, Location? Location = default)
        : this(Text, LocationTag, ParserNodeOrTriviaKind.RegionDirectiveTriviaSyntax, default, RegionDirective, Location ?? RegionDirective.GetLocation()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegionEnd"/> struct with an end region directive.
    /// </summary>
    /// <param name="Text">The text associated with the end of the region block.</param>
    /// <param name="LocationTag">The location tag</param>
    /// <param name="Location">The location of the end of the region block.</param>
    public RegionEnd(string? Text, LocationTag LocationTag, Location Location)
        : this(Text, LocationTag, ParserNodeOrTriviaKind.AttributeSyntax, default, default, Location) { }

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

public record RegionBlockInformation(
    RegionBlock RegionBlock,
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