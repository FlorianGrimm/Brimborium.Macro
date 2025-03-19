using Brimborium.Macro.Model;

using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro {
    public class BrainstormIdea {
        private readonly SolutionService _SolutionService;
        private Solution? _Solution;
        private ImmutableDictionary<ProjectId, ProjectState> _CachedProject = ImmutableDictionary<ProjectId, ProjectState>.Empty;
        private ImmutableDictionary<string, ProjectId[]> _CachedDocumentProject = ImmutableDictionary<string, ProjectId[]>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);
        private ImmutableDictionary<string, DocumentFileInfo> _CachedDocument = ImmutableDictionary<string, DocumentFileInfo>.Empty.WithComparers(StringComparer.OrdinalIgnoreCase);

        public BrainstormIdea(SolutionService solutionService) {
            this._SolutionService = solutionService;
        }

        public async Task OpenSolutionAsync(string solutionFilePath, CancellationToken ctStop) {
            var solution = await _SolutionService.OpenSolutionAsync(solutionFilePath, null, ctStop);
            if (!(solution is { })) {
                throw new Exception("Solution is not loaded.");
            }
            this._Solution = solution;
        }

        public async Task ListenForChanges(CancellationToken ctStop) {
            await Task.CompletedTask;
        }
        public async Task GetAllDocumentFileInfo(CancellationToken ctStop) {
            var cachedProject = this._CachedProject.ToBuilder();
            var cachedDocument = this._CachedDocument.ToBuilder();
            var hsEmpty = ImmutableHashSet<string>.Empty;
            var hsPreviousAllFullNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var hsNextAllFullNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var project in this._Solution?.Projects ?? Enumerable.Empty<Project>()) {
                ImmutableHashSet<string> hsPreviousFullNames;
                {
                    if (cachedProject.TryGetValue(project.Id, out var previousProjectState)) {
                        hsPreviousFullNames = previousProjectState.HsMarcoFileName;
                        hsPreviousAllFullNames.UnionWith(previousProjectState.HsMarcoFileName);
                    } else {
                        hsPreviousFullNames = hsEmpty;
                    }
                }
                {
                    var projectFilePath = project.FilePath;
                    var compilation = await project.GetCompilationAsync(ctStop);
                    System.Console.WriteLine($"Project: {project.FilePath}");
                    var hsFullNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var document in project.Documents) {
                        var documentFileInfo = this.GetDocumentFileInfo(document);
                        if (documentFileInfo is null) { continue; }
                        var fullName = documentFileInfo.FullName;
                        hsFullNames.Add(fullName);
                        cachedDocument[fullName] = documentFileInfo;
                    }
                    cachedProject[project.Id] = new ProjectState(
                        Project: project,
                        Compilation: compilation,
                        HsMarcoFileName: hsFullNames.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase));
                    hsNextAllFullNames.UnionWith(hsFullNames);
                }
            }
            this._CachedProject = cachedProject.ToImmutable();
            this._CachedDocument = cachedDocument.ToImmutable();
            await Task.CompletedTask;
        }

        public DocumentFileInfo? GetDocumentFileInfo(Microsoft.CodeAnalysis.Document document) {
            if (!document.SupportsSemanticModel) { return null; }
            if (DocumentUtility.GetIsGenerated(document)) { return null; }
            var filePath = document.FilePath;
            if (string.IsNullOrEmpty(filePath)) { return null; }
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
            DateTime? lastWriteTimeUtc = (fileInfo.Exists)
                ? fileInfo.LastWriteTimeUtc
                : default;
            return new DocumentFileInfo(fileInfo.FullName, lastWriteTimeUtc);
        }

        public async Task ReloadIfNeeded(CancellationToken ctStop) {
            // TODO: handle changed and reload
            await Task.CompletedTask;
        }

        public async Task UpdateAllMacros(CancellationToken ctStop) {
            await this.ReloadIfNeeded(ctStop);
            var listMacroLocation = await this.ScanForAllMacros(ctStop);
            var grpFileMacroLocation = listMacroLocation.GroupBy(macroLocation => macroLocation.DocumentFileInfo.FullName);
            foreach (var fileMacroLocation in grpFileMacroLocation) {
                await UpdateAllFileMacros(fileMacroLocation.First().DocumentFileInfo, fileMacroLocation, ctStop);
            }
        }

        public async Task UpdateAllFileMacros(DocumentFileInfo documentFileInfo, IEnumerable<MacroLocation> listFileMacroLocation, CancellationToken ctStop) {
            foreach (var macroLocation in listFileMacroLocation) {
                await UpdateMacro(macroLocation, ctStop);
            }
        }

        public async Task<List<MacroLocation>> ScanForAllMacros(CancellationToken ctStop) {
            var result = new List<MacroLocation>();
            await Task.CompletedTask;
            return result;
        }


        public async Task UpdateMacro(MacroLocation macroLocation, CancellationToken ctStop) {
            var differenceDocumentRegionTree = await CalculateMacro(macroLocation, ctStop);
            await ApplyMacro(differenceDocumentRegionTree, ctStop);
        }

        public async Task<DifferenceDocumentRegionTree> CalculateMacro(MacroLocation macroLocation, CancellationToken ctStop) {
            await Task.CompletedTask;
            return null!;
        }

        public async Task ApplyMacro(DifferenceDocumentRegionTree differenceDocumentRegionTree, CancellationToken ctStop) {
            await Task.CompletedTask;
        }

    }
}

public record ProjectState(
    Project Project,
    Compilation? Compilation,
    ImmutableHashSet<string> HsMarcoFileName);

public record MacroLocation(
    DocumentFileInfo DocumentFileInfo
    );

public record DocumentFileInfo(
    string FullName,
    DateTime? LastWriteTimeUtc);