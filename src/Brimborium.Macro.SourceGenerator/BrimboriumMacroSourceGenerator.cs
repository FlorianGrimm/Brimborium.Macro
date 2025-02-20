using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System;
using System.Collections.Immutable;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;

namespace Brimborium.Macro.SourceGenerator;
//https://github.com/dartk/ScribanGen/blob/main/ScribanGen/ScribanRenderer.cs

[Generator]
public class BrimboriumMacroSourceGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        IncrementalValueProvider<MacroOptions> macroOptionProvider = context.AnalyzerConfigOptionsProvider.Select(
                AnalyzerConfigOptionsUtil.MacroOptionProvider
            ).WithTrackingName("MacroOption");

        
        // https://github.com/dartk/ScribanGen/blob/main/ScribanGen/MultilineCommentGenerator.cs

        var macroAttributeSyntaxProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "Brimborium.Macro.MacroSourceAttribute",
                predicate: PredicateSyntaxNode,
                transform: static (GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken) => context);

        IncrementalValueProvider<System.Collections.Immutable.ImmutableArray<ScribanSourceText>> listScribanCSFileProvider = context.AdditionalTextsProvider
            .Where((additionalText) => additionalText.Path.EndsWith(".scriban-cs", StringComparison.OrdinalIgnoreCase))
            .Select((additionalText, token) => {
                var sourceText = additionalText.GetText(token);
                return new ScribanSourceText(additionalText.Path, null, sourceText);
            })
            .Where((scribanSourceText) => scribanSourceText.SourceText is not null)
            .Collect()
            .WithTrackingName("scriban-cs files");

        IncrementalValueProvider<ImmutableArray<ScribanSourceText>> listScribanCSFileRelativeProvider = listScribanCSFileProvider.Combine(macroOptionProvider)
            .Select((args, ctStop) => {
                var (scribanFiles, macroOptions) = args;
                if (macroOptions.MSBuildProjectDirectory is { Length: > 0 } projectDirectory) {
                    return scribanFiles.Select((item) => {
                        return item.GetRelative(projectDirectory);
                    }).ToImmutableArray();
                } else {
                    return scribanFiles;
                }
            })
            ;

        //context.RegisterHostOutput
        // macroAttributeSyntaxProvider.Where
        IncrementalValueProvider<(System.Collections.Immutable.ImmutableArray<ScribanSourceText> Left, MacroOptions Right)> x = listScribanCSFileProvider.Combine(macroOptionProvider);

        context.RegisterSourceOutput<(GeneratorAttributeSyntaxContext Left, (System.Collections.Immutable.ImmutableArray<ScribanSourceText> Left, MacroOptions Right) Right)>(
            source: macroAttributeSyntaxProvider.Combine(x),
            action: (sourceProductionContext, args) => {
                var generatorAttributeSyntaxContext = args.Left;
                var listScribanSourceText = args.Right.Left;
                var formatCode = args.Right.Right;
                var (targetSymbol, y) = args;
                var type = (INamedTypeSymbol)targetSymbol.TargetSymbol;

                var syntaxTree = generatorAttributeSyntaxContext.TargetNode.SyntaxTree;
                var sourceCodeText = syntaxTree.GetText().ToString();
                int x = 20;
                var l = syntaxTree.GetLocation(new TextSpan(20, 0));
                _ = l.GetLineSpan().Span.Start.Line;
                _ = l.GetLineSpan().Span.Start.Character;
                var node = syntaxTree.GetRoot().FindNode(span: new TextSpan(20, 0), findInsideTrivia: true, getInnermostNodeForTie: false);
                //generatorAttributeSyntaxContext.TargetNode.SyntaxTree.WithChangedText()

                var filename = type.Name.Replace('.', '-');
                sourceProductionContext.AddSource($"Brimborium.Macro-{filename}.cs", $"// This is a generated file {type.Name}");
            });
    }

    public static bool PredicateSyntaxNode(SyntaxNode syntaxNode, CancellationToken cancellationToken) {
        if (syntaxNode is TypeDeclarationSyntax typeDeclaration) {
            return IsPartial(typeDeclaration);
        }
        if (syntaxNode.IsKind(SyntaxKind.PropertyDeclaration)) {
            return true;
        }
        return false;
    }

    public static bool IsPartial(TypeDeclarationSyntax typeDeclaration) {
        foreach (var modifier in typeDeclaration.Modifiers) {
            if (modifier.IsKind(SyntaxKind.PartialKeyword)) {
                return true;
            }
        }
        return false;
    }
}

public record ScribanSourceText(string Path, string? RelativePath, SourceText? SourceText) {
    public ScribanSourceText GetRelative(string rootPath) {
        if (this.Path.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase)
                && this.Path.Length > rootPath.Length
                && this.Path[rootPath.Length] == System.IO.Path.DirectorySeparatorChar
            ) {
            var relativePath = this.Path.Substring(rootPath.Length + 1);
            return new ScribanSourceText(this.Path, relativePath, this.SourceText);
        }
        return this;
    }
}

public record class MacroSourceInfo(ISymbol TargetSymbol);