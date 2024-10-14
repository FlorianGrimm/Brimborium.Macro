#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;

namespace Brimborium.Macro;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MacroAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticIdMacroRun = "MacroRun";

    // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
    // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzersResources.AnalyzerTitle), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzersResources.AnalyzerMessageFormat), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzersResources.AnalyzerDescription), AnalyzersResources.ResourceManager, typeof(AnalyzersResources));
    private const string Category = "Macro";

    public static DiagnosticDescriptor MacroRunRule = new(
        DiagnosticIdMacroRun,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);
    private static ImmutableArray<DiagnosticDescriptor> _SupportedDiagnostics => ImmutableArray.Create<DiagnosticDescriptor>(MacroRunRule);
 
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _SupportedDiagnostics;


    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        // TODO: WEICHEI: context.RegisterSyntaxNodeAction(AnalyzeRegionDirective, SyntaxKind.RegionDirectiveTrivia);
    }


    private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        if (context.IsGeneratedCode) { return; }
        // context.Options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.Macro", out var macroValue);
        foreach (var item in MacroParser.AnalyzeSyntaxTree(context.Tree)){
            context.ReportDiagnostic(Diagnostic.Create(MacroRunRule, item.Location));
            if (context.CancellationToken.IsCancellationRequested) { return; }
        }
    }

#if WEICHEI
    private void AnalyzeRegionDirective(SyntaxNodeAnalysisContext context)
    {
        var regionDirective = (RegionDirectiveTriviaSyntax)context.Node;
        if (regionDirective.EndOfDirectiveToken.IsMissing) { return; }
        var regionName = regionDirective.EndOfDirectiveToken.ToFullString();
        if (regionName.StartsWith("macro ", StringComparison.OrdinalIgnoreCase))
        {
            context.ReportDiagnostic(Diagnostic.Create(MacroRunRule, regionDirective.GetLocation()));
        }
    }
#endif


// TODO: WEICHEI old sample
#if WEICHEI
    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

        // make sure the declaration isn't already const:
        if (localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return;
        }

        TypeSyntax variableTypeName = localDeclaration.Declaration.Type;
        ITypeSymbol? variableType = context.SemanticModel.GetTypeInfo(variableTypeName, context.CancellationToken).ConvertedType;
        if (variableType is null)
        {
            return;
        }

        // Ensure that all variables in the local declaration have initializers that
        // are assigned with constant values.
        foreach (VariableDeclaratorSyntax variable in localDeclaration.Declaration.Variables)
        {
            EqualsValueClauseSyntax? initializer = variable.Initializer;
            if (initializer == null)
            {
                return;
            }

            Optional<object?> constantValue = context.SemanticModel.GetConstantValue(initializer.Value, context.CancellationToken);
            if (!constantValue.HasValue)
            {
                return;
            }

            // Ensure that the initializer value can be converted to the type of the
            // local declaration without a user-defined conversion.
            Conversion conversion = context.SemanticModel.ClassifyConversion(initializer.Value, variableType);
            if (!conversion.Exists || conversion.IsUserDefined)
            {
                return;
            }

            // Special cases:
            //  * If the constant value is a string, the type of the local declaration
            //    must be System.String.
            //  * If the constant value is null, the type of the local declaration must
            //    be a reference type.
            if (constantValue.Value is string)
            {
                if (variableType.SpecialType != SpecialType.System_String)
                {
                    return;
                }
            }
            else if (variableType.IsReferenceType && constantValue.Value != null)
            {
                return;
            }
        }

        // Perform data flow analysis on the local declaration.
        DataFlowAnalysis? dataFlowAnalysis = context.SemanticModel.AnalyzeDataFlow(localDeclaration);
        if (dataFlowAnalysis is null)
        {
            return;
        }

        foreach (VariableDeclaratorSyntax variable in localDeclaration.Declaration.Variables)
        {
            // Retrieve the local symbol for each variable in the local declaration
            // and ensure that it is not written outside of the data flow analysis region.
            ISymbol? variableSymbol = context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken);
            if (variableSymbol is null)
            {
                return;
            }
            if (dataFlowAnalysis.WrittenOutside.Contains(variableSymbol))
            {
                return;
            }
        }

        context.ReportDiagnostic(Diagnostic.Create(MacroRunRule, context.Node.GetLocation()));
    }
#endif
}
