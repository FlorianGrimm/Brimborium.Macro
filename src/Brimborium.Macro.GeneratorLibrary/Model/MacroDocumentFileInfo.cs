namespace Brimborium.Macro.Model;

public  record class MacroDocumentFileInfo (
    string FilePath,
    DateTime? LastWriteTimeUtc,
    Microsoft.CodeAnalysis.ProjectId projectId);

public record class MacroProjectInfo(
    Microsoft.CodeAnalysis.Project Project,
    System.Collections.Immutable.ImmutableArray<Microsoft.CodeAnalysis.Document> ProjectDocuments
    );