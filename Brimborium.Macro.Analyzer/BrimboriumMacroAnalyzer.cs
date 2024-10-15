using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Brimborium.Macro
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BrimboriumMacroAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticIdMacroRun = "BrimboriumMacroRun";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString TitleMacroRun = new LocalizableResourceString(nameof(AnalyzersResources.AnalyzerTitle), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
        private static readonly LocalizableString MessageFormatMacroRun = new LocalizableResourceString(nameof(AnalyzersResources.AnalyzerMessageFormat), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
        private static readonly LocalizableString DescriptionMacroRun = new LocalizableResourceString(nameof(AnalyzersResources.AnalyzerDescription), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
        private const string Category = "Tooling";

        public static DiagnosticDescriptor MacroRunRule = new(
            DiagnosticIdMacroRun,
            TitleMacroRun,
            MessageFormatMacroRun,
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: DescriptionMacroRun);
        private static ImmutableArray<DiagnosticDescriptor> _SupportedDiagnostics => ImmutableArray.Create<DiagnosticDescriptor>(MacroRunRule);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _SupportedDiagnostics;
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }
        private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context) {
            if (context.IsGeneratedCode) { return; }
            // context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.Macro", out var macroValue);
            foreach (var item in MacroParser.AnalyzeSyntaxTree(context.Tree)) {
                if (item.Kind == ParserNodeOrTriviaKind.None) { continue; }

                var diagnostic = Diagnostic.Create(
                    MacroRunRule, 
                    item.Location!, 
                    item.Text ?? item.Location!.ToString());

                context.ReportDiagnostic(diagnostic);
                if (context.CancellationToken.IsCancellationRequested) { return; }
            }
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            // TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            // Find just those named type symbols with names containing lowercase letters.
            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
            {
                // For all such symbols, produce a diagnostic.
                var diagnostic = Diagnostic.Create(MacroRunRule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
