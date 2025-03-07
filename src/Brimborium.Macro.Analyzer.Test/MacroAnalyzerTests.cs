﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

//using Microsoft.VisualStudio.TestTools.UnitTesting;

using Xunit;

/*
using VerifyCS = CSharpCodeFixVerifier<
    Brimborium.Macro.MacroAnalyzer,
    Brimborium.Macro.MacroCodeFixProvider>;
*/
using VerifyCS = Brimborium.Macro.CSharpAnalyzerVerifier<Brimborium.Macro.BrimboriumMacroAnalyzer>;

namespace Brimborium.Macro;

public class MacroAnalyzerTests {
    [Fact]
    public async Task MacroAnalyzer_Region_Diagnostic() {
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
        await (new VerifyCS.Test() {
            TestCode = source,
            ExpectedDiagnostics = {
                VerifyCS.Diagnostic(BrimboriumMacroAnalyzer.DiagnosticIdMacroRun)
                .WithLocation(5, 9)
                .WithArguments("""{{ Print "Hello, World!" 10 }}"""),
            }
        }).RunAsync();
    }

    [Fact]
    public async Task MacroAnalyzer_Comment_Diagnostic() {
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
        await (new VerifyCS.Test() {
            TestCode = source,
            ExpectedDiagnostics = {
                VerifyCS.Diagnostic(BrimboriumMacroAnalyzer.DiagnosticIdMacroRun)
                .WithLocation(5, 9)
                .WithArguments("""{{ Print "Hello, World!" 10 }}"""),
            }
        }).RunAsync();
    }

    [Fact]
    public async Task MacroAnalyzer_RegionAndComment_Diagnostic() {
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
        await (new VerifyCS.Test() {
            TestCode = source,
            ExpectedDiagnostics = {
                VerifyCS.Diagnostic(BrimboriumMacroAnalyzer.DiagnosticIdMacroRun)
                    .WithLocation(5, 9)
                    .WithArguments("""{{ Print "Hello, World!" 10 }}"""),
                VerifyCS.Diagnostic(BrimboriumMacroAnalyzer.DiagnosticIdMacroRun)
                    .WithLocation(9, 9)
                    .WithArguments("""{{ Print "Hello, World!" 10 }}"""),
            }
        }).RunAsync();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(30)]
    [InlineData(40)]
    [InlineData(50)]
    [InlineData(60)]
    [InlineData(70)]
    [InlineData(80)]
    [InlineData(90)]
    //[InlineData(100)]
    //[InlineData(200)]
    //[InlineData(300)]
    //[InlineData(400)]
    //[InlineData(500)]
#warning StackOverflowException
    // at Microsoft.CodeAnalysis.Testing.WeightedMatch.MatchRecursive[[Microsoft.CodeAnalysis.Testing.DiagnosticResult, 
    //[InlineData(1000)]
    public async Task MacroAnalyzer_RegionAndComment_Many_Diagnostic(int regionCount) {

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


        StringBuilder sourceBuilder = new();
        StringBuilder fixedCodeBuilder = new();
        List<DiagnosticResult> listExpectedDiagnostics = new();

        sourceBuilder.Append(sourceStart);
        for (int index = 0; index < regionCount; index++) {
            sourceBuilder
                .Append(sourceRegion1).Append(sourceRegion2);
            listExpectedDiagnostics.Add(
                VerifyCS.Diagnostic(BrimboriumMacroAnalyzer.DiagnosticIdMacroRun)
                    .WithLocation(5 + index * 8, 9)
                    .WithArguments("""{{ Print "Hello, World!" 10 }}"""));
            listExpectedDiagnostics.Add(
                VerifyCS.Diagnostic(BrimboriumMacroAnalyzer.DiagnosticIdMacroRun)
                    .WithLocation(9 + index * 8, 9)
                    .WithArguments("""{{ Print "Hello, World!" 10 }}"""));
        }
        sourceBuilder.Append(sourceEnd);

        var source = sourceBuilder.ToString();
        var fixedCode = fixedCodeBuilder.ToString();
        var arrExpectedDiagnostics = listExpectedDiagnostics.ToArray();

        await VerifyCS.VerifyAnalyzerAsync(source, arrExpectedDiagnostics);
    }
}
