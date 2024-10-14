﻿#pragma warning disable IDE1006 // Naming Styles

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using System.Text;
using System.Collections;

namespace Brimborium.Macro;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MacroCodeFixProvider)), Shared]
public class MacroCodeFixProvider : CodeFixProvider {
    public sealed override ImmutableArray<string> FixableDiagnosticIds {
        get { return ImmutableArray.Create(MacroAnalyzer.RunDiagnosticId); }
    }

    public sealed override FixAllProvider GetFixAllProvider() {
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
        if (context.Document.TryGetSyntaxRoot(out var syntaxRoot)) {
            //
        } else {
            syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (syntaxRoot is null) {
                return;
            }
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticLocation = diagnostic.Location;
        var diagnosticSpan = diagnosticLocation.SourceSpan;
        var diagnosticToken = syntaxRoot.FindToken(diagnosticSpan.Start, findInsideTrivia: true);

        RegisterCodeFixes(context, diagnostic, syntaxRoot, diagnosticToken);
    }

    private void RegisterCodeFixes(CodeFixContext context, Diagnostic diagnostic, SyntaxNode syntaxRoot, SyntaxToken diagnosticToken) {
        var diagnosticLocation = diagnostic.Location;

        if (diagnosticToken.Parent?.AncestorsAndSelf() is { } listAncestors) {
            foreach (var node in listAncestors) {
                if (node is RegionDirectiveTriviaSyntax nodeRegionDirectiveTriviaSyntax) {
                    if (diagnosticLocation == nodeRegionDirectiveTriviaSyntax.GetLocation()) {
                        // Register a code action that will invoke the fix.
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                title: CodeFixesResources.CodeFixTitle,
                                createChangedDocument: c => MacroRunAsync(context.Document, diagnosticLocation, c),
                                equivalenceKey: nameof(CodeFixesResources.CodeFixTitle)),
                            diagnostic);
                        return;
                    }
                }
            }

            var fullText = syntaxRoot.SyntaxTree.GetText().ToString() ?? string.Empty;
            foreach (var node in listAncestors) {
                foreach (var trivia in node.GetLeadingTrivia()) {
                    if (trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)) {
                        if (diagnosticLocation == trivia.GetLocation()) {
                            ReadOnlySpan<char> commentText = fullText.AsSpan(trivia.FullSpan.Start, trivia.FullSpan.Length);
                            if (1 == MacroParser.TryGetMultiLineComment(commentText, out var _)) {
                                context.RegisterCodeFix(
                                    CodeAction.Create(
                                        title: CodeFixesResources.CodeFixTitle,
                                        createChangedDocument: c => MacroRunAsync(context.Document, diagnosticLocation, c),
                                        equivalenceKey: nameof(CodeFixesResources.CodeFixTitle)),
                                    diagnostic);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }

    private static async Task<Document> MacroRunAsync(
        Document document,
        Location location,
        CancellationToken cancellationToken) {
        //if (document.TryGetSyntaxRoot(out var syntaxRoot)) {
        //    //
        //} else {
        //    syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        //    if (syntaxRoot is null) {
        //        return document;
        //    }
        //}

        if (document.TryGetSyntaxTree(out var syntaxTree)) {
            //
        } else {
            syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            if (syntaxTree is null) {
                return document;
            }
        }

        var macroParseRegionsResults = MacroParseRegions.ParseRegions(syntaxTree, location, cancellationToken);
        if (macroParseRegionsResults.Error is { }) { return document; }
        if (macroParseRegionsResults.RegionBlockAtLocation is not { } regionBlock) { return document; }
        var documentNext = await UpdateMacroRegionAsync(document, macroParseRegionsResults, regionBlock, cancellationToken);
        return documentNext;
    }

    private static async Task<Document> UpdateMacroRegionAsync(
        Document document,
        MacroParseRegionsResult macroParseRegionsResults,
        RegionBlock regionBlock,
        CancellationToken cancellationToken) {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var sourceString = sourceText.ToString();

        regionBlock.Start.Deconstruct(out var regionStartText, out var regionStartKind, out var regionStartSyntaxTrivia, out var regionStartRegionDirective, out var location);
        if ((regionStartKind == ParserNodeOrTriviaKind.None)
            || (regionStartRegionDirective is null && regionStartSyntaxTrivia.IsKind(SyntaxKind.None))
            || (location is null)
            ) { return document; }

        regionBlock.End.Deconstruct(out var regionEndText, out var regionEndKind, out var regionEndSyntaxTrivia, out var regionEndRegionDirective, out var regionEndLocation);
        if ((regionEndKind == ParserNodeOrTriviaKind.None)
            || (regionEndRegionDirective is null && regionEndSyntaxTrivia.IsKind(SyntaxKind.None))
            || (location is null)
            ) { return document; }

        TextSpan regionBlockStartSourceSpan;
        int endOfRegionStart;
        int startOfRegionEnd;
        int startOfMacroContent;
        int endOfMacroContent;
        if (regionStartRegionDirective is { }) {
            regionBlockStartSourceSpan = regionStartRegionDirective.FullSpan;
            endOfRegionStart = regionBlockStartSourceSpan.End;

            startOfMacroContent = endOfRegionStart;
            if (sourceString[startOfMacroContent] == '\r') {
                startOfMacroContent++;
            }
            if (sourceString[startOfMacroContent] == '\n') {
                startOfMacroContent++;
            }
        } else {
            regionBlockStartSourceSpan = regionStartSyntaxTrivia.FullSpan;
            endOfRegionStart = regionBlockStartSourceSpan.End;

            startOfMacroContent = endOfRegionStart;
            if (sourceString[startOfMacroContent] == '\r') {
                startOfMacroContent++;
            }
            if (sourceString[startOfMacroContent] == '\n') {
                startOfMacroContent++;
            }
        }

        if (regionBlock.End.Kind != ParserNodeOrTriviaKind.None) {
            TextSpan regionBlockEndSourceSpan;
            if (regionEndRegionDirective is { }) {
                regionBlockEndSourceSpan = regionEndRegionDirective.FullSpan;
                startOfRegionEnd = regionBlockEndSourceSpan.Start; // "#endregion\r\n    }\r\n}"
                startOfRegionEnd = MacroParser.GotoLeftWhileWhitespace(sourceString, startOfRegionEnd);

                endOfMacroContent = MacroParser.GotoLeftIfNewline(sourceString, startOfRegionEnd);
                endOfMacroContent = MacroParser.GotoLeftWhileWhitespace(sourceString, endOfMacroContent);
            } else {
                regionBlockEndSourceSpan = regionEndSyntaxTrivia.FullSpan;
                startOfRegionEnd = regionBlockEndSourceSpan.Start;

                startOfRegionEnd = MacroParser.GotoLeftWhileWhitespace(sourceString, startOfRegionEnd);
                endOfMacroContent = startOfRegionEnd;

                endOfMacroContent = MacroParser.GotoLeftIfNewline(sourceString, endOfMacroContent);
                endOfMacroContent = MacroParser.GotoLeftWhileWhitespace(sourceString, endOfMacroContent);
            }
        } else {
            return document;
        }

        //while (0 < startOfStartLine) {
        //    if (sourceString[startOfStartLine - 1] == '\n'
        //        || sourceString[startOfStartLine - 1] == '\r') { break; }
        //    startOfStartLine--;
        //}
        //var stringIndent = sourceString.Substring(startOfStartLine, startOfStartSyntax - startOfStartLine);

        System.Diagnostics.Debug.Assert(endOfRegionStart <= startOfMacroContent);
        System.Diagnostics.Debug.Assert(startOfMacroContent <= endOfMacroContent);
        System.Diagnostics.Debug.Assert(endOfMacroContent <= startOfRegionEnd);

        var stringIndent = "        ";
        var stringBefore = sourceString[..endOfRegionStart];
        var stringOldMacroContent = sourceString[startOfMacroContent..endOfMacroContent];
        var stringAfter = sourceString[startOfRegionEnd..];

        var filePath = document.FilePath;
        var folders = document.Folders;
        var stringPrevMacroContent = regionStartText;
        if (stringPrevMacroContent is null || stringPrevMacroContent.Length == 0) { return document; }
        string stringNextMacroContent = RunMacro(
            stringPrevMacroContent,
            stringIndent,
            cancellationToken);

        if (MacroParser.EqualsLines(stringOldMacroContent, stringNextMacroContent)) {
            return document;
        }

        // build the new source
        StringBuilder sbNext = new(sourceString.Length);
        sbNext.Append(stringBefore);
        if (MacroParser.NeedNewLine(stringBefore, stringNextMacroContent)) { sbNext.Append("\r\n"); }
        sbNext.Append(stringNextMacroContent);
        if (MacroParser.NeedNewLine(stringNextMacroContent, stringAfter)) { sbNext.Append("\r\n"); }
        sbNext.Append(stringAfter);
        var stringNext = sbNext.ToString();
        var sourceTextNext = SourceText.From(stringNext);
        var documentNext = document.WithText(sourceTextNext);
        /*
        if (documentNext.SupportsSyntaxTree) {
            var treeNext = await documentNext.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            if (treeNext is { }) {
                return document.WithSyntaxRoot(treeNext.GetRoot());
            }
        }
        */
        return documentNext;
    }

    private static string RunMacro(string macro, string stringIndent, CancellationToken cancellationToken) {
        return $"{stringIndent}// Macro: {macro} //";
    }


#warning WEICHEI
#if WEICHEI
    public async Task RegisterCodeFixesAsyncGone(CodeFixContext context) {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) { return; }

        // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the type declaration identified by the diagnostic.
        var declaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>().First();
        if (declaration is null) { return; }

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixesResources.CodeFixTitle,
                createChangedDocument: c => MakeConstAsync(context.Document, declaration, c),
                equivalenceKey: nameof(CodeFixesResources.CodeFixTitle)),
            diagnostic);
    }

#warning WEICHEI
    private static async Task<Document> MakeConstAsync(
        Document document,
        LocalDeclarationStatementSyntax localDeclaration,
        CancellationToken cancellationToken) {
        // Remove the leading trivia from the local declaration.
        SyntaxToken firstToken = localDeclaration.GetFirstToken();
        SyntaxTriviaList leadingTrivia = firstToken.LeadingTrivia;
        LocalDeclarationStatementSyntax trimmedLocal = localDeclaration.ReplaceToken(
            firstToken, firstToken.WithLeadingTrivia(SyntaxTriviaList.Empty));

        // Create a const token with the leading trivia.
        SyntaxToken constToken = SyntaxFactory.Token(leadingTrivia, SyntaxKind.ConstKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));

        // Insert the const token into the modifiers list, creating a new modifiers list.
        SyntaxTokenList newModifiers = trimmedLocal.Modifiers.Insert(0, constToken);

        // If the type of the declaration is 'var', create a new type name
        // for the inferred type.
        VariableDeclarationSyntax variableDeclaration = localDeclaration.Declaration;
        TypeSyntax variableTypeName = variableDeclaration.Type;
        if (variableTypeName.IsVar) {
            SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (semanticModel is null) { return document; }

            // Special case: Ensure that 'var' isn't actually an alias to another type
            // (e.g. using var = System.String).
            IAliasSymbol? aliasInfo = semanticModel.GetAliasInfo(variableTypeName, cancellationToken);
            if (aliasInfo == null) {
                // Retrieve the type inferred for var.
                ITypeSymbol? type = semanticModel.GetTypeInfo(variableTypeName, cancellationToken).ConvertedType;
                if (type is null) { return document; }

                // Special case: Ensure that 'var' isn't actually a type named 'var'.
                if (type.Name != "var") {
                    // Create a new TypeSyntax for the inferred type. Be careful
                    // to keep any leading and trailing trivia from the var keyword.
                    TypeSyntax typeName = SyntaxFactory.ParseTypeName(type.ToDisplayString())
                        .WithLeadingTrivia(variableTypeName.GetLeadingTrivia())
                        .WithTrailingTrivia(variableTypeName.GetTrailingTrivia());

                    // Add an annotation to simplify the type name.
                    TypeSyntax simplifiedTypeName = typeName.WithAdditionalAnnotations(Simplifier.Annotation);

                    // Replace the type in the variable declaration.
                    variableDeclaration = variableDeclaration.WithType(simplifiedTypeName);
                }
            }
        }
        // Produce the new local declaration.
        LocalDeclarationStatementSyntax newLocal = trimmedLocal.WithModifiers(newModifiers)
                                   .WithDeclaration(variableDeclaration);

        // Add an annotation to format the new local declaration.
        LocalDeclarationStatementSyntax formattedLocal = newLocal.WithAdditionalAnnotations(Formatter.Annotation);

        // Replace the old local declaration with the new local declaration.
        SyntaxNode? oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (oldRoot is null) { return document; }

        SyntaxNode newRoot = oldRoot.ReplaceNode(localDeclaration, formattedLocal);

        // Return document with transformed tree.
        return document.WithSyntaxRoot(newRoot);
    }

#endif
}
