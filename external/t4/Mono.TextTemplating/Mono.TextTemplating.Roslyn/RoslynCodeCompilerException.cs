// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Mono.TextTemplating;

[Serializable]
internal sealed class RoslynCodeCompilerException : Exception {
    public RoslynCodeCompilerException() { }
    public RoslynCodeCompilerException(string message) : base(message) { }
    public RoslynCodeCompilerException(string message, Exception inner) : base(message, inner) { }
}