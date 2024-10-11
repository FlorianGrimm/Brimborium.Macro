using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Testing;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

using Xunit;

/*
using VerifyCS = CSharpCodeFixVerifier<
    Brimborium.Macro.MacroAnalyzer,
    Brimborium.Macro.MacroCodeFixProvider>;
*/
using VerifyCS = Brimborium.Macro.CSharpAnalyzerVerifier<Brimborium.Macro.MacroAnalyzer>;

namespace Brimborium.Macro;

public class MacroAnalyzerTests {
    [Fact]
    public async Task MacroAnalyzer_Diagnostic() {
        const string testCode = """
            namespace Brimborium.Macro.Sample;

            internal class Program {
                static void Main(string[] args) {
                    #region Macro {{ Print "Hello, World!" 10 }}
                    System.Console.WriteLine("Hello, World!");
                    #endregion
                }
            }
            """;
        VerifyCS.Test test = new() {
            TestCode = testCode,
            ExpectedDiagnostics = {
                VerifyCS.Diagnostic(MacroAnalyzer.RunDiagnosticId).WithLocation(5, 9)
            }
        };
        await test.RunAsync();
    }
}
