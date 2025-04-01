using Mediator;

namespace Brimborium.Macro;

public class CustomData { }
public class Transform { }


// TEST
public sealed record Ping(Guid Id) : IRequest<Pong>;

public sealed record Pong(Guid Id);

public sealed class PingHandler : IRequestHandler<Ping, Pong> {
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken) {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}
