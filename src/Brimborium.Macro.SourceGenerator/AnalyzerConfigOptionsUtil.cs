using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Brimborium.Macro.SourceGenerator;

public static class AnalyzerConfigOptionsUtil {

    public static MacroOptions MacroOptionProvider(
        AnalyzerConfigOptionsProvider optionsProvider, CancellationToken ctStop
        ) {
        var formatCode = !(optionsProvider.GlobalOptions.TryGetValue(
                "build_property.MacroFormatCode",
                out var valueMacroFormatCode)
            && valueMacroFormatCode.Equals("false", StringComparison.InvariantCultureIgnoreCase)
            );

        var validMSBuildProjectDirectory = (optionsProvider.GlobalOptions.TryGetValue(
                "build_property.MSBuildProjectDirectory",
                out var valueMSBuildProjectDirectory)
            && (valueMSBuildProjectDirectory is { Length: > 0 }));

        return new MacroOptions(
            FormatCode: formatCode,
            MSBuildProjectDirectory: ((validMSBuildProjectDirectory) ? valueMSBuildProjectDirectory : default)
            );
    }
}
public record MacroOptions(
    bool FormatCode,
    string? MSBuildProjectDirectory
    );