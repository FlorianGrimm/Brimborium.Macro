#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Brimborium.Macro;

public class MacroParseRegions {
    public static MacroParseRegionsResult ParseRegions(
        SyntaxTree tree,
        Location? locationToSearch,
        CancellationToken cancellationToken) {
        var parser = new MacroParseRegions(locationToSearch);
        return parser.parseRegionsImpl(tree, cancellationToken);
    }

    private string? Error = default;

    private List<RegionBlock> _Result = new();
    private Stack<RegionBlock> _StackRegionBlock = new();

    private RegionBlock? _RegionBlockAtLocation = default;
    private RegionBlock? _CurrentRegionBlock = default;
    private Location? _LocationToSearch = default;
    private HashSet<Location> _HsKnownLocation = new();

    private MacroParseRegions(Location? locationToSearch) {
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
                var regionBlock = new RegionBlock() { Start = regionStart };

                if (this._CurrentRegionBlock is not null) {
                    this._CurrentRegionBlock.Children.Add(regionBlock);
                }
                this._CurrentRegionBlock = regionBlock;
                if (this._StackRegionBlock.Count == 0) {
                    this._Result.Add(regionBlock);
                }
                this._StackRegionBlock.Push(regionBlock);
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
            this._CurrentRegionBlock.End = regionEnd;
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
        }
    }

    private MacroParseRegionsResult parseRegionsImpl(
        SyntaxTree tree,

        CancellationToken cancellationToken) {
        string? fullText = default;

        var rootNode = tree.GetRoot();

        //List<SyntaxToken> listSyntaxToken = new(1024);
        //foreach (var token in rootNode.DescendantTokens()) {
        //    if (token.IsKind(SyntaxKind.RegionDirectiveTrivia)
        //        || token.IsKind(SyntaxKind.EndRegionDirectiveTrivia)
        //        || token.IsKind(SyntaxKind.MultiLineCommentTrivia)
        //        ) {
        //        listSyntaxToken.Add(token);
        //    }
        //}

        //foreach (var syntaxToken in listSyntaxToken) {
        //    var node = syntaxToken.Parent;
        //    if (node is null) { continue; }

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
        if (this._CurrentRegionBlock is not null) {
            this.Error = "No EndRegionDirectiveTrivia";
        }
        return new MacroParseRegionsResult(this._Result, this._RegionBlockAtLocation, this.Error);
    }
}


public sealed record class MacroParseRegionsResult(
    List<RegionBlock> Result,
    RegionBlock? RegionBlockAtLocation,
    string? Error
    );
