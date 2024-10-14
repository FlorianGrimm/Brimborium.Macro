#pragma warning disable RS0030 // Do not use banned APIs

using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

using Xunit;

using VerifyCS = Brimborium.Macro.CSharpCodeFixVerifier<
    Brimborium.Macro.MacroAnalyzer,
    Brimborium.Macro.MacroCodeFixProvider>;

namespace Brimborium.Macro;

public class MacroCodefixTests {
    [Fact]
    public async Task MacroCodefix_Region_Test() {
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


        DiagnosticResult expected0 =
            VerifyCS.Diagnostic(MacroAnalyzer.DiagnosticIdMacroRun).WithLocation(5, 9);

        await VerifyCS.VerifyAnalyzerAsync(source, expected0);

        var fixedCode = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {
        #region Macro {{ Print "Hello, World!" 10 }}
        // Macro: {{ Print "Hello, World!" 10 }} //
        #endregion
    }
}
""";

        await new VerifyCS.Test {
            TestState =
                {
                    Sources = { source },
                    ExpectedDiagnostics = {expected0, },
                },
            FixedState = {
                    Sources = { fixedCode } ,
                    ExpectedDiagnostics = { expected0, },
                },
            CodeActionIndex = 0,
            CodeActionEquivalenceKey = MacroAnalyzer.DiagnosticIdMacroRun
        }.RunAsync();
    }

    [Fact]
    public async Task MacroCodefix_Comment_Test() {
        const string source = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {
        /* Macro {{ Print "Hello, World!" 10 }} */
        System.Console.WriteLine("Hello, World!");
        /* EndMacro*/
    }
}
""";


        DiagnosticResult expected0 =
            VerifyCS.Diagnostic(MacroAnalyzer.DiagnosticIdMacroRun).WithLocation(5, 9);

        await VerifyCS.VerifyAnalyzerAsync(source, expected0);

        var fixedCode = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {
        /* Macro {{ Print "Hello, World!" 10 }} */
        // Macro: {{ Print "Hello, World!" 10 }} //
        /* EndMacro*/
    }
}
""";

        await new VerifyCS.Test {
            TestState =
                {
                    Sources = { source },
                    ExpectedDiagnostics = {expected0},
                },
            FixedState = {
                    Sources = { fixedCode } ,
                    ExpectedDiagnostics = { expected0},
                },
            //CodeActionIndex = 0,
            CodeActionEquivalenceKey = MacroAnalyzer.DiagnosticIdMacroRun
        }.RunAsync();
    }

    [Fact]
    public async Task MacroCodefix_RegionAndComment_Test() {
        const string source = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {
        #region Macro {{ Print "Hello, World!" 10 }}
        System.Console.WriteLine("Hello, World!");
        #endregion

        /* Macro {{ Print "Hello, World!" 10 }} */
        System.Console.WriteLine("Hello, World!");
        /* EndMacro*/
    }
}
""";


        DiagnosticResult expected0 =
            VerifyCS.Diagnostic(MacroAnalyzer.DiagnosticIdMacroRun).WithLocation(5, 9);

        DiagnosticResult expected1 =
            VerifyCS.Diagnostic(MacroAnalyzer.DiagnosticIdMacroRun).WithLocation(9, 9);


        await VerifyCS.VerifyAnalyzerAsync(source, expected0, expected1);


#if false
        var fixedCode1 = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {
        #region Macro {{ Print "Hello, World!" 10 }}
        // Macro: {{ Print "Hello, World!" 10 }} //
        #endregion

        /* Macro {{ Print "Hello, World!" 10 }} */
        System.Console.WriteLine("Hello, World!");
        /* EndMacro*/
    }
}
""";
#endif

        var fixedCode2 = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {
        #region Macro {{ Print "Hello, World!" 10 }}
        // Macro: {{ Print "Hello, World!" 10 }} //
        #endregion

        /* Macro {{ Print "Hello, World!" 10 }} */
        // Macro: {{ Print "Hello, World!" 10 }} //
        /* EndMacro*/
    }
}
""";
        await new VerifyCS.Test {
            TestState =
                {
                    Sources = { source },
                    ExpectedDiagnostics = {expected0, expected1},
                },
            FixedState = {
                    Sources = { fixedCode2 } ,
                    ExpectedDiagnostics = { expected0, expected1 },
                },
            NumberOfFixAllIterations = 2,
            //CodeActionIndex = 0,
            CodeActionEquivalenceKey = MacroAnalyzer.DiagnosticIdMacroRun
        }.RunAsync();

        /*
        await new VerifyCS.Test {
            TestState =
                {
                    Sources = { fixedCode1 },
                    ExpectedDiagnostics = {expected0, expected1},
                },
            FixedState = {
                    Sources = { fixedCode2 } ,
                    ExpectedDiagnostics = { expected0, expected1 },
                },
            CodeActionIndex = 1,
            CodeActionEquivalenceKey = MacroAnalyzer.DiagnosticIdMacroRun
        }.RunAsync();
        */
    }


