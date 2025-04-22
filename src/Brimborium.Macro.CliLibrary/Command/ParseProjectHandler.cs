using Brimborium.Macro.CliLibrary.Service;
using Brimborium.Macro.Model;

using Mediator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;

using System.Reflection.Metadata;

namespace Brimborium.Macro.CliLibrary.Command;

public sealed record ParseProjectRequest(
    StateService StateService,
    MacroProjectInfo ProjectInfo
    ) : IRequest<ParseProjectResponse>;

public sealed record ParseProjectResponse(
    List<ParseFileResponse> ListParseFileResponse
    );

public sealed class ParseProjectHandler : IRequestHandler<ParseProjectRequest, ParseProjectResponse> {
    private readonly IMediator _Mediator;

    public ParseProjectHandler(
        IMediator mediator
        ) {
        this._Mediator = mediator;
    }

    public async ValueTask<ParseProjectResponse> Handle(ParseProjectRequest request, CancellationToken cancellationToken) {
        var stateService = request.StateService;
        var project = request.ProjectInfo.Project;
        var compilation = await project.GetCompilationAsync(cancellationToken);
        List<ParseFileResponse> listParseFileResponse = new(request.ProjectInfo.ProjectDocuments.Length);

        foreach (var projectDocument in request.ProjectInfo.ProjectDocuments) {
            var fileExtension = System.IO.Path.GetExtension(projectDocument.FilePath);
            if (fileExtension == ".cs") {
                var parseFileCSharpRequest = new ParseFileCSharpRequest(stateService, request.ProjectInfo, compilation, projectDocument);
                var parseFileCSharpResponse = await this._Mediator.Send(parseFileCSharpRequest, cancellationToken);
                listParseFileResponse.Add(parseFileCSharpResponse);
            }
        }

        return new ParseProjectResponse(listParseFileResponse);
    }
}
