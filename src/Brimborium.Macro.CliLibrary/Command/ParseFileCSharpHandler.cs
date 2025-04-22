using Brimborium.Macro.CliLibrary.Service;
using Brimborium.Macro.Model;

using Mediator;

using Microsoft.CodeAnalysis;

namespace Brimborium.Macro.CliLibrary.Command;
public record ParseFileResponse();


public sealed record ParseFileCSharpRequest(
    StateService StateService,
    MacroProjectInfo ProjectInfo,
    Compilation? Compilation,
    Document ProjectDocument
    ) : IRequest<ParseFileCSharpResponse>;

public sealed record ParseFileCSharpResponse(
    ): ParseFileResponse();

public sealed class ParseFileCSharpHandler : IRequestHandler<ParseFileCSharpRequest, ParseFileCSharpResponse> {
    public async ValueTask<ParseFileCSharpResponse> Handle(ParseFileCSharpRequest request, CancellationToken cancellationToken) {
        var stateService = request.StateService;
        var project = request.ProjectInfo.Project;
        var compilation = request.Compilation;
        var projectDocument = request.ProjectDocument;

        var syntaxTree = await projectDocument.GetSyntaxTreeAsync(cancellationToken);
        var sourceCode = await projectDocument.GetTextAsync(cancellationToken);
        var semanticModel = projectDocument.GetSemanticModelAsync(cancellationToken);
        
        return new ParseFileCSharpResponse();
    }
}
