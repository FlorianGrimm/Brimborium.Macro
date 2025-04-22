using Brimborium.Macro.Model;
using Brimborium.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Brimborium.Macro.Parsing;

public sealed class MacroRegionScanner {
    private readonly string _SourceCode;
    private readonly SyntaxNode _SyntaxTreeRoot;
    private readonly SemanticModel _SemanticModel;

    public MacroRegionScanner(
        string sourceCode,
        SyntaxNode syntaxTreeRoot,
        SemanticModel semanticModel) {
        this._SourceCode = sourceCode;
        this._SyntaxTreeRoot = syntaxTreeRoot;
        this._SemanticModel = semanticModel;
    }

    public List<PositionAndSyntax> ScanRegions(CancellationToken cancellationToken) {
        var typeMacroAttribute = _SemanticModel.Compilation.GetTypeByMetadataName("Brimborium.Macro.MacroAttribute");

        List<PositionAndSyntax> listSyntaxTrivia = new(1024);
        foreach (var syntaxTrivia in _SyntaxTreeRoot.DescendantTrivia(null, false)) {
            if (syntaxTrivia.IsKind(SyntaxKind.WhitespaceTrivia)
                || syntaxTrivia.IsKind(SyntaxKind.EndOfLineTrivia)
                || syntaxTrivia.IsKind(SyntaxKind.EndOfFileToken)
                ) {
                continue;
            }
            if (syntaxTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia)) {
                var positionAndSyntax = this.ScanRegionDirective(syntaxTrivia);
                if (positionAndSyntax is null) { continue; }
                listSyntaxTrivia.Add(positionAndSyntax);
                continue;
            }
            if (syntaxTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)) {
                var positionAndSyntax = this.ScanEndRegionDirective(syntaxTrivia);
                if (positionAndSyntax is null) { continue; }
                listSyntaxTrivia.Add(positionAndSyntax);
                continue;
            }
            if (syntaxTrivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) {
                var positionAndSyntax = this.ScanMultiLineCommentTrivia(syntaxTrivia);
                if (positionAndSyntax is null) { continue; }
                listSyntaxTrivia.Add(positionAndSyntax);
                continue;
            }
            continue;
        }

