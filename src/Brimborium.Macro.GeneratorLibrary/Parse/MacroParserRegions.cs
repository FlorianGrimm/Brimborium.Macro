#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057

using Brimborium.Macro.Commands;
using Brimborium.Macro.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;
using System.Configuration;
using System.Text;

namespace Brimborium.Macro.Parse;

public sealed record class ParseRegionsResult(
    DocumentRegionTree DocumentRegionTree,
    RegionBlock? RegionBlockAtLocation,
    string? Error
    );

public sealed record class DocumentRegionTree(
    string FilePath,
    ImmutableArray<RegionBlock> Tree
    ) {

    public void Generate(string sourceCode, StringBuilder sbOut) {
        int pos = 0;
        this.Generate(sourceCode, ref pos, sbOut);
        sbOut.Append(sourceCode.AsSpan(pos));
    }

    public void Generate(string sourceCode, ref int pos, StringBuilder sbOut) {
        foreach (var regionBlock in this.Tree) {
            regionBlock.AppendPrefix(sourceCode, ref pos, sbOut);
            regionBlock.Generate(sourceCode, ref pos, sbOut);
        }
    }
}

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
        SyntaxNode treeRoot,
        SemanticModel semanticModel,
        string? fullText,
        Location? locationToSearch,
        CancellationToken cancellationToken) {
        var parser = new MacroParserRegions();
        // TODO: locationToSearch
        fullText ??= syntaxTree.GetText(cancellationToken).ToString() ?? string.Empty;
        return parser.parseRegionsImpl(filePath, syntaxTree, treeRoot, semanticModel, fullText, cancellationToken);
    }

    private string? Error = default;

    private readonly Stack<RegionBlock> _StackRegionBlock = new();

    private RegionBlock? _RegionBlockAtLocation = default;

    private MacroParserRegions() {
        var root = new RegionBlock(new RegionStart("Root", new LocationTag(), ParserNodeOrTriviaKind.None, null, null, null, null, null));
        this._StackRegionBlock.Push(root);
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

            //if (this._LocationToSearch is { } locationToSearch
            //    && locationToSearch.Equals(location)) {
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
        } else if (currentRegionBlock.Start.Kind == ParserNodeOrTriviaKind.None) {
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
            var currentRegionBlockWithEndInformation = currentRegionBlockWithEnd with {
                Information = information
            };

            if (0 == this._StackRegionBlock.Count) {
                this.Error = "No StackRegionBlock";
                return false;
            }

            var currentRegionBlockParentNext = this._StackRegionBlock.Pop()
                .WithReplaceLastChild(currentRegionBlock, currentRegionBlockWithEndInformation);
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
                    Kind: ParserNodeOrTriviaKind.Constant,
                    SyntaxTrivia: null,
                    RegionDirective: null,
                    Attribute: null,
                    SyntaxNode: null,
                    Location: null),
                Children: [],
                End: new RegionEnd(
                    Text: null,
                    LocationTag: new LocationTag(),
                    Kind: ParserNodeOrTriviaKind.Constant,
                    SyntaxTrivia: null,
                    RegionDirective: null,
                    Location: null),
                Error: null,
                Information: null
                );

        var currentRegionBlock = this._StackRegionBlock.Pop();
        var nextRegionBlock = currentRegionBlock.WithAddChild(regionBlock);
        this._StackRegionBlock.Push(nextRegionBlock);
        return true;
    }

    private ParseRegionsResult parseRegionsImpl(
        string FilePath,
        SyntaxTree syntaxTree,
        SyntaxNode syntaxTreeRoot,
        SemanticModel semanticModel,
        string fullText,
        CancellationToken cancellationToken
        ) {

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
        var position = 0;

        foreach (var (_, currentTriviaQ, currentAttribute, currentNode) in listSyntaxTrivia) {
            if (currentTriviaQ.HasValue) {
                var currentTrivia = currentTriviaQ.Value;
                var token = currentTrivia.Token;
                var node = token.Parent;
                if (currentTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia)) {
                    if (currentTrivia.IsDirective) {
                        var location = currentTrivia.GetLocation();

                        var structure = (DirectiveTriviaSyntax)currentTrivia.GetStructure()!;
                        if (structure is RegionDirectiveTriviaSyntax regionDirective) {
                            if (!regionDirective.EndOfDirectiveToken.IsMissing) {
                                location = regionDirective.GetLocation();

                                var regionText = regionDirective.EndOfDirectiveToken.ToFullString().AsSpan();
                                if (MacroParser.TryGetRegionBlockStart(regionText, out var commentMacroText)) {
                                    MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                                    var positionStart = regionDirective.FullSpan.Start;
                                    this.addRegionConst(fullText, position, positionStart);
                                    var regionStart = new RegionStart(macroText.ToString(), locationTag, regionDirective, location);
                                    if (this.addRegionStart(regionStart, location)) {
                                        position = regionDirective.FullSpan.End;
                                        continue;
                                    } else {
                                        return CreateResult();
                                    }
                                }
                            }
                        }
                    }
                } else if (currentTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)) {
                    if (currentTrivia.IsDirective) {
                        var location = currentTrivia.GetLocation();

                        var structure = (DirectiveTriviaSyntax)currentTrivia.GetStructure()!;
                        if (structure is EndRegionDirectiveTriviaSyntax endRegionDirective) {
                            location = endRegionDirective.GetLocation();
                            var positionStart = endRegionDirective.FullSpan.Start;
                            this.addRegionConst(fullText, position, positionStart);
                            position = endRegionDirective.FullSpan.End;
                            string regionText = (endRegionDirective.EndOfDirectiveToken.IsMissing)
                                ? string.Empty
                                : endRegionDirective.EndOfDirectiveToken.Text;
                            if (MacroParser.TryGetRegionBlockEnd(regionText.AsSpan(), out var macroText, out var locationTag)) {
                                var regionEnd = new RegionEnd(macroText.ToString(), locationTag, endRegionDirective, location);
                                if (this.addRegionEnd(regionEnd, location, syntaxTree, syntaxTreeRoot)) {
                                    continue;
                                } else {
                                    return CreateResult();
                                }
                            }
                        }
                    }
                } else if (currentTrivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) {
                    var location = currentTrivia.GetLocation();
                    ReadOnlySpan<char> commentText = fullText.AsSpan(currentTrivia.FullSpan.Start, currentTrivia.FullSpan.Length);

                    switch (MacroParser.TryGetMultiLineComment(commentText, out var commentMacroText)) {
                        case 1: {
                            var positionStart = currentTrivia.FullSpan.Start;
                            this.addRegionConst(fullText, position, positionStart);
                            position = positionStart + currentTrivia.FullSpan.Length;

                            MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                            var regionStart = new RegionStart(macroText.ToString(), locationTag, currentTrivia, location);
                            if (this.addRegionStart(regionStart, location)) {
                                continue;
                            } else {
                                return CreateResult();
                            }
                        }
                        case 2: {
                            var positionStart = currentTrivia.FullSpan.Start;
                            this.addRegionConst(fullText, position, positionStart);
                            position = positionStart + currentTrivia.FullSpan.Length;

                            MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                            var regionEnd = new RegionEnd(macroText.ToString(), locationTag, currentTrivia, location);
                            if (this.addRegionEnd(regionEnd, location, syntaxTree, syntaxTreeRoot)) {
                                continue;
                            } else {
                                return CreateResult();
                            }
                        }
                        default: continue;
                    }
                }
            } else if (currentNode is { } node && currentAttribute is { } attribute) {
                // classDeclarationSyntax.Identifier
                var location = node.GetLocation();
                var positionStart = node.FullSpan.Start;
                this.addRegionConst(fullText, position, positionStart);
                position = node.FullSpan.End;
                var regionStart = new RegionStart(
                    null,
                    new LocationTag(),
                    attribute,
                    node,
                    location
                );
                var regionEnd = new RegionEnd(
                    Text: null,
                    LocationTag: new LocationTag(),
                    location
                );
                this.addRegionStart(regionStart, location);
                this.addRegionEnd(regionEnd, location, syntaxTree, syntaxTreeRoot);
            }
        }

        this.addRegionConst(fullText, position, fullText.Length);

        if (0 != this._StackRegionBlock.Count) {
            this.Error = "Unbalaced";
        }

        return CreateResult();

        ParseRegionsResult CreateResult() { 
            var root= this._StackRegionBlock.Pop();
            var result = new DocumentRegionTree(FilePath, root.Children);
            return new ParseRegionsResult(result, this._RegionBlockAtLocation, this.Error);
        }
    }
}

internal record class PositionAndSyntax(
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