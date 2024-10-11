using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Testing;

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
        var test = new VerifyCS.Test()
        {
            TestCode = testCode,
            BatchFixedCode = testCode
        };
        await test.RunAsync();
    }
}