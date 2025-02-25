#pragma warning disable xUnit1013 // Public method should be marked as test
/*
 * https://github.com/VerifyTests/Verify/blob/main/docs/wiz/Windows_VisualStudio_Cli_Xunit_None.md
 * dotnet tool install -g DiffEngineTray
 * dotnet tool install -g verify.tool
 * 
 */
using Microsoft.CodeAnalysis;

using System.Runtime.CompilerServices;

namespace Brimborium.Macro.GeneratorLibrary.Test;

public static class StaticSettingsUsage {
    [ModuleInitializer]
    public static void Initialize() {
        VerifyDiffPlex.Initialize();

        var solutionFolder = TestUtils.GetSolutionFolder();
        VerifierSettings.AddScrubber(_ => _.Replace(solutionFolder, "solutionFolder"));
    }
}

public class VerifyChecksTests {
    [Fact]
    public Task Run() =>
        VerifyChecks.Run();
}

public static class Defaults {
    private static global::VerifyTests.VerifySettings? _VerifySettings;
    public static VerifySettings VerifySettings {
        get {
            if (_VerifySettings is null) {
                _VerifySettings = new();
                _VerifySettings.ScrubMembersWithType<SyntaxTrivia>();
                _VerifySettings.ScrubMembersWithType<Microsoft.CodeAnalysis.CSharp.Syntax.RegionDirectiveTriviaSyntax>();
                _VerifySettings.ScrubMembersWithType<Microsoft.CodeAnalysis.CSharp.Syntax.EndRegionDirectiveTriviaSyntax>();
                _VerifySettings.ScrubMembers<SyntaxTree>(
                    item=>item.HasCompilationUnitRoot,
                    item=>item.Options);
                _VerifySettings.ScrubMembers<Microsoft.CodeAnalysis.Location>(
                    item => item.IsInMetadata,
                    item => item.IsInSource);
                


            }
            return _VerifySettings;
        }
    }
}