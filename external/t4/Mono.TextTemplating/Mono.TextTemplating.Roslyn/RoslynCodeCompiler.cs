// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace Mono.TextTemplating;

internal sealed class RoslynCodeCompiler : Mono.TextTemplating.CodeCompilation.T4CodeCompiler {
    private readonly T4RuntimeInfo runtime;

    public RoslynCodeCompiler(T4RuntimeInfo runtime) {
        this.runtime = runtime;
    }

    public override Task<T4CodeCompilerResult> CompileFile(
        T4CodeCompilerArguments arguments,
        TextWriter log,
        CancellationToken token)
        => Task.FromResult(this.CompileFileInternal(arguments, token));

    private T4CodeCompilerResult CompileFileInternal(
            T4CodeCompilerArguments arguments,
            CancellationToken token) {
        CSharpCommandLineArguments args = null;
        bool hasLangVersionArg = false;
        if (arguments.AdditionalArguments != null) {
            var splitArgs = CommandLineParser.SplitCommandLineIntoArguments(arguments.AdditionalArguments, false);
            if (splitArgs.Any()) {
                args = CSharpCommandLineParser.Default.Parse(splitArgs, arguments.TempDirectory, null, null);
            }
            hasLangVersionArg = splitArgs.Any(T4CSharpLangVersionHelper.IsLangVersionArg);
        }

        var references = new List<MetadataReference>();
        foreach (var assemblyReference in T4AssemblyResolver.GetResolvedReferences(this.runtime, arguments.AssemblyReferences)) {
            references.Add(MetadataReference.CreateFromFile(assemblyReference));
        }

        var parseOptions = args?.ParseOptions ?? new CSharpParseOptions();

        // arguments.LangVersion takes precedence over any langversion arg in arguments.AdditionalArguments
        // This behavior should match that of CscCodeCompiler.CompileFile and CSharpLangVersionHelper.GetLangVersionArg
        if (arguments.LangVersion != null) {
            if (LanguageVersionFacts.TryParse(arguments.LangVersion, out var langVersion)) {
                parseOptions = parseOptions.WithLanguageVersion(langVersion);
                hasLangVersionArg = true;
            } else {
                throw new RoslynCodeCompilerException($"Unknown value '{arguments.LangVersion}' for langversion");
            }
        }

        if (!hasLangVersionArg) {
            // Default to the highest language version supported by the runtime
            // as we may be using a version of Roslyn where "latest" language
            // features depend on new APIs that aren't available on the current runtime.
            // If the runtime is an unknown version, its MaxSupportedLangVersion will default
            // to "latest" so new runtime versions will work before we explicitly add support for them.
            if (LanguageVersionFacts.TryParse(T4CSharpLangVersionHelper.ToString(this.runtime.RuntimeLangVersion), out var runtimeSupportedLangVersion)) {
                parseOptions = parseOptions.WithLanguageVersion(runtimeSupportedLangVersion);
            } else {
                // if Roslyn did not recognize the runtime's default lang version, it's newer than
                // this version of Roslyn supports, so default to the latest supported version
                parseOptions = parseOptions.WithLanguageVersion(LanguageVersion.Latest);
            }
        }

        var syntaxTrees = new List<SyntaxTree>();
        foreach (var sourceFile in arguments.SourceFiles) {
            using var stream = File.OpenRead(sourceFile);
            var sourceText = SourceText.From(stream, Encoding.UTF8, canBeEmbedded: true);
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(sourceText, parseOptions, path: sourceFile, cancellationToken: token));
        }

        var compilationOptions = (args?.CompilationOptions ?? new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithOutputKind(OutputKind.DynamicallyLinkedLibrary);

        var compilation = CSharpCompilation.Create(
            "GeneratedTextTransformation",
            syntaxTrees,
            references,
            compilationOptions
        );

        EmitOptions emitOptions = args?.EmitOptions ?? new EmitOptions();
        if (arguments.Debug) {
            var embeddedTexts = syntaxTrees.Select(st => EmbeddedText.FromSource(st.FilePath, st.GetText())).ToList();
            emitOptions = emitOptions.WithDebugInformationFormat(DebugInformationFormat.Embedded);
        }

        using var fs = File.OpenWrite(arguments.OutputPath);
        EmitResult result = compilation.Emit(fs, options: emitOptions, cancellationToken: token);

        if (result.Success) {
            return new T4CodeCompilerResult {
                Output = new List<string>(),
                Success = true,
                Errors = new List<CodeCompilerError>()
            };
        }

        var failures = result.Diagnostics.Where(x => x.IsWarningAsError || x.Severity == DiagnosticSeverity.Error);
        var errors = failures.Select(x => {
            var location = x.Location.GetMappedLineSpan();
            var startLinePosition = location.StartLinePosition;
            var endLinePosition = location.EndLinePosition;
            return new CodeCompilerError {
                Message = x.GetMessage(CultureInfo.CurrentCulture),
                Column = startLinePosition.Character,
                Line = startLinePosition.Line,
                EndLine = endLinePosition.Line,
                EndColumn = endLinePosition.Character,
                IsError = x.Severity == DiagnosticSeverity.Error,
                Origin = location.Path
            };
        }).ToList();

        return new T4CodeCompilerResult {
            Success = false,
            Output = new List<string>(),
            Errors = errors
        };
    }
}