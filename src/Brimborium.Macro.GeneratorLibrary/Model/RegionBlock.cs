#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057
#pragma warning disable IDE0301 // Simplify collection initialization

using Brimborium.Macro.Parse;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Primitives;

using System.Collections.Immutable;
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
    RegionEnd End,
    string? Error
    ) {
    public static RegionBlock Empty => new RegionBlock(
        RegionStart.Empty,
        Children: ImmutableArray<RegionBlock>.Empty,
        End: default,
        Error: default);

    public RegionBlock(RegionStart Start)
        : this(
            Start: Start,
            Children: ImmutableArray<RegionBlock>.Empty,
            End: default,
            Error: default) {
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
        if (this.Start.Kind == SyntaxNodeType.SyntaxTrivia) {
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
                   && this.End.HasValue
                   && this.End.TryGetLocation(out var endLocation)) {
                    sbOut.Append(sourceCode.AsSpan(
                        startLocation.SourceSpan.End,
                        endLocation.SourceSpan.Start - startLocation.SourceSpan.End));
                }
            }

            sbOut.Append("/* EndMacro");
            this.End.LocationTag.Generate(sbOut);
            sbOut.Append(" */");

        } else if (this.Start.Kind == SyntaxNodeType.RegionDirectiveTriviaSyntax) {
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
            this.End.LocationTag.Generate(sbOut);
            sbOut.AppendLine();
        } else if (this.Start.Kind == SyntaxNodeType.AttributeSyntax) {
            sbOut.Append("// TODO: class AttributeSyntax {");

            sbOut.Append("// TODO: class AttributeSyntax }");
        } else if (this.Start.Kind == SyntaxNodeType.Constant) {
            sbOut.Append(this.Start.Text);
        } else if (this.Start.Kind == SyntaxNodeType.None) {
        } else {
        }
        {
            if (this.End.TryGetLocation(out var endLocation)) {
                pos = endLocation.SourceSpan.End;
            }
        }
    }

    public void Generate(StringBuilder sbOut) {
        var start = this.Start;
        {
            if (start.Kind == SyntaxNodeType.None) {
            } else if (start.Kind == SyntaxNodeType.Constant) {
                sbOut.Append(start.Text);
            } else if (start.Kind == SyntaxNodeType.SyntaxTrivia) {
                sbOut.Append("/* Macro ");
                var startText = start.Text.AsSpan();
                MacroParser.TrimLeftWhitespaceNoNewLine(ref startText);
                MacroParser.TrimRightWhitespaceNoNewLine(ref startText);
                sbOut.Append(startText);
                start.LocationTag.Generate(sbOut);
                sbOut.Append(" */");
            } else if (start.Kind == SyntaxNodeType.RegionDirectiveTriviaSyntax) {
                sbOut.Append("#region Macro ");
                var startText = start.Text.AsSpan();
                MacroParser.TrimLeftWhitespaceNoNewLine(ref startText);
                MacroParser.TrimRightWhitespaceNoNewLine(ref startText);
                sbOut.Append(startText);
                start.LocationTag.Generate(sbOut);
                sbOut.AppendLine();
            } else if (start.Kind == SyntaxNodeType.AttributeSyntax) {
                sbOut.Append("// TODO: class AttributeSyntax {");
                sbOut.AppendLine();
                
            } else {
            }
        }
        {
            foreach (var child in this.Children) {
                child.Generate(sbOut);
            }
        }
        {
            if (this.End.HasValue) {
                if (this.End.Kind == SyntaxNodeType.None) {
                } else if (this.End.Kind == SyntaxNodeType.Constant) {
                    sbOut.Append(this.End.Text);
                } else if (this.End.Kind == SyntaxNodeType.SyntaxTrivia) {
                    sbOut.Append("/* EndMacro");
                    this.End.LocationTag.Generate(sbOut);
                    sbOut.Append(" */");
                } else if (this.End.Kind == SyntaxNodeType.RegionDirectiveTriviaSyntax) {
                    sbOut.Append("#endregion");
                    this.End.LocationTag.Generate(sbOut);
                    sbOut.AppendLine();
                } else if (this.End.Kind == SyntaxNodeType.AttributeSyntax) {
                    sbOut.Append("// TODO: class AttributeSyntax }");
                    sbOut.AppendLine();
                } else {
                }
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
        if (this.End.HasValue) {
            if (this.End.LocationTag.Equals(locationTag)) {
                return this;
            } else {
                return this with {
                    End = this.End with {
                        LocationTag = locationTag
                    }
                };
            }
        } else {
            return this;
        }
    }

    public RegionBlock WithStart(RegionStart regionStart) {
        if (this.Start.Equals(regionStart)) {
            return this;
        } else {
            return this with { Start = regionStart };
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

    public RegionBlock WithEnd(RegionEnd regionEnd) {
        if (this.End.Equals(regionEnd)) {
            return this;
        } else {
            return this with { End = regionEnd};
        }
    }
}