#if false
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    //[InlineData(100)]
    //[InlineData(1000)]
    public async Task MacroCodefix_Comment_Many_Test(int regionCount) {
        //const int regionCount = 1;

        const string sourceStart = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {

""";
        const string sourceRegion2 = """
        /* Macro {{ Print "Hello, World!" 10 }} */
        System.Console.WriteLine("Hello, World!");
        /* EndMacro*/


""";
        const string sourceEnd = """
    }
}
""";


        var fixedCodeStart = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {

""";
        var fixedCodeRegion2 = """
        /* Macro {{ Print "Hello, World!" 10 }} */
        // Macro: {{ Print "Hello, World!" 10 }} //
        /* EndMacro*/


""";
        var fixedCodeEnd = """
    }
}
""";

        StringBuilder sourceBuilder = new();
        StringBuilder fixedCodeBuilder = new();
        List<DiagnosticResult> listExpectedDiagnostics = new();

        sourceBuilder.Append(sourceStart);
        fixedCodeBuilder.Append(fixedCodeStart);
        for (int index = 0; index < regionCount; index++) {
            sourceBuilder
                .Append(sourceRegion2);
            fixedCodeBuilder
                .Append(fixedCodeRegion2);

            //listExpectedDiagnostics.Add(VerifyCS.Diagnostic(MacroAnalyzer.MacroRunRule).WithLocation(5 + index * 8, 9));
            listExpectedDiagnostics.Add(VerifyCS.Diagnostic(MacroAnalyzer.RunDiagnosticId).WithLocation(5 + index * 4, 9));
        }
        sourceBuilder.Append(sourceEnd);
        fixedCodeBuilder.Append(fixedCodeEnd);

        var source = sourceBuilder.ToString();
        var fixedCode = fixedCodeBuilder.ToString();
        var arrExpectedDiagnostics = listExpectedDiagnostics.ToArray();

        await VerifyCS.VerifyAnalyzerAsync(source, arrExpectedDiagnostics);

        var test = new VerifyCS.Test {
            TestState =
                {
                    Sources = { source },
                },
            FixedState = {
                    Sources = { fixedCode } ,
                },
            NumberOfFixAllIterations = 1 * regionCount,
            //CodeActionIndex = 0,
            CodeActionEquivalenceKey = MacroAnalyzer.DiagnosticIdMacroRun
        };
        test.TestState.ExpectedDiagnostics.AddRange(arrExpectedDiagnostics);
        test.FixedState.ExpectedDiagnostics.AddRange(arrExpectedDiagnostics);
        await test.RunAsync();
    }

#endif
#if false
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    //[InlineData(10)]
    //[InlineData(100)]
    //[InlineData(1000)]
    public async Task MacroCodefix_RegionAndComment_Many_Test(int regionCount) {

        const string sourceStart = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {

""";
        const string sourceRegion1 = """
        #region Macro {{ Print "Hello, World!" 10 }}
        System.Console.WriteLine("Hello, World!");
        #endregion


""";
        const string sourceRegion2 = """
        /* Macro {{ Print "Hello, World!" 10 }} */
        System.Console.WriteLine("Hello, World!");
        /* EndMacro*/


""";
        const string sourceEnd = """
    }
}
""";


        var fixedCodeStart = """
namespace Brimborium.Macro.Sample;

internal class Program {
    static void Main(string[] args) {

""";
        var fixedCodeRegion1 = """
        #region Macro {{ Print "Hello, World!" 10 }}
        // Macro: {{ Print "Hello, World!" 10 }} //
        #endregion


""";
        var fixedCodeRegion2 = """
        /* Macro {{ Print "Hello, World!" 10 }} */
        // Macro: {{ Print "Hello, World!" 10 }} //
        /* EndMacro*/


""";
        var fixedCodeEnd = """
    }
}
""";

        StringBuilder sourceBuilder = new();
        StringBuilder fixedCodeBuilder = new();
        List<DiagnosticResult> listExpectedDiagnostics = new();

        sourceBuilder.Append(sourceStart);
        fixedCodeBuilder.Append(fixedCodeStart);
        for (int index = 0; index < regionCount; index++) {
            sourceBuilder
                .Append(sourceRegion1).Append(sourceRegion2);
            fixedCodeBuilder
                .Append(fixedCodeRegion1).Append(fixedCodeRegion2);

            listExpectedDiagnostics.Add(VerifyCS.Diagnostic(MacroAnalyzer.MacroRunRule).WithLocation(5 + index * 8, 9));
            listExpectedDiagnostics.Add(VerifyCS.Diagnostic(MacroAnalyzer.RunDiagnosticId).WithLocation(9 + index * 8, 9));
        }
        sourceBuilder.Append(sourceEnd);
        fixedCodeBuilder.Append(fixedCodeEnd);

        var source = sourceBuilder.ToString();
        var fixedCode = fixedCodeBuilder.ToString();
        var arrExpectedDiagnostics = listExpectedDiagnostics.ToArray();

        await VerifyCS.VerifyAnalyzerAsync(source, arrExpectedDiagnostics);

        var test = new VerifyCS.Test {
            TestState =
                {
                    Sources = { source },
                },
            FixedState = {
                    Sources = { fixedCode } ,
                },
            NumberOfFixAllIterations = 2 * regionCount,
            //CodeActionIndex = 0,
            CodeActionEquivalenceKey = MacroAnalyzer.DiagnosticIdMacroRun
        };
        test.TestState.ExpectedDiagnostics.AddRange(arrExpectedDiagnostics);
        test.FixedState.ExpectedDiagnostics.AddRange(arrExpectedDiagnostics);
        await test.RunAsync();
    }
#endif
}