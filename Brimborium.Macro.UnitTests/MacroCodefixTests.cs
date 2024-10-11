#pragma warning disable RS0030 // Do not use banned APIs

using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

using Xunit;

using VerifyCS = Brimborium.Macro.CSharpCodeFixVerifier<
    Brimborium.Macro.MacroAnalyzer,
    Brimborium.Macro.MacroCodeFixProvider>;

namespace Brimborium.Macro;

public class MacroCodefixTests
{
    [Fact]
    public async Task MacroCodefixTest()
    {
        const string source = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {
        #region Macro {{ Print "Hello, World!" 10 }}
        System.Console.WriteLine("Hello, World!");
        #endregion
    }
}
""";
 

            DiagnosticResult expected = VerifyCS
                .Diagnostic(MacroAnalyzer.MacroRunRule)
                .WithLocation(5, 9);
                //VerifyCS.Diagnostic(MacroAnalyzer.RunDiagnosticId).WithLocation(5, 9)
            await VerifyCS.VerifyAnalyzerAsync(source, expected);

            var fixedCode = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {
        #region Macro {{ Print "Hello, World!" 10 }}
        // Macro: {{ Print "Hello, World!" 10 }}
        #endregion
    }
}
""";

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source },
                    ExpectedDiagnostics = { expected },
                },
                FixedState = { 
                    Sources = { fixedCode } ,
                    ExpectedDiagnostics = { expected },
                },
                CodeActionIndex = 0,
                CodeActionEquivalenceKey = nameof(CodeFixesResources.CodeFixTitle) //string.Format(CodeAnalysisDiagnosticsResources.ApplyDiagnosticAnalyzerAttribute_1, LanguageNames.CSharp),
            }.RunAsync();
            
    }
}