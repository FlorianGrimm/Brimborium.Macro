#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057

using Brimborium.Macro.Commands;
using Brimborium.Macro.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Text;

namespace Brimborium.Macro.Parse;

public sealed record class ParseRegionsResult(
    DocumentRegionTree DocumentRegionTree,
    RegionBlock? RegionBlockAtLocation,
    string? Error
    );

public sealed record class DocumentRegionTree(
    string FilePath,
    List<RegionBlock> Tree
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
    /// <param name="tree"></param>
    /// <param name="fullText"></param>
    /// <param name="locationToSearch"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static ParseRegionsResult ParseRegions(
        string filePath,
        SyntaxTree tree,
        SyntaxNode root,
        SemanticModel semanticModel,
        string? fullText,
        Location? locationToSearch,
        CancellationToken cancellationToken) {
        var parser = new MacroParserRegions(locationToSearch);
        return parser.parseRegionsImpl(filePath, tree, root, semanticModel, fullText, cancellationToken);
    }

    private string? Error = default;

    private readonly RegionBlock _Root;
    private readonly Stack<RegionBlock> _StackRegionBlock = new();
    private RegionBlock _CurrentRegionBlock;

    private RegionBlock? _RegionBlockAtLocation = default;
    private Location? _LocationToSearch = default;
    private HashSet<Location> _HsKnownLocation = new();

    private MacroParserRegions(Location? locationToSearch) {
        this._Root = new RegionBlock(new RegionStart("Root", new LocationTag(), ParserNodeOrTriviaKind.None, null, null, null, null, null));
        //this._StackRegionBlock.Push(this._Root);
        this._CurrentRegionBlock = this._Root;
        this._LocationToSearch = locationToSearch;
    }

    private bool addRegionStart(RegionStart regionStart, Location? location) {
        if (location is null) {
            if (!regionStart.TryGetLocation(out location)) {
                this.Error = "No location";
                return false;
            }
        }

        {
            if (this._HsKnownLocation.Contains(location)) {
                return true;
            } else {
                this._HsKnownLocation.Add(location);
                var regionBlock = new RegionBlock(regionStart);
                this._StackRegionBlock.Push(this._CurrentRegionBlock);
                this._CurrentRegionBlock = regionBlock;
                if (this._LocationToSearch is { } locationToSearch
                    && locationToSearch.Equals(location)) {
                    this._RegionBlockAtLocation = regionBlock;
                }
                return true;
            }
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

        if (this._CurrentRegionBlock is null) {
            this.Error = "No currentRegionBlock";
            return false;
        } else if (this._CurrentRegionBlock.Start.Kind == ParserNodeOrTriviaKind.None) {
            this.Error = "No currentRegionBlock.Start";
            return false;
        } else if (this._CurrentRegionBlock.Start.Kind != regionEnd.Kind) {
            this.Error = "Mismatch RegionBlock Kind";
            return false;
        // } else if (!this._HsKnownLocation.Add(location)) {
        //     return true;
        } else {
            var currentRegionBlock = this._CurrentRegionBlock;
            var currentRegionBlockWithEnd = currentRegionBlock with {
                End = regionEnd
            };

            RegionBlockAnalyse.GetRegionBlockInformation(currentRegionBlockWithEnd, syntaxTree, root);

            if (0 == this._StackRegionBlock.Count) {
                this.Error = "No StackRegionBlock";
                return false;
            }

            var currentRegionBlockNext = this._StackRegionBlock.Pop();
            this._CurrentRegionBlock = currentRegionBlockNext;
            currentRegionBlockNext.Children.Add(currentRegionBlockWithEnd);

            return true;
            /*
            if (0 < this._StackRegionBlock.Count) {
                this._StackRegionBlock.Pop();
                if (0 < this._StackRegionBlock.Count) {
                    this._CurrentRegionBlock = this._StackRegionBlock.Peek();
                } else {
                    this._CurrentRegionBlock = null;
                }
                return true;
            } else {
                this.Error = "No StackRegionBlock"; // Cannot be...
                return false;
            }
            */
        }
    }

    private ParseRegionsResult parseRegionsImpl(
        string FilePath,
        SyntaxTree syntaxTree,
        SyntaxNode syntaxTreeRoot,
        SemanticModel semanticModel,
        string? fullText = default,
        CancellationToken cancellationToken = default
        ) {

        var rootNode = syntaxTree.GetRoot();
        var typeMacroAttribute = semanticModel.Compilation.GetTypeByMetadataName("Brimborium.Macro.MacroAttribute");

        List<PositionAndSyntax> listSyntaxTrivia = new(1024);
        foreach (var syntaxTrivia in rootNode.DescendantTrivia(null, false)) {
            if (syntaxTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                || syntaxTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)
                || syntaxTrivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                ) {
                listSyntaxTrivia.Add(new PositionAndSyntax(syntaxTrivia));
            }
        }
        SyntaxNode? lastNpp = null;
        foreach (var node in rootNode.DescendantTokens(null, false)) {
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

        foreach (var (_, currentTriviaQ, currentAttribute, currentNode) in listSyntaxTrivia) {
            if (currentTriviaQ.HasValue) {
                var currentTrivia = currentTriviaQ.Value;
                var token = currentTrivia.Token;
                var node = token.Parent;
                if (currentTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia)) {
                    if (currentTrivia.IsDirective) {
                        var location = currentTrivia.GetLocation();
                        if (this._HsKnownLocation.Contains(location)) {
                            continue;
                        }
                        var structure = (DirectiveTriviaSyntax)currentTrivia.GetStructure()!;
                        if (structure is RegionDirectiveTriviaSyntax regionDirective) {
                            if (!regionDirective.EndOfDirectiveToken.IsMissing) {
                                location = regionDirective.GetLocation();
                                if (this._HsKnownLocation.Contains(location)) {
                                    continue;
                                }
                                var regionText = regionDirective.EndOfDirectiveToken.ToFullString().AsSpan();
                                if (MacroParser.TryGetRegionBlockStart(regionText, out var commentMacroText)) {
                                    MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                                    if (this.addRegionStart(
                                        new RegionStart(macroText.ToString(), locationTag, regionDirective, location),
                                        location)) {
                                        continue;
                                    } else {
                                        return new ParseRegionsResult(
                                            new DocumentRegionTree(FilePath, this._Root.Children),
                                            this._RegionBlockAtLocation, this.Error);
                                    }
                                }
                            }
                        }
                    }
                } else if (currentTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)) {
                    if (currentTrivia.IsDirective) {
                        var location = currentTrivia.GetLocation();
                        if (this._HsKnownLocation.Contains(location)) {
                            throw new Exception();
                            //continue;
                        }
                        var structure = (DirectiveTriviaSyntax)currentTrivia.GetStructure()!;
                        if (structure is EndRegionDirectiveTriviaSyntax endRegionDirective) {
                            if (this._CurrentRegionBlock is null) {
                            } else {
                                location = endRegionDirective.GetLocation();
                                if (this._HsKnownLocation.Contains(location)) {
                                    throw new Exception();
                                    // continue;
                                }
                                string regionText = (endRegionDirective.EndOfDirectiveToken.IsMissing)
                                    ? string.Empty
                                    : endRegionDirective.EndOfDirectiveToken.Text;
                                if (MacroParser.TryGetRegionBlockEnd(regionText.AsSpan(), out var macroText, out var locationTag)) {
                                    if (this.addRegionEnd(new RegionEnd(macroText.ToString(), locationTag, endRegionDirective, location), location, syntaxTree, syntaxTreeRoot)) {
                                        continue;
                                    } else {
                                        return new ParseRegionsResult(new DocumentRegionTree(FilePath, this._Root.Children), this._RegionBlockAtLocation, this.Error);
                                    }
                                }
                            }
                        }
                    }
                } else if (currentTrivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) {
                    var location = currentTrivia.GetLocation();
                    if (this._HsKnownLocation.Contains(location)) {
                        throw new Exception();
                        //continue;
                    }
                    if (fullText is null) {
                        fullText = syntaxTree.GetText(cancellationToken).ToString() ?? string.Empty;
                    }
                    ReadOnlySpan<char> commentText = fullText.AsSpan(currentTrivia.FullSpan.Start, currentTrivia.FullSpan.Length);

                    switch (MacroParser.TryGetMultiLineComment(commentText, out var commentMacroText)) {
                        case 1: {
                            MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                            if (this.addRegionStart(new RegionStart(macroText.ToString(), locationTag, currentTrivia, location), location)) {
                                continue;
                            } else {
                                return new ParseRegionsResult(new DocumentRegionTree(FilePath, this._Root.Children), this._RegionBlockAtLocation, this.Error);
                            }
                        }
                        case 2: {
                            MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                            if (this.addRegionEnd(new RegionEnd(macroText.ToString(), locationTag, currentTrivia, location), location, syntaxTree, syntaxTreeRoot)) {
                                continue;
                            } else {
                                return new ParseRegionsResult(new DocumentRegionTree(FilePath, this._Root.Children), this._RegionBlockAtLocation, this.Error);
                            }
                        }
                        default: continue;
                    }
                }
            } else if (currentNode is { } node && currentAttribute is { } attribute) {
                if (node is ClassDeclarationSyntax classDeclarationSyntax){
                    // classDeclarationSyntax.Identifier
                    var location = node.GetLocation();
                    addRegionStart(
                        new RegionStart(
                            null, 
                            new LocationTag(), 
                            attribute,
                            node, 
                            location
                        ), 
                        location);
                    addRegionEnd(
                        new RegionEnd(
                            Text:null, 
                            LocationTag:new LocationTag(), 
                            location
                        ), 
                        location, 
                        syntaxTree, syntaxTreeRoot);
                }
            }
        }

        if (0 != this._StackRegionBlock.Count) {
            this.Error = "Unbalaced";
        }
        return new ParseRegionsResult(new DocumentRegionTree(FilePath, this._Root.Children), this._RegionBlockAtLocation, this.Error);
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

    public PositionAndSyntax(AttributeSyntax? Attribute,SyntaxNode syntaxNode)
    : this(
        Position: syntaxNode.FullSpan.Start,
        SyntaxTrivia: null,
        Attribute: Attribute,
        SyntaxNode: syntaxNode
        ) {
    }
}