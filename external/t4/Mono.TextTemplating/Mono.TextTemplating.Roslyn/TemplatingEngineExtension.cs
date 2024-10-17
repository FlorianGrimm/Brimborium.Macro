// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#if false
namespace Mono.TextTemplating;

// TODO: WEICHEI: Remove this since it's the default compiler now
public static class RoslynTemplatingEngineExtensions {
    public static void UseInProcessCompiler(this TemplatingEngine engine) {
        engine.SetCompilerFunc((RuntimeInfo r) => new RoslynCodeCompiler(r));
    }

    public static void UseInProcessCompiler(this TemplateGenerator generator) {
        generator.Engine.UseInProcessCompiler();
    }
}
#endif