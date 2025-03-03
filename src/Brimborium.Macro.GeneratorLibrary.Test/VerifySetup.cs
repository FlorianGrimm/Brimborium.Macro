#pragma warning disable xUnit1013 // Public method should be marked as test
/*
 * https://github.com/VerifyTests/Verify/blob/main/docs/wiz/Windows_VisualStudio_Cli_Xunit_None.md
 * dotnet tool install -g DiffEngineTray
 * dotnet tool install -g verify.tool
 * 
 */
using Brimborium.Macro.Commands;
using Brimborium.Macro.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
                _VerifySettings.IgnoreMembers<SyntaxTree>(
                    item => item.HasCompilationUnitRoot,
                    item => item.Options);
                _VerifySettings.IgnoreMembers<Microsoft.CodeAnalysis.Location>(
                    item => item.IsInMetadata,
                    item => item.IsInSource);
                _VerifySettings.IgnoreMembers<RegionBlockInformation>(
                    item => item.Node,
                    item => item.Identifier,
                    item => item.Type
                );
                _VerifySettings.IgnoreMembers<RegionStart>(
                    //item => item.SyntaxTrivia,
                    //item => item.RegionDirective,
                    item => item.Attribute,
                    item => item.SyntaxNode
                );
                //SyntaxTrivia
                _VerifySettings.AddExtraSettings(options => {
                    options.Converters.Add(new SyntaxTriviaJsonConverter());
                    options.Converters.Add(new SyntaxNodeJsonConverter());
                    options.Converters.Add(new RegionDirectiveTriviaSyntaxJsonConverter());
                    options.Converters.Add(new EndRegionDirectiveTriviaSyntaxJsonConverter());
                });
            }
            return _VerifySettings;
        }
    }
}
internal class SyntaxTriviaJsonConverter : WriteOnlyJsonConverter<SyntaxTrivia> {
    public override void Write(VerifyJsonWriter writer, SyntaxTrivia value) {
        writer.WriteStartObject();
        writer.WriteMember<string>(value, value.Kind().ToString(), "Kind", string.Empty);
        writer.WriteMember<TextSpan?>(value, value.Span, "Span", null);
        writer.WriteMember<string>(value, value.ToString(), "ToString", string.Empty);
        writer.WriteEndObject();
    }
}

internal class SyntaxNodeJsonConverter : WriteOnlyJsonConverter<SyntaxNode> {
    public override void Write(VerifyJsonWriter writer, SyntaxNode value) {
        writer.WriteStartObject();
        writer.WriteMember(value, value.Kind().ToString(), "Kind");
        writer.WriteMember(value, value.Span, "Span");
        writer.WriteMember(value, value.ToString(), "ToString");
        writer.WriteEndObject();
    }
}

internal class RegionDirectiveTriviaSyntaxJsonConverter : WriteOnlyJsonConverter<RegionDirectiveTriviaSyntax> {
    public override void Write(VerifyJsonWriter writer, RegionDirectiveTriviaSyntax value) {
        writer.WriteStartObject();
        writer.WriteMember(value, value.Kind().ToString(), "Kind");
        writer.WriteMember(value, value.Span, "Span");
        writer.WriteMember(value, value.ToString(), "ToString");
        writer.WriteEndObject();
    }
}


internal class EndRegionDirectiveTriviaSyntaxJsonConverter : WriteOnlyJsonConverter<EndRegionDirectiveTriviaSyntax> {
    public override void Write(VerifyJsonWriter writer, EndRegionDirectiveTriviaSyntax value) {
        writer.WriteStartObject();
        writer.WriteMember(value, value.Kind().ToString(), "Kind");
        writer.WriteMember(value, value.Span, "Span");
        writer.WriteMember(value, value.ToString(), "ToString");
        writer.WriteEndObject();
    }
}
