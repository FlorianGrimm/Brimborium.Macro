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
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace Brimborium.Macro;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MacroCodeRefactoring)), Shared]
public class MacroCodeRefactoring : CodeRefactoringProvider {
    public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {
        // TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) {
            return;
        }

        // Find the node at the selection.
        var node = root.FindNode(context.Span);

        // Only offer a refactoring if the selected node is a type declaration node.
        var typeDecl = node as TypeDeclarationSyntax;
        if (typeDecl == null) {
            return;
        }

        // For any type declaration node, create a code action to reverse the identifier text.
        var action = CodeAction.Create("Reverse type name", c => ReverseTypeNameAsync(context.Document, typeDecl, c));

        // Register this code action.
        context.RegisterRefactoring(action);
    }

    private async Task<Solution> ReverseTypeNameAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken) {
        // Produce a reversed version of the type declaration's identifier token.
        var identifierToken = typeDecl.Identifier;
        var newName = new string(identifierToken.Text.ToCharArray().Reverse().ToArray());

        // Get the symbol representing the type to be renamed.
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

        // Produce a new solution that has all references to that type renamed, including the declaration.
        var originalSolution = document.Project.Solution;
        var optionSet = originalSolution.Workspace.Options;
#pragma warning disable CS0618 // Type or member is obsolete
        var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618 // Type or member is obsolete

        // Return the new solution with the now-uppercase type name.
        return newSolution;
    }
}


