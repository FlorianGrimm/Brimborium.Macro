// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Mono.TextTemplating;

[Serializable]
internal class T4TemplatingEngineException
    : Exception {
    public T4TemplatingEngineException() { }
    public T4TemplatingEngineException(string message) : base(message) { }
    public T4TemplatingEngineException(string message, Exception inner) : base(message, inner) { }
    protected T4TemplatingEngineException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