#if false
        SyntaxNode? lastNpp = null;
        foreach (var node in syntaxTreeRoot.DescendantTokens(null, false)) {
            if (node.IsKind(SyntaxKind.OpenBracketToken)
                && node.Parent is AttributeListSyntax attributeListSyntax

                ) {
                AttributeSyntax? found = null;
                foreach (var attributeSyntax in attributeListSyntax.Attributes) {
                    var attributeTypeInfo = semanticModel.GetTypeInfo(attributeSyntax);
                    if (SymbolEqualityComparer.Default.Equals(attributeTypeInfo.Type, typeMacroAttribute)) {
                        found = attributeSyntax;
                        break;
                    }
                }
                if (found is { }) {
                    var npp = node.Parent.Parent;
                    if (npp is null
                        || ReferenceEquals(lastNpp, npp)) {
                        continue;
                    }
                    lastNpp = npp;
                    listSyntaxTrivia.Add(new PositionAndSyntax(found, npp));
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
#endif

        listSyntaxTrivia.Sort((x, y) => x.Position.CompareTo(y.Position));
        return listSyntaxTrivia;
    }

    public PositionAndSyntax? ScanRegionDirective(SyntaxTrivia currentTrivia) {
        if (currentTrivia.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective) {
            return null;
        }
        if (regionDirective.EndOfDirectiveToken.IsMissing) {
            return null;
        }

        var location = regionDirective.GetLocation();
        var regionText = new StringSlice(regionDirective.EndOfDirectiveToken.ToFullString());
        if (MacroParser.TryGetRegionBlockStart(regionText, out var commentMacroText)) {
            MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
            var regionDirectiveFullSpan = regionDirective.FullSpan;
            var positionStart = regionDirectiveFullSpan.Start;
#warning TODO            var macroRegionStart = new MacroRegionStart(macroText.ToString(), locationTag, regionDirective, location);
            //this.addRegionConst(_FullText, state.TextStart, positionStart);
            //var regionStart = new RegionStart(macroText.ToString(), locationTag, regionDirective, location);
            //if (this.addRegionStart(regionStart, location)) {
            //    state = state with { TextStart = regionDirective.FullSpan.End };
            //    return (true, state);
            //} else {
            //    return (false, state);
            //}
        }
        return null;
    }

    public PositionAndSyntax? ScanEndRegionDirective(SyntaxTrivia currentTrivia) {
#if false
   if (currentTrivia.IsDirective) {
            // var location = currentTrivia.GetLocation();
            var structure = (DirectiveTriviaSyntax)currentTrivia.GetStructure()!;
            if (structure is EndRegionDirectiveTriviaSyntax endRegionDirective) {
                var location = endRegionDirective.GetLocation();
                var positionStart = endRegionDirective.FullSpan.Start;
                this.addRegionConst(_FullText, state.TextStart, positionStart);
                state = state with { TextStart = endRegionDirective.FullSpan.End };

                string regionText;
                if (endRegionDirective.EndOfDirectiveToken.IsMissing) {
                    regionText = string.Empty;
                } else {
                    var fullSpan = endRegionDirective.EndOfDirectiveToken.FullSpan;
                    regionText = this._FullText.Substring(fullSpan.Start, fullSpan.Length);
                }
                if (MacroParser.TryGetRegionBlockEnd(regionText.AsSpan(), out var macroText, out var locationTag)) {
                    var regionEnd = new RegionEnd(macroText.ToString(), locationTag, endRegionDirective, location);
                    var success = this.addRegionEnd(regionEnd, location, _SyntaxTree, _SyntaxTreeRoot);
                    return (success, state);
                }
            }
        }
        return (true, state);
#endif
        return null;
    }

    public PositionAndSyntax? ScanMultiLineCommentTrivia(SyntaxTrivia currentTrivia) {
        var location = currentTrivia.GetLocation();
        var commentText = this.GetSourceCodeStringSlice(currentTrivia.FullSpan);

        //StringSlice commentText = this._SourceCode.AsSpan(currentTrivia.FullSpan.Start, currentTrivia.FullSpan.Length);
        var kind = MacroParser.TryGetMultiLineComment(commentText, out var commentMacroText);
        switch (kind) {
            case 1: {
                MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                var result = new MacroRegionStartBuilder(
                    text: commentText,
                    payload: macroText,
                    locationTag: locationTag,
                    syntaxTrivia: currentTrivia,
                    location: location);
                return new PositionAndSyntax(result);
            }

            case 2: {
                MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                var result = new MacroRegionEndBuilder(commentText, macroText, locationTag, SyntaxNodeType.SyntaxTrivia, currentTrivia, null, location);
                return new PositionAndSyntax(result);
            }

            default:
                return default;
        }
#if false
        switch (kind) {
            case 1: {
                var positionStart = currentTrivia.FullSpan.Start;
                this.addRegionConst(_FullText, state.TextStart, positionStart);
                state = state with { TextStart = positionStart + currentTrivia.FullSpan.Length };

                MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                var regionStart = new RegionStart(macroText.ToString(), locationTag, currentTrivia, location);
                var success = this.addRegionStart(regionStart, location);
                return (success, state);
            }

            case 2: {
                var positionStart = currentTrivia.FullSpan.Start;
                this.addRegionConst(_FullText, state.TextStart, positionStart);
                state = state with { TextStart = positionStart + currentTrivia.FullSpan.Length };

                MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                var regionEnd = new RegionEnd(macroText.ToString(), locationTag, currentTrivia, location);
                var success = this.addRegionEnd(regionEnd, location, _SyntaxTree, _SyntaxTreeRoot);
                return (success, state);
            }

            default:
                this.Error = "Error";
                return (false, state);
        }
#endif
    }

    private StringSlice GetSourceCodeStringSlice(TextSpan textSpan) {
        var range = new Range(start: textSpan.Start, end: textSpan.Start + textSpan.Length);
        var result = new StringSlice(this._SourceCode, range);
        return result;
    }

    public MacroRegionTreeNode ParseRegions(List<PositionAndSyntax> listPositionAndSyntax, CancellationToken cancellationToken) {
        IMacroRegionTreeNodeBuilder currentBuilder = new MacroRegionTreeNodeBuilder(null);
        var stack = new List<IMacroRegionTreeNodeBuilder>(32);
        stack.Add(currentBuilder);
        cancellationToken.ThrowIfCancellationRequested();
        int positionStart = 0;
        foreach (var positionAndSyntax in listPositionAndSyntax) {
            var nodeBuilder = positionAndSyntax.MacroRegionNodeBuilder;
            if (nodeBuilder is null) { continue; }
            if (nodeBuilder is MacroRegionStartBuilder macroRegionStartBuilder) {
                positionStart = AddConst(currentBuilder, positionStart, nodeBuilder.Text);

                var macroRegionBlockBuilder = new MacroRegionBlockBuilder(default);
                macroRegionBlockBuilder.Start = macroRegionStartBuilder;
                stack.Add(macroRegionBlockBuilder);
                currentBuilder.AddChild(macroRegionBlockBuilder);
                currentBuilder = macroRegionBlockBuilder;
            } else if (nodeBuilder is MacroRegionEndBuilder macroRegionEndBuilder) {
                if (stack.Count < 2) {
                    throw new Exception("Unbalanced region end");
                }
                var stackTop = stack[stack.Count - 1];
                if (stackTop is MacroRegionBlockBuilder macroRegionBlockBuilder) {
                    positionStart = AddConst(currentBuilder, positionStart, nodeBuilder.Text);

                    macroRegionBlockBuilder.End = macroRegionEndBuilder;
                    stack.RemoveAt(stack.Count - 1);
                    currentBuilder = stack[stack.Count - 1];
                } else {
                    throw new Exception("Unbalanced region end");
                }
            } else {
                // currentBuilder.AddChild(nodeBuilder);
                throw new Exception("Unexpected");
            }

            /*
                        if (node.Kind == SyntaxNodeType.RegionStart) {
                            var newNode = new MacroRegionTreeNodeBuilder(node);
                            stack.Add(newNode);
                            currentBuilder.AddChild(newNode);
                            currentBuilder = newNode;
                        } else if (node.Kind == SyntaxNodeType.RegionEnd) {
                            if (stack.Count < 2) {
                                throw new Exception("Unbalanced region end");
                            }
                            stack.RemoveAt(stack.Count - 1);
                            currentBuilder = stack.Last();
                        } else {
                            currentBuilder.AddChild(node);
                        }
            */
            //if (positionAndSyntax.MacroRegionNodeBuilder is { } macroRegionNodeBuilder) {
            //    currentBuilder.Add(macroRegionNodeBuilder);
            //} else if (positionAndSyntax.SyntaxTrivia is { } syntaxTrivia) {
            //    var node = new MacroRegionNodeBuilder(syntaxTrivia);
            //    currentBuilder.Add(node);
            //} else if (positionAndSyntax.Attribute is { } attribute) {
            //    var node = new MacroRegionNodeBuilder(attribute);
            //    currentBuilder.Add(node);
            //}
        }
        {
            var length = this._SourceCode.Length;
            if (positionStart < length) {
                var textSlice = this.GetSourceCodeStringSlice(new TextSpan(start: positionStart, length: length - positionStart));
                var macroRegionConstant = new MacroRegionConstantBuilder(textSlice, null);
                currentBuilder.AddChild(macroRegionConstant);
            }
        }

        var result = currentBuilder.Build();
        return result;

        int AddConst(IMacroRegionTreeNodeBuilder currentBuilder, int positionStart, StringSlice text) {
            var (offset, length) = text.GetOffsetAndLength();
            if (positionStart < offset) {
                var textSlice = this.GetSourceCodeStringSlice(new TextSpan(start: positionStart, length: offset - positionStart));
                var macroRegionConstant = new MacroRegionConstantBuilder(textSlice, null);
                currentBuilder.AddChild(macroRegionConstant);
                positionStart = offset + length;
            }

            return positionStart;
        }
    }
}


public sealed record class PositionAndSyntax(
    int Position,
    IMacroRegionNodeBuilder? MacroRegionNodeBuilder = default,
    SyntaxTrivia? SyntaxTrivia = default,
    AttributeSyntax? Attribute = default,
    SyntaxNode? SyntaxNode = default
) {
    public PositionAndSyntax(IMacroRegionNodeBuilder macroRegionNodeBuilder)
        : this(
            Position: macroRegionNodeBuilder.Location?.SourceSpan.Start ?? 0,
            MacroRegionNodeBuilder: macroRegionNodeBuilder,
            SyntaxTrivia: null,
            Attribute: null,
            SyntaxNode: null
        ) {
    }

    public PositionAndSyntax(SyntaxTrivia syntaxTrivia)
    : this(
        Position: syntaxTrivia.FullSpan.Start,
        MacroRegionNodeBuilder: default,
        SyntaxTrivia: syntaxTrivia,
        Attribute: null,
        SyntaxNode: null
        ) {
    }

    public PositionAndSyntax(AttributeSyntax? Attribute, SyntaxNode syntaxNode)
    : this(
        Position: syntaxNode.FullSpan.Start,
        MacroRegionNodeBuilder: default,
        SyntaxTrivia: null,
        Attribute: Attribute,
        SyntaxNode: syntaxNode
        ) {
    }
}