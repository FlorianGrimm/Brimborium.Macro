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

namespace Brimborium.Macro;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MacroCodeFixProvider)), Shared]
public class MacroCodeFixProvider : CodeFixProvider {
    public sealed override ImmutableArray<string> FixableDiagnosticIds {
        get { return ImmutableArray.Create(MacroAnalyzer.DiagnosticId); }
    }

    public sealed override FixAllProvider GetFixAllProvider() {
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
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
}
