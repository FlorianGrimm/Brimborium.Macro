using Brimborium.Macro.CliLibrary.Service;
using Brimborium.Macro.Service;

using Mediator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Macro.CliLibrary.Command;

public sealed record ParseRequest() : IRequest<ParseResponse>;

public sealed record ParseResponse();

public sealed class ParseHandler : IRequestHandler<ParseRequest, ParseResponse> {
    private readonly StateService _StateService;

    public ParseHandler(
        StateService stateService
        ) {
        this._StateService = stateService;
        // 
    }

    public async ValueTask<ParseResponse> Handle(ParseRequest request, CancellationToken cancellationToken) {
        var solution = await this._StateService.GetSolution();
        if (solution == null) {
            return new ParseResponse();
        }

        //
        return new ParseResponse();
    }
}
