using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

using System.Reflection;
using System.Runtime.Loader;

namespace Brimborium.Macro;

public partial class JupiterUtlity {
    class PluginLoadContext : AssemblyLoadContext {
        private readonly AssemblyDependencyResolver _Resolver;
        private readonly DependencyContext? _DependencyContext;

        public PluginLoadContext(string pluginPath) {
            this._Resolver = new AssemblyDependencyResolver(pluginPath);
            this._DependencyContext = Microsoft.Extensions.DependencyModel.DependencyContext.Load(this.GetType().Assembly);
            var listDefaultAssemblyNames = this._DependencyContext?.GetDefaultAssemblyNames().ToList();
        }

        protected override Assembly? Load(AssemblyName assemblyName) {
            if (this._Resolver.ResolveAssemblyToPath(assemblyName) is { } assemblyPath) {
                return base.LoadFromAssemblyPath(assemblyPath);
            } else {
                return null;
            }
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName) {
            if (this._Resolver.ResolveUnmanagedDllToPath(unmanagedDllName) is { } libraryPath) {
                return base.LoadUnmanagedDllFromPath(libraryPath);
            } else {
                return IntPtr.Zero;
            }
        }
    }


    internal sealed class AssemblyResolver : IDisposable {
        public static AssemblyResolver? Create(Assembly assembly, string? path = default) {
            var dependencyContext = DependencyContext.Load(assembly) ?? DependencyContext.Default;
            if (dependencyContext is null) { return null; }

            if (string.IsNullOrEmpty(path)) {
                path = System.IO.Path.GetDirectoryName(assembly.Location);
            }
            if (path is null) { return null; }

            var assemblyResolver = new CompositeCompilationAssemblyResolver(
                new ICompilationAssemblyResolver[] {
                    new AppBaseCompilationAssemblyResolver(path),
                    new ReferenceAssemblyPathResolver(path, []),
                    new PackageCompilationAssemblyResolver()
                });

            var loadContext = AssemblyLoadContext.GetLoadContext(assembly);
            if (loadContext is null) { return null; }

            return new AssemblyResolver(
                assembly,
                path,
                assemblyResolver,
                dependencyContext,
                loadContext
                );
        }

        private readonly ICompilationAssemblyResolver _AssemblyResolver;
        private readonly DependencyContext _DependencyContext;
        private readonly AssemblyLoadContext _LoadContext;


        public AssemblyResolver(
            Assembly assembly,
            string path,
            CompositeCompilationAssemblyResolver assemblyResolver,
            DependencyContext dependencyContext,
            AssemblyLoadContext loadContext) {
            this.Assembly = assembly;
            this.Path = path;
            this._AssemblyResolver = assemblyResolver;
            this._DependencyContext = dependencyContext;
            this._LoadContext = loadContext;

            if (loadContext is { }) {
                loadContext.Resolving += this.OnResolving;
            }
        }

        public string? Path { get; }
        public Assembly? Assembly { get; }

        public void Dispose() {
            if (this._LoadContext is { } loadContext) {
                loadContext.Resolving -= this.OnResolving;
            }
        }

        private Assembly? OnResolving(AssemblyLoadContext context, AssemblyName name) {
            bool NamesMatch(RuntimeLibrary runtime) {
                return string.Equals(runtime.Name, name.Name, StringComparison.OrdinalIgnoreCase);
            }

            var runtimeLibrary = this._DependencyContext.RuntimeLibraries.FirstOrDefault(NamesMatch);
            if (runtimeLibrary is null) { return null; }

            if (runtimeLibrary is { }) {
                var wrapperCompilationLibrary = new CompilationLibrary(
                    runtimeLibrary.Type,
                    runtimeLibrary.Name,
                    runtimeLibrary.Version,
                    runtimeLibrary.Hash,
                    runtimeLibrary.RuntimeAssemblyGroups.SelectMany(g => g.AssetPaths),
                    runtimeLibrary.Dependencies,
                    runtimeLibrary.Serviceable);

                var listAssembly = new List<string>();
                this._AssemblyResolver.TryResolveAssemblyPaths(wrapperCompilationLibrary, listAssembly);
                if (listAssembly.Count > 0) {
                    return this._LoadContext.LoadFromAssemblyPath(listAssembly[0]);
                }
            }

            return null;
        }
    }
}