// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Mono.TextTemplating;

internal class T4CurrentDomainAssemblyResolver : IDisposable {
    private readonly Func<string, string> resolveAssemblyReference;
    private readonly string[] assemblyFiles;
    private bool disposed;

    public T4CurrentDomainAssemblyResolver(string[] assemblyFiles, Func<string, string> resolveAssemblyReference) {
        this.resolveAssemblyReference = resolveAssemblyReference;
        this.assemblyFiles = assemblyFiles;

        AppDomain.CurrentDomain.AssemblyResolve += this.ResolveReferencedAssemblies;
    }

    private Assembly ResolveReferencedAssemblies(object sender, ResolveEventArgs args) {
        var asmName = new AssemblyName(args.Name);

        // The list of assembly files referenced by the template may contain reference assemblies,
        // which will fail to load. Letting the host attempt to resolve the assembly first
        // gives it an opportunity to resolve runtime assemblies.
        var path = this.resolveAssemblyReference(asmName.Name + ".dll");
        if (File.Exists(path)) {
            return Assembly.LoadFrom(path);
        }

        foreach (var asmFile in this.assemblyFiles) {
            if (asmName.Name == Path.GetFileNameWithoutExtension(asmFile)) {
                return Assembly.LoadFrom(asmFile);
            }
        }

        return null;
    }

    public void Dispose() {
        if (!this.disposed) {
            AppDomain.CurrentDomain.AssemblyResolve -= this.ResolveReferencedAssemblies;
            this.disposed = true;
        }
    }
}
