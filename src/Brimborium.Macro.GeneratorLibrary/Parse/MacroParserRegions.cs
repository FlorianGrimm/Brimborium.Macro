#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057

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
        foreach(var regionBlock in this.Tree) {
            regionBlock.AppendPrefix(sourceCode, ref pos, sbOut);
            regionBlock.Generate(sourceCode, ref pos, sbOut);
        }
    }
}

public class MacroParserRegions {

    public static ParseRegionsResult ParseRegions(
        string filePath,
        SyntaxTree tree,
        string? fullText,
        Location? locationToSearch,
        CancellationToken cancellationToken) {
        var parser = new MacroParserRegions(locationToSearch);
        return parser.parseRegionsImpl(filePath, tree, fullText, cancellationToken);
    }

    private string? Error = default;

    private readonly RegionBlock _Root;
    private readonly Stack<RegionBlock> _StackRegionBlock = new();
    private RegionBlock _CurrentRegionBlock;

    private RegionBlock? _RegionBlockAtLocation = default;
    private Location? _LocationToSearch = default;
    private HashSet<Location> _HsKnownLocation = new();

    private MacroParserRegions(Location? locationToSearch) {
        this._Root = new RegionBlock(new RegionStart("Root", new LocationTag(), ParserNodeOrTriviaKind.None, null, null, null));
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

                //if (this._CurrentRegionBlock is not null) {
                //    this._CurrentRegionBlock.Children.Add(regionBlock);
                //}
                //this._CurrentRegionBlock = regionBlock;
                //if (this._StackRegionBlock.Count == 0) {
                //    this._Result.Add(regionBlock);
                //}
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

    private bool addRegionEnd(RegionEnd regionEnd, Location? location) {
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
        } else if (!this._HsKnownLocation.Add(location)) {
            return true;
        } else {
            var currentRegionBlock = this._CurrentRegionBlock;
            var currentRegionBlockEnd = currentRegionBlock with {
                End = regionEnd
            };

            if (0 == this._StackRegionBlock.Count) {
                this.Error = "No StackRegionBlock";
                return false;
            }

            var currentRegionBlockNext = this._StackRegionBlock.Pop();
            this._CurrentRegionBlock = currentRegionBlockNext;
            currentRegionBlockNext.Children.Add(currentRegionBlockEnd);

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
        SyntaxTree tree,
        string? fullText = default,
        CancellationToken cancellationToken = default
        ) {

        var rootNode = tree.GetRoot();

#if true
        List<SyntaxTrivia> listSyntaxTrivia = new(1024);
        foreach (var syntaxTrivia in rootNode.DescendantTrivia(null, false)) {
            if (syntaxTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                || syntaxTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)
                || syntaxTrivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
                ) {
                listSyntaxTrivia.Add(syntaxTrivia);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        foreach (var trivia in listSyntaxTrivia) {
            var token = trivia.Token;
            var node = token.Parent;
            if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)) {
                if (trivia.IsDirective) {
                    var location = trivia.GetLocation();
                    if (this._HsKnownLocation.Contains(location)) {
                        continue;
                    }
                    var structure = (DirectiveTriviaSyntax)trivia.GetStructure()!;
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
            } else if (trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)) {
                if (trivia.IsDirective) {
                    var location = trivia.GetLocation();
                    if (this._HsKnownLocation.Contains(location)) {
                        continue;
                    }
                    var structure = (DirectiveTriviaSyntax)trivia.GetStructure()!;
                    if (structure is EndRegionDirectiveTriviaSyntax endRegionDirective) {
                        if (this._CurrentRegionBlock is null) {
                        } else {
                            location = endRegionDirective.GetLocation();
                            if (this._HsKnownLocation.Contains(location)) {
                                continue;
                            }
                            string regionText = (endRegionDirective.EndOfDirectiveToken.IsMissing)
                                ? string.Empty
                                : endRegionDirective.EndOfDirectiveToken.Text;
                            if (MacroParser.TryGetRegionBlockEnd(regionText.AsSpan(), out var macroText, out var locationTag)) {
                                if (this.addRegionEnd(new RegionEnd(macroText.ToString(), locationTag, endRegionDirective, location), location)) {
                                    continue;
                                } else {
                                    return new ParseRegionsResult(new DocumentRegionTree(FilePath, this._Root.Children), this._RegionBlockAtLocation, this.Error);
                                }
                            }
                        }
                    }
                }
            } else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) {
                var location = trivia.GetLocation();
                if (this._HsKnownLocation.Contains(location)) {
                    continue;
                }
                if (fullText is null) {
                    fullText = tree.GetText(cancellationToken).ToString() ?? string.Empty;
                }
                ReadOnlySpan<char> commentText = fullText.AsSpan(trivia.FullSpan.Start, trivia.FullSpan.Length);

                switch (MacroParser.TryGetMultiLineComment(commentText, out var commentMacroText)) {
                    case 1: {
                        MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                        if (this.addRegionStart(new RegionStart(macroText.ToString(), locationTag, trivia, location), location)) {
                            continue;
                        } else {
                            return new ParseRegionsResult(new DocumentRegionTree(FilePath, this._Root.Children), this._RegionBlockAtLocation, this.Error);
                        }
                    }
                    case 2: {
                        MacroParser.SplitLocationTag(commentMacroText, out var macroText, out var locationTag);
                        if (this.addRegionEnd(new RegionEnd(macroText.ToString(), locationTag, trivia, location), location)) {
                            continue;
                        } else {
                            return new ParseRegionsResult(new DocumentRegionTree(FilePath, this._Root.Children), this._RegionBlockAtLocation, this.Error);
                        }
                    }
                    default: continue;
                }
            }
        }
#endif

#if false
        foreach (var node in rootNode.DescendantNodesAndTokensAndSelf()) {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var trivia in node.GetLeadingTrivia()) {
                if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)) {
                    if (trivia.IsDirective) {
                        var location = trivia.GetLocation();
                        if (this._HsKnownLocation.Contains(location)) {
                            continue;
                        }
                        var structure = (DirectiveTriviaSyntax)trivia.GetStructure()!;
                        if (structure is RegionDirectiveTriviaSyntax regionDirective) {
                            if (!regionDirective.EndOfDirectiveToken.IsMissing) {
                                location = regionDirective.GetLocation();
                                if (this._HsKnownLocation.Contains(location)) {
                                    continue;
                                }
                                var regionText = regionDirective.EndOfDirectiveToken.ToFullString().AsSpan();
                                if (MacroParser.TryGetRegionBlockStart(regionText, out var macroText)) {
                                    if (this.addRegionStart(
                                        new RegionStart(macroText.ToString(), regionDirective, location),
                                        location)) {
                                        continue;
                                    } else {
                                        return new MacroParseRegionsResult(this._Result, this._RegionBlockAtLocation, this.Error);
                                    }
                                }
                            }
                        }
                    }
                } else if (trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia)) {
                    if (trivia.IsDirective) {
                        var location = trivia.GetLocation();
                        if (this._HsKnownLocation.Contains(location)) {
                            continue;
                        }
                        var structure = (DirectiveTriviaSyntax)trivia.GetStructure()!;
                        if (structure is EndRegionDirectiveTriviaSyntax endRegionDirective) {
                            if (this._CurrentRegionBlock is null) {
                            } else {
                                location = endRegionDirective.GetLocation();
                                if (this._HsKnownLocation.Contains(location)) {
                                    continue;
                                }
                                string regionText = (endRegionDirective.EndOfDirectiveToken.IsMissing)
                                    ? string.Empty
                                    : endRegionDirective.EndOfDirectiveToken.Text;
                                if (MacroParser.TryGetRegionBlockEnd(regionText.AsSpan(), out var macroText)) {
                                    if (this.addRegionEnd(new RegionEnd(macroText.ToString(), endRegionDirective, location), location)) {
                                        continue;
                                    } else {
                                        return new MacroParseRegionsResult(this._Result, this._RegionBlockAtLocation, this.Error);
                                    }
                                }
                            }
                        }
                    }
                } else if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) {
                    var location = trivia.GetLocation();
                    if (this._HsKnownLocation.Contains(location)) {
                        continue;
                    }
                    if (fullText is null) {
                        fullText = tree.GetText().ToString() ?? string.Empty;
                    }
                    ReadOnlySpan<char> commentText = fullText.AsSpan(trivia.FullSpan.Start, trivia.FullSpan.Length);

                    switch (MacroParser.TryGetMultiLineComment(commentText, out var macroText)) {
                        case 1: {
                            if (this.addRegionStart(new RegionStart(macroText.ToString(), trivia, location), location)) {
                                continue;
                            } else {
                                return new MacroParseRegionsResult(this._Result, this._RegionBlockAtLocation, this.Error);
                            }
                        }
                        case 2: {
                            if (this.addRegionEnd(new RegionEnd(commentText.ToString(), trivia, location), location)) {
                                continue;
                            } else {
                                return new MacroParseRegionsResult(this._Result, this._RegionBlockAtLocation, this.Error);
                            }
                        }
                        default: continue;
                    }
                }
            }
        }
#endif
        if (this._CurrentRegionBlock is not null) {
            this.Error = "No EndRegionDirectiveTrivia";
        }
        return new ParseRegionsResult(new DocumentRegionTree(FilePath, this._Root.Children), this._RegionBlockAtLocation, this.Error);
    }
}

