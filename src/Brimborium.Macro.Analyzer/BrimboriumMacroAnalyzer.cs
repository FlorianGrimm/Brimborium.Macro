using System.Runtime.CompilerServices;


using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[assembly:InternalsVisibleTo("Brimborium.Macro.Analyzer.Test")]

namespace Brimborium.Macro {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BrimboriumMacroAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "BrimboriumMacro";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        internal static readonly DiagnosticDescriptor DiagnosticIdMacroRun = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        private static ImmutableArray<DiagnosticDescriptor>? _SupportedDiagnostics;
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return _SupportedDiagnostics??ImmutableArray.Create(DiagnosticIdMacroRun); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            //context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
            
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Namespace);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context) {
            if (context.IsGeneratedCode) { 
                return; 
            }

            if (context.Node is not AttributeSyntax attributeSyntax) { 
                return;
            }

            var attrTypeInfo = context.SemanticModel.GetTypeInfo(attributeSyntax);

            var attrTypeInfo2 = context.SemanticModel.GetTypeInfo(attributeSyntax.Name);
            
            //var attrTypeInfo2 = context.SemanticModel.GetTypeInfo(((GenericNameSyntax)attributeSyntax.Name);

            // Expected: type symbol of the type used as type argument
            // Actual: type parameter symbol of 'TState'
            //var stateType = ((INamedTypeSymbol)attrTypeInfo.Type!).TypeArguments[0];

            //var stateType = context.SemanticModel.GetTypeInfo(((GenericNameSyntax)attributeSyntax.Name).TypeArgumentList.Arguments[0]).Type!;

        }

        private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context) {
            if (context.IsGeneratedCode) { return; }
            /*
            foreach (var item in MacroParser.AnalyzeSyntaxTree(context.Tree)) {
                if (item.Kind == ParserNodeOrTriviaKind.None) { continue; }

                var diagnostic = Diagnostic.Create(
                    MacroRunRule,
                    item.Location!,
                    item.Text ?? item.Location!.ToString());

                context.ReportDiagnostic(diagnostic);
                if (context.CancellationToken.IsCancellationRequested) { return; }
            }
            */
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                var diagnostic = Diagnostic.Create(DiagnosticIdMacroRun, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
