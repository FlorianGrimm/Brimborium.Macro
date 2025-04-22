using Brimborium.Macro.Parsing;
using Brimborium.Text;

using Microsoft.CodeAnalysis;

using System.Collections.Immutable;
using System.Text;

namespace Brimborium.Macro.Model;

public sealed record class MacroRegionBlock(
    StringSlice Text,
    Location? Location,
    MacroRegionStart? Start,
    ImmutableArray<MacroRegionNode> Children,
    MacroRegionEnd? End
    //string? Error
    ) : MacroRegionNode(Text, Location)
    , IMacroRegionNode<MacroRegionBlockBuilder> {
    public override IMacroRegionNodeBuilder ConvertToBuilder() => this.ToBuilder();
    public MacroRegionBlockBuilder ToBuilder() => new MacroRegionBlockBuilder(this);


#if false
    public static MacroRegionBlock Empty => new MacroRegionBlock(
        Start: default,
        Children: ImmutableArray<MacroRegionBlock>.Empty,
        End: default,
        Error: default);

    public MacroRegionBlock(MacroRegionStart Start)
        : this(
            Start: Start,
            Children: ImmutableArray<MacroRegionBlock>.Empty,
            End: default,
            Error: default) {
    }

    public MacroRegionBlockBuilder ToBuilder() {
        List<MacroRegionBlockBuilder> builderChildren = new(this.Children.Length);
        foreach (MacroRegionBlock child in Children) {
            builderChildren.Add(child.ToBuilder());
        }

        var result= new MacroRegionBlockBuilder(
            Start: this.Start?.ToBuilder() ?? new MacroRegionStartBuilder(),
              Children: builderChildren,
              End: this.End?.ToBuilder() ?? new MacroRegionEndBuilder() ,
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
    public MacroRegionBlock WithAddChild(MacroRegionBlock MacroRegionBlock) {
        return this with {
            Children = this.Children.Add(MacroRegionBlock)
        };
    }

    public MacroRegionBlock WithStartLocationTag(LocationTag locationTag) {
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

    public MacroRegionBlock WithEndLocationTag(LocationTag locationTag) {
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

    public MacroRegionBlock WithStart(MacroRegionStart MacroRegionStart) {
        if (this.Start.Equals(MacroRegionStart)) {
            return this;
        } else {
            return this with { Start = MacroRegionStart };
        }
    }

    public MacroRegionBlock WithReplaceLastChild(MacroRegionBlock valueOld, MacroRegionBlock valueNew) {
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

    public MacroRegionBlock WithEnd(MacroRegionEnd regionEnd) {
        if (this.End.Equals(regionEnd)) {
            return this;
        } else {
            return this with { End = regionEnd };
        }
    }
#endif
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class MacroRegionBlockBuilder(MacroRegionBlock? source)
    : MacroRegionNodeBuilder<MacroRegionBlock>(source)
    , IMacroRegionTreeNodeBuilder {
    protected override void Awake(MacroRegionBlock source) {
        base.Awake(source);
        this._Start = source.Start?.ToBuilder();
        if (0 < source.Children.Length) {
            this._Children = new(source.Children.Length);
            foreach (var instance in source.Children) {
                var builder = instance.ConvertToBuilder();
                this._Children.Add(builder);
            }
        }
        this._End = source.End?.ToBuilder();
    }

    private MacroRegionStartBuilder? _Start;
    public MacroRegionStartBuilder? Start {
        get {
            if (this._Source is { } source) {
                return source.Start?.ToBuilder();
            } else {
                return this._Start;
            }
        }
        set {
            this.EnsureAwake();
            this._Start = value;
        }
    }
    private List<IMacroRegionNodeBuilder>? _Children;
    public List<IMacroRegionNodeBuilder> Children {
        get {
            this.EnsureAwake();
            return (this._Children ??= new());
        }
        set {
            this.EnsureAwake();
            this._Children = value;
        }
    }

    private MacroRegionEndBuilder? _End;
    public MacroRegionEndBuilder? End {
        get {
            this.EnsureAwake();
            return this._End;
        }
        set {
            this.EnsureAwake();
            this._End = value;
        }
    }

    public override MacroRegionBlock Build() {
        if (this._Source is { } source) {
            return source;
        } else {
            ImmutableArray<MacroRegionNode> targetChildren;
            if (this._Children is { Count: > 0 } children) {
                List<MacroRegionNode> listChildren = new();
                foreach (var child in children) {
                    var instance = child.ConvertToInstance();
                    listChildren.Add(instance);
                }
                targetChildren = listChildren.ToImmutableArray();
            } else {
                targetChildren = ImmutableArray<MacroRegionNode>.Empty;
            }
            return new MacroRegionBlock(
                Text: this.Text,
                Location: this.Location,
                Start: this._Start?.Build(),
                Children: targetChildren,
                End: this._End?.Build()
                );
        }
    }

    public void AddChild(IMacroRegionNodeBuilder child) {
        this.EnsureAwake();
        (this._Children ??= new()).Add(child);
    }

    MacroRegionTreeNode IMacroRegionNodeBuilder<MacroRegionTreeNode>.Build() {
        throw new NotImplementedException();
    }

    private string GetDebuggerDisplay() {
        
        return $"Block:{this.Start?.Kind},{this.Children.Count},{this.End?.Location}";
    }
}