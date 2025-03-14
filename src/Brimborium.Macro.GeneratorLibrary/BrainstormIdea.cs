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
            var cachedProject = this._CachedProject.ToBuilder();
            foreach (var project in this._Solution?.Projects ?? Enumerable.Empty<Project>()) {
                var projectFilePath = project.FilePath;
                System.Console.WriteLine($"Project: {project.FilePath}");
                foreach (var document in project.Documents) {
                    this.GetDocumentFileInfo(document);


                }
                var compilation = await project.GetCompilationAsync(ctStop);
                cachedProject[project.Id] = new ProjectState(project, compilation);
            }
            this._CachedProject = cachedProject.ToImmutable();
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
            var listMacroLocation = await this.ScanForMacros(ctStop);
            foreach (var macroLocation in listMacroLocation) {
                await UpdateMacro(macroLocation, ctStop);
            }
        }

        public async Task<List<MacroLocation>> ScanForMacros(CancellationToken ctStop) {
            var result = new List<MacroLocation>();
            await Task.CompletedTask;
            return result;
        }


        public async Task UpdateMacro(MacroLocation macroLocation, CancellationToken ctStop) {
            await Task.CompletedTask;
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
    Project project,
    Compilation? compilation);

public record MacroLocation(
    DocumentFileInfo DocumentFileInfo
    );

public record DocumentFileInfo(
    string FullName, 
    DateTime? LastWriteTimeUtc);