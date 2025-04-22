using Brimborium.Macro.Parsing;

using System.Collections.Immutable;
using System.Text;

namespace Brimborium.Macro.Model;

public sealed class MacroRegionBlockBuilder : MacroRegionNodeBuilder {

    public static MacroRegionBlockBuilder Empty => new MacroRegionBlockBuilder(
        Start: default,
        Children: default,
        End: default,
        Error: default);

    public MacroRegionBlockBuilder(
        MacroRegionStartBuilder? Start,
        List<MacroRegionBlockBuilder>? Children,
        MacroRegionEndBuilder? End,
        string? Error
    ) {
        this.Start = Start;
        if (Children is { }) { 
            this.Children.AddRange(Children);
        }
        this.End = End;
        this.Error = Error;
    }

    public MacroRegionStart? Start { get; set; }
    public List<MacroRegionBlockBuilder> Children { get; private set; } = [];
    public MacroRegionEnd? End { get; set; }
    public string? Error { get; set; }


    public MacroRegionBlockBuilder(MacroRegionStart Start) {
        this.Start = Start;
    }

    public MacroRegionBlock Build() {
        List<MacroRegionBlock> children = new(this.Children.Count);
        foreach (var child in this.Children) {
            children.Add(child.Build());
        }
        var result = new MacroRegionBlock(
            Start: this.Start,
            Children: children.ToImmutableArray(),
            End: this.End,
            Error: this.Error);
        return result;
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

            if (0 == this.Children.Count) {
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

            if (0 == this.Children.Count) {
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

    public MacroRegionBlockBuilder WithStartLocationTag(LocationTag locationTag) {
        if (this.Start.LocationTag.Equals(locationTag)) {
            return this;
        }

        this.Start = this.Start with {
            LocationTag = locationTag
        };
        return this;
    }

    public MacroRegionBlockBuilder WithEndLocationTag(LocationTag locationTag) {
        if (this.End.HasValue) {
            if (this.End.LocationTag.Equals(locationTag)) {
                return this;
            }
        }
        this.End = this.End with {
            LocationTag = locationTag
        };
        return this;
    }

    public MacroRegionBlockBuilder WithStart(MacroRegionStart MacroRegionStart) {
        if (this.Start.Equals(MacroRegionStart)) {
            return this;
        }
        this.Start = MacroRegionStart;
        return this;
    }

    public MacroRegionBlockBuilder WithAddChild(MacroRegionBlockBuilder child) {
        this.Children.Add(child);
        return this;
    }

    public MacroRegionBlockBuilder WithReplaceLastChild(MacroRegionBlockBuilder valueOld, MacroRegionBlockBuilder valueNew) {
        if (this.Children.Count == 0) {
            throw new InvalidOperationException();
        }
        if (this.Children[^1].Start != valueOld.Start) {
            throw new InvalidOperationException();
        }

        this.Children[this.Children.Count - 1] = valueNew;
        return this;
    }

    public MacroRegionBlockBuilder WithEnd(MacroRegionEnd regionEnd) {
        if (this.End.Equals(regionEnd)) {
        return this;
        }
        this.End = regionEnd;
        return this;
    }
}
