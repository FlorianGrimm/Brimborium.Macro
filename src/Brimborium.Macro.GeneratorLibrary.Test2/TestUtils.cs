using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System.Runtime.CompilerServices;

namespace Brimborium.Macro.GeneratorLibrary;

public static class TestUtils {

    private static string? _SolutionFolder;
    public static string GetSolutionFolder([CallerFilePath] string? callerFilePath = null) {
        if (_SolutionFolder is not null) {
            return _SolutionFolder;
        }
        var directory = new System.IO.DirectoryInfo(callerFilePath ?? System.IO.Directory.GetCurrentDirectory());
        while (directory != null && !System.IO.Directory.Exists(System.IO.Path.Combine(directory.FullName, ".git"))) {
            directory = directory.Parent;
        }
        if (directory is null) {
            throw new System.Exception("Solution folder not found.");
        }
        return _SolutionFolder = directory.FullName;
    }

    public static async Task<PreparedDocument> PrepareDocumentFromFile(string filename) {
        var fullFilename = System.IO.Path.Combine(GetSolutionFolder(), filename);
        var content = await System.IO.File.ReadAllTextAsync(fullFilename);
        return await PrepareDocumentFromSourceCode(content);
    }

    public static async Task<PreparedDocument> PrepareDocumentFromSourceCode(string sourceCode) {
        var parseOptions = new CSharpParseOptions(languageVersion: LanguageVersion.Latest, documentationMode: DocumentationMode.Parse, kind: SourceCodeKind.Regular);

        AdhocWorkspace adhocWorkspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var metadataReferences = new MetadataReference[]
               {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(File).Assembly.Location)
               };
        var projectInfo = ProjectInfo.Create(
            id: projectId,
            version: VersionStamp.Default,
            name: "TestProject",
            assemblyName: "TestProject",
            language: LanguageNames.CSharp,
            compilationOptions: new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable),
            metadataReferences: metadataReferences);
        adhocWorkspace.AddProject(projectInfo);

        var pathMacroAttribute = System.IO.Path.Combine(TestUtils.GetSolutionFolder(), @"src\Brimborium.Macro\MacroAttribute.cs");
        var sourceMacroAttribute = System.IO.File.ReadAllText(pathMacroAttribute);

        var documentMacroAttribute = adhocWorkspace.AddDocument(projectId, "MacroAttribute.cs", SourceText.From(sourceMacroAttribute))
            ?? throw new System.Exception("documentMacroAttribute is null");
        var document = adhocWorkspace.AddDocument(projectId, "TestDocument.cs", SourceText.From(sourceCode))
            ?? throw new System.Exception("document is null");

        var project = adhocWorkspace.CurrentSolution.Projects
            .FirstOrDefault(project => project.Id == projectId)
            ?? throw new System.Exception("project is null");

        var compliation = await project.GetCompilationAsync()
            ?? throw new System.Exception("compliation is null");
        var listDiagnostics = compliation.GetDiagnostics();

        var syntaxTree = compliation.SyntaxTrees.FirstOrDefault(
            syntaxTree => syntaxTree.FilePath == "TestDocument.cs")
            ?? throw new System.Exception("syntaxTree is null");

        var semanticModel = compliation.GetSemanticModel(syntaxTree)
            ?? throw new System.Exception("semanticModel is null");

        return new PreparedDocument(
            Workspace: adhocWorkspace,
            Project: project,
            Compilation: compliation,
            FilePath: "TestDocument.cs",
            SourceCode: sourceCode,
            SyntaxTree: syntaxTree,
            SemanticModel: semanticModel
        );
    }
}

public record PreparedDocument(
    AdhocWorkspace Workspace,
    Project Project,
    Compilation Compilation,
    string FilePath,
    string SourceCode,
    SyntaxTree SyntaxTree,
    SemanticModel SemanticModel) {
}
