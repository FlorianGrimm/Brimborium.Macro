#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057

using Brimborium.Macro.Commands;
using Brimborium.Macro.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Brimborium.Macro.Parse;

public sealed record class ParseRegionsResult(
    DocumentRegionTree DocumentRegionTree,
    RegionBlock? RegionBlockAtLocation,
    string? Error
    );

public class MacroParserRegions {

    /// <summary>
    /// Parse the regions /* Macro */ #region Macro and Brimborium.Macro.MacroAttribute. 
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="syntaxTree"></param>
    /// <param name="fullText"></param>
    /// <param name="locationToSearch"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static ParseRegionsResult ParseRegions(
        string filePath,
        SyntaxTree syntaxTree,
        SyntaxNode syntaxTreeRoot,
        SemanticModel semanticModel,
        string? fullText,
        Location? locationToSearch,
        CancellationToken cancellationToken) {

        Stack<RegionBlock> stackRegionBlock = new();
        stackRegionBlock.Push(RegionBlock.Empty);
        fullText ??= syntaxTree.GetText(cancellationToken).ToString() ?? string.Empty;

        var parser = new MacroParserRegions(
            filePath,
            syntaxTree,
            syntaxTreeRoot,
            fullText,
            locationToSearch,
            stackRegionBlock
        );
        var listSyntaxTrivia = ScanRegions(syntaxTreeRoot, semanticModel, cancellationToken);

        var initialState = new ParseRegionState(0, listSyntaxTrivia, 0, fullText.Length);
        var (success, state) = parser.parseRegionsImpl(initialState, cancellationToken);
        if (success) {
            if (1 != stackRegionBlock.Count) {
                parser.Error = "Unbalaced";
            } else {
                parser.addRegionConst(fullText, state.TextStart, fullText.Length);
            }
        } else {
            parser.Error ??= "Error";
        }

        if (parser.Error != null) {
            var documentRegionTree = new DocumentRegionTree(filePath, RegionBlock.Empty);
            var result = new ParseRegionsResult(documentRegionTree, parser._RegionBlockAtLocation, parser.Error);
            return result;
        } else {
            var root = stackRegionBlock.Pop();
            var documentRegionTree = new DocumentRegionTree(filePath, root);
            // TODO: _LocationToSearch - parser._RegionBlockAtLocation
            var result = new ParseRegionsResult(documentRegionTree, parser._RegionBlockAtLocation, parser.Error);
            return result;
        }
    }
    private record struct ParseRegionState(
        int IndexPositionAndSyntax,
        List<PositionAndSyntax> ListPositionAndSyntax,
        int TextStart,
        int TextEnd
    );

    private string? Error = default;

    private readonly Stack<RegionBlock> _StackRegionBlock = new();

    private RegionBlock? _RegionBlockAtLocation = default;
    private string _FilePath;
    private SyntaxTree _SyntaxTree;
    private SyntaxNode _SyntaxTreeRoot;
    private string _FullText;
    private Location? _LocationToSearch;

    private MacroParserRegions(
        string filePath,
        SyntaxTree syntaxTree,
        SyntaxNode syntaxTreeRoot,
        string fullText,
        Location? locationToSearch,
        Stack<RegionBlock> stackRegionBlock
        ) {
        this._FilePath = filePath;
        this._SyntaxTree = syntaxTree;
        this._SyntaxTreeRoot = syntaxTreeRoot;
        this._FullText = fullText;
        this._LocationToSearch = locationToSearch;
        this._StackRegionBlock = stackRegionBlock;
    }

    private bool addRegionStart(RegionStart regionStart, Location? location) {
        if (location is null) {
            if (!regionStart.TryGetLocation(out location)) {
                this.Error = "No location";
                return false;
            }
        }

        {
            var currentRegionBlock = this._StackRegionBlock.Pop();
            var regionBlock = new RegionBlock(regionStart);
            var currentRegionBlockNext = currentRegionBlock.WithAddChild(regionBlock);
            this._StackRegionBlock.Push(currentRegionBlockNext);
            this._StackRegionBlock.Push(regionBlock);

            //if (this._LocationToSearch is { } _LocationToSearch
            //    && _LocationToSearch.Equals(location)) {
            //    this._RegionBlockAtLocation = regionBlock;
            //}
            return true;
        }
    }

    private bool addRegionEnd(
        RegionEnd regionEnd,
        Location? location,
        SyntaxTree syntaxTree,
        SyntaxNode root
        ) {
        if (location is null) {
            if (!regionEnd.TryGetLocation(out location)) {
                this.Error = "No location";
                return false;
            }
        }


        if (0 == this._StackRegionBlock.Count) {
            this.Error = "No StackRegionBlock";
            return false;
        }
        var currentRegionBlock = this._StackRegionBlock.Pop();

        if (currentRegionBlock is null) {
            this.Error = "No currentRegionBlock";
            return false;
        } else if (currentRegionBlock.Start.Kind == SyntaxNodeType.None) {
            this.Error = "No currentRegionBlock.Start";
            return false;
        } else if (currentRegionBlock.Start.Kind != regionEnd.Kind) {
            this.Error = "Mismatch RegionBlock Kind";
            return false;
            // } else if (!this._HsKnownLocation.Add(location)) {
            //     return true;
        } else {
            var currentRegionBlockWithEnd = currentRegionBlock with {
                End = regionEnd
            };
            var information = RegionBlockAnalyse.GetRegionBlockInformation(
                currentRegionBlockWithEnd, syntaxTree, root);
            //var currentRegionBlockWithEndInformation = currentRegionBlockWithEnd with {
            //    Information = information
            //};

            if (0 == this._StackRegionBlock.Count) {
                this.Error = "No StackRegionBlock";
                return false;
            }

            var currentRegionBlockParentNext = this._StackRegionBlock.Pop()
                .WithReplaceLastChild(currentRegionBlock, currentRegionBlockWithEnd);
            this._StackRegionBlock.Push(currentRegionBlockParentNext);

            return true;
        }
    }

    private bool addRegionConst(string fullText, int positionStart, int positionEnd) {
        if (positionStart >= positionEnd) {
            return true;
        }

        if (0 == this._StackRegionBlock.Count) {
            this.Error = "No StackRegionBlock";
            return false;
        }

        var regionBlock = new RegionBlock(
                Start: new RegionStart(fullText.AsSpan(positionStart, positionEnd - positionStart).ToString(),
                    LocationTag: new LocationTag(),
                    Kind: SyntaxNodeType.Constant,
                    SyntaxTrivia: null,
                    RegionDirective: null,
                    Attribute: null,
                    SyntaxNode: null,
                    Location: null),
                Children: [],
                End: new RegionEnd(
                    Text: null,
                    LocationTag: new LocationTag(),
                    Kind: SyntaxNodeType.Constant,
                    SyntaxTrivia: null,
                    RegionDirective: null,
                    Location: null),
                Error: null
                );

        var currentRegionBlock = this._StackRegionBlock.Pop();
        var nextRegionBlock = currentRegionBlock.WithAddChild(regionBlock);
        this._StackRegionBlock.Push(nextRegionBlock);
        return true;
    }


    public static List<PositionAndSyntax> ScanRegions(SyntaxNode syntaxTreeRoot, SemanticModel semanticModel, CancellationToken cancellationToken) {
        var typeMacroAttribute = semanticModel.Compilation.GetTypeByMetadataName("Brimborium.Macro.MacroAttribute");

        List<PositionAndSyntax> listSyntaxTrivia = new(1024);
        foreach (var syntaxTrivia in syntaxTreeRoot.DescendantTrivia(null, false)) {
            if (syntaxTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                || syntaxTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)
                || syntaxTrivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                ) {
                listSyntaxTrivia.Add(new PositionAndSyntax(syntaxTrivia));
            }
        }
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

        listSyntaxTrivia.Sort((x, y) => x.Position.CompareTo(y.Position));
        return listSyntaxTrivia;
    }
    
    private (bool success, ParseRegionState state) parseRegionsImpl(
        ParseRegionState state,
        CancellationToken cancellationToken
        ) {
        while (state.IndexPositionAndSyntax < state.ListPositionAndSyntax.Count) {
            cancellationToken.ThrowIfCancellationRequested();
            var (currentPosition, currentTriviaQ, currentAttribute, currentNode) = state.ListPositionAndSyntax[state.IndexPositionAndSyntax];
            if (state.TextEnd <= currentPosition) {

                return (true, state);
            }

            var nextIndex = state.IndexPositionAndSyntax + 1;
            var nextPosition = (nextIndex < state.ListPositionAndSyntax.Count)
                ? state.ListPositionAndSyntax[nextIndex].Position
                : state.TextEnd;

            bool success;
            if (currentTriviaQ.HasValue) {
                var currentTrivia = currentTriviaQ.Value;
                if (currentTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia)) {
                    (success, state) = this.parseRegionsImplRegionDirectiveTrivia(currentTrivia, state, cancellationToken);
                } else if (currentTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)) {
                    (success, state) = this.parseRegionsImplEndRegionDirectiveTrivia(currentTrivia, state, cancellationToken);
                } else if (currentTrivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) {
                    (success, state) = this.parseRegionsImplMultiLineCommentTrivia(currentTrivia, state, cancellationToken);
                } else {
                    this.Error = "Error";
                    return (false, state);
                }
            } else if (currentNode is { } node && currentAttribute is { } attribute) {
                (success, state) = this.parseRegionsImplNodeAttribute(node, attribute, state, cancellationToken);
            } else {
                this.Error = "Error";
                return (false, state);
            }
            if (!success || this.Error != null) { break; }
            state.IndexPositionAndSyntax++;
        }

        return (true, state);
    }

    private (bool success, ParseRegionState state) parseRegionsImplRegionDirectiveTrivia(
        SyntaxTrivia currentTrivia,
        ParseRegionState state,
        CancellationToken cancellationToken) {
        if (currentTrivia.IsDirective) {
            //var location = currentTrivia.GetLocation();

            var structure = (DirectiveTriviaSyntax)currentTrivia.GetStructure()!;
            if (structure is RegionDirectiveTriviaSyntax regionDirective) {
                if (!regionDirective.EndOfDirectiveToken.IsMissing) {
                    var location = regionDirective.GetLocation();

                    var regionText = regionDirective.EndOfDirectiveToken.ToFullString().AsSpan();
                    if (MacroParser.TryGetRegionBlockStart(regionText, out var commentMacroText)) {
                        MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                        var positionStart = regionDirective.FullSpan.Start;
                        this.addRegionConst(_FullText, state.TextStart, positionStart);
                        var regionStart = new RegionStart(macroText.ToString(), locationTag, regionDirective, location);
                        if (this.addRegionStart(regionStart, location)) {
                            state = state with { TextStart = regionDirective.FullSpan.End };
                            return (true, state);
                        } else {
                            return (false, state);
                        }
                    }
                }
            }
        }
        return (true, state);
    }

    private (bool success, ParseRegionState state) parseRegionsImplEndRegionDirectiveTrivia(
        SyntaxTrivia currentTrivia,
        ParseRegionState state,
        CancellationToken cancellationToken) {
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
    }

    private (bool success, ParseRegionState state) parseRegionsImplMultiLineCommentTrivia(
        SyntaxTrivia currentTrivia,
        ParseRegionState state,
        CancellationToken cancellationToken) {
        var location = currentTrivia.GetLocation();
        ReadOnlySpan<char> commentText = _FullText.AsSpan(currentTrivia.FullSpan.Start, currentTrivia.FullSpan.Length);
        var kind = MacroParser.TryGetMultiLineComment(commentText, out var commentMacroText);
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
    }

    private (bool success, ParseRegionState state) parseRegionsImplNodeAttribute(
        SyntaxNode node,
        AttributeSyntax attribute,
        ParseRegionState state,
        CancellationToken cancellationToken) {
        //        PropertyDeclarationSyntax? propertyDeclarationSyntax = node as PropertyDeclarationSyntax;

        // ClassDeclarationSyntax x;
        var location = node.GetLocation();
        var positionStart = node.FullSpan.Start;
        this.addRegionConst(_FullText, state.TextStart, positionStart);
        state = state with { TextStart = positionStart };

        var (s, e) = getRangeNodeAttribute(node, attribute, state, cancellationToken);

        var childNodes = node.ChildNodes().ToList();
        var regionStart = new RegionStart(
            null,
            new LocationTag(),
            attribute,
            node,
            location
        );
        this.addRegionStart(regionStart, location);

        var lastNode = node.GetLastToken(includeZeroWidth: true, includeSkipped: true, includeDirectives: true, includeDocumentationComments: true);
        if (lastNode.RawKind == 0) {

        } else {
            var innerPositionEnd = lastNode.GetLocation().SourceSpan.End;
        }

        var positionEnd = node.FullSpan.End;
        // is their a PositionAndSyntax between positionStart and positionEnd
        var nextIndex = state.IndexPositionAndSyntax + 1;
        if (nextIndex < state.ListPositionAndSyntax.Count) {
            var itemPositionAndSyntax = state.ListPositionAndSyntax[state.IndexPositionAndSyntax];
            var itemPosition = itemPositionAndSyntax.Position;
            if (positionStart < itemPosition && itemPosition < positionEnd) {
                var stateNext = state with { TextEnd = positionEnd };
                var (success, stateAfter) = this.parseRegionsImpl(stateNext, cancellationToken);
                if (!success) { return (false, stateAfter); }
                state = stateAfter;
            }
        }

        state = state with { TextStart = positionEnd };
        var regionEnd = new RegionEnd(
            Text: null,
            LocationTag: new LocationTag(),
            location
        );
        this.addRegionEnd(regionEnd, location, _SyntaxTree, _SyntaxTreeRoot);
        return (true, state);
    }

    private (int s, int e) getRangeNodeAttribute(SyntaxNode node, AttributeSyntax attribute, ParseRegionState state, CancellationToken cancellationToken) {
        var nodeFullSpan = node.FullSpan;
        var lastTokenSpan = node.GetLastToken().Span;

        if (node is PropertyDeclarationSyntax propertyDeclarationSyntax) {
            var s = propertyDeclarationSyntax.Identifier.Span.End;
            var e = propertyDeclarationSyntax.GetLastToken().Span.End;
            return (s, e);
        }
        return (0, 0);
    }
}

public sealed record class PositionAndSyntax(
    int Position,
    SyntaxTrivia? SyntaxTrivia,
    AttributeSyntax? Attribute,
    SyntaxNode? SyntaxNode
) {
    public PositionAndSyntax(SyntaxTrivia syntaxTrivia)
    : this(
        Position: syntaxTrivia.FullSpan.Start,
        SyntaxTrivia: syntaxTrivia,
        Attribute: null,
        SyntaxNode: null
        ) {
    }

    public PositionAndSyntax(AttributeSyntax? Attribute, SyntaxNode syntaxNode)
    : this(
        Position: syntaxNode.FullSpan.Start,
        SyntaxTrivia: null,
        Attribute: Attribute,
        SyntaxNode: syntaxNode
        ) {
    }
}