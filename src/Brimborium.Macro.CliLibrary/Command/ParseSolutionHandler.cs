using Brimborium.Macro.CliLibrary.Service;
using Brimborium.Macro.Model;
using Brimborium.Macro.Service;

using Mediator;

using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.CliLibrary.Command;

public sealed record ParseSolutionRequest(
    StateService StateService
    ) : IRequest<ParseSolutionResponse>;

public sealed record ParseSolutionResponse(
    List<ParseFileResponse> ListParseFileResponse
    );

public sealed class ParseSolutionHandler : IRequestHandler<ParseSolutionRequest, ParseSolutionResponse> {
    private readonly IServiceProvider _ServiceProvider;
    private readonly IMediator _Mediator;
    private readonly ILogger<ParseSolutionHandler> _Logger;

    public ParseSolutionHandler(
        IServiceProvider serviceProvider,
        IMediator mediator,
        ILogger<ParseSolutionHandler> logger
        ) {
        this._ServiceProvider = serviceProvider;
        this._Mediator = mediator;
        this._Logger = logger;
    }

    public async ValueTask<ParseSolutionResponse> Handle(ParseSolutionRequest request, CancellationToken cancellationToken) {
        var stateService = request.StateService;
        var solution = await stateService.GetSolution();
        if (solution == null) {
            return new ParseSolutionResponse([]);
        }

        var listDocumentFileInfo = new List<MacroDocumentFileInfo>();
        var dictProjectInfo = new Dictionary<ProjectId, MacroProjectInfo>();
        var listProjectInfo = new List<MacroProjectInfo>();

        var dictProjectById = new Dictionary<ProjectId, Project>();
        foreach (var project in solution.Projects) {
            dictProjectById[project.Id] = project;
        }

        var projectDependencyGraph = solution.GetProjectDependencyGraph();
        var listProjectId = projectDependencyGraph.GetTopologicallySortedProjects(cancellationToken);
        foreach (var projectId in listProjectId) {
            var project = dictProjectById[projectId];
            var listProjectDocument = new List<Microsoft.CodeAnalysis.Document>();

            foreach (var document in project.Documents) {
                var documentFileInfo = DocumentUtility.GetDocumentFileInfo(document, project.Id);
                if (documentFileInfo is null) {
                    continue;
                }
                listDocumentFileInfo.Add(documentFileInfo);
                listProjectDocument.Add(document);
            }
            var macroProjectInfo = new MacroProjectInfo(
                project,
                listProjectDocument.ToImmutableArray());
            listProjectInfo.Add(macroProjectInfo);
            dictProjectInfo[project.Id] = macroProjectInfo;
        }

        var imdictProjectInfo = dictProjectInfo.ToImmutableDictionary();
        var imlistDocumentFileInfo = listDocumentFileInfo.ToImmutableArray();
        stateService.SetProjectInfo(imdictProjectInfo, imlistDocumentFileInfo);
        //

        var result = new List<ParseFileResponse>();
        foreach (var projectInfo in listProjectInfo) {
            var response=await this._Mediator.Send(new ParseProjectRequest(stateService, projectInfo));
            result.AddRange(response.ListParseFileResponse);
        }

        //
        return new ParseSolutionResponse(result);
    }
}
