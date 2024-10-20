//
// FrameworkHelpers.cs
//
// Author:
//       Mikayla Hutchinson <m.j.hutchinson@gmail.com>
//
// Copyright (c) 2018 Microsoft Corp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace Mono.TextTemplating.CodeCompilation;

internal enum RuntimeKind {
    NetCore,
    NetFramework,
    Mono
}

internal class T4RuntimeInfo {
    private T4RuntimeInfo(RuntimeKind kind, string error) {
        this.Kind = kind;
        this.Error = error;
    }

    private T4RuntimeInfo(RuntimeKind kind, string runtimeDir, Version runtimeVersion, string refAssembliesDir, string runtimeFacadesDir, string cscPath, T4CSharpLangVersion cscMaxLangVersion, T4CSharpLangVersion runtimeLangVersion) {
        this.Kind = kind;
        this.RuntimeVersion = runtimeVersion;
        this.RuntimeDir = runtimeDir;
        this.RefAssembliesDir = refAssembliesDir;
        this.RuntimeFacadesDir = runtimeFacadesDir;
        this.CscPath = cscPath;
        this.CscMaxLangVersion = cscMaxLangVersion;
        this.RuntimeLangVersion = runtimeLangVersion;
    }

    private static T4RuntimeInfo FromError(RuntimeKind kind, string error) => new(kind, error);

    public RuntimeKind Kind { get; }
    public string Error { get; }
    public string RuntimeDir { get; }

    // may be null, as this is not a problem when the optional in-process compiler is used
    public string CscPath { get; }

    /// <summary>
    /// Maximum C# language version supported by C# compiler in <see cref="CscPath"/>.
    /// </summary>
    public T4CSharpLangVersion CscMaxLangVersion { get; }

    /// <summary>
    /// The C# version fully supported by the runtime, which is the default when targeting this runtime.
    /// </summary>
    /// <remarks>
    /// Using newer C# language versions is possible but some features may not work if they depend on runtime changes.
    /// </remarks>
    public T4CSharpLangVersion RuntimeLangVersion { get; }

    public bool IsValid => this.Error == null;
    public Version RuntimeVersion { get; }

    public string RefAssembliesDir { get; }
    public string RuntimeFacadesDir { get; }

    public static T4RuntimeInfo GetRuntime() {
        if (Type.GetType("Mono.Runtime") != null) {
            return GetMonoRuntime();
        } else if (RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase)) {
            return GetNetFrameworkRuntime();
        } else {
            return GetDotNetCoreSdk();
        }
    }

    private static T4RuntimeInfo GetMonoRuntime() {
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        var csc = Path.Combine(runtimeDir, "csc.exe");
        if (!File.Exists(csc)) {
            return FromError(RuntimeKind.Mono, "Could not find csc in host Mono installation");
        }

        return new T4RuntimeInfo(
            RuntimeKind.Mono,
            runtimeDir: runtimeDir,
            // we don't really care about the version if it's not .net core
            runtimeVersion: new Version("4.7.2"),
            refAssembliesDir: null,
            runtimeFacadesDir: Path.Combine(runtimeDir, "Facades"),
            cscPath: csc,
            //if mono has csc at all, we know it at least supports 6.0
            cscMaxLangVersion: T4CSharpLangVersion.v6_0,
            runtimeLangVersion: T4CSharpLangVersion.v5_0
        );
    }

    private static T4RuntimeInfo GetNetFrameworkRuntime() {
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        var csc = Path.Combine(runtimeDir, "csc.exe");
        if (!File.Exists(csc)) {
            return FromError(RuntimeKind.NetFramework, "Could not find csc in host .NET Framework installation");
        }
        return new T4RuntimeInfo(
            RuntimeKind.NetFramework,
            runtimeDir: runtimeDir,
            // we don't really care about the version if it's not .net core
            runtimeVersion: new Version("4.7.2"),
            refAssembliesDir: null,
            runtimeFacadesDir: runtimeDir,
            cscPath: csc,
            cscMaxLangVersion: T4CSharpLangVersion.v5_0,
            runtimeLangVersion: T4CSharpLangVersion.v5_0
        );
    }

    private static T4RuntimeInfo GetDotNetCoreSdk() {
        static bool DotnetRootIsValid(string root) => !string.IsNullOrEmpty(root) && (File.Exists(Path.Combine(root, "dotnet")) || File.Exists(Path.Combine(root, "dotnet.exe")));

        // the runtime dir is used when probing for DOTNET_ROOT
        // and as a fallback in case we cannot locate reference assemblies
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);

        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");

        if (!DotnetRootIsValid(dotnetRoot)) {
            // this will work if runtimeDir is $DOTNET_ROOT/shared/Microsoft.NETCore.App/5.0.0
            dotnetRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(runtimeDir)));

            if (!DotnetRootIsValid(dotnetRoot)) {
                return FromError(RuntimeKind.NetCore, "Could not locate .NET root directory from running app. It can be set explicitly via the `DOTNET_ROOT` environment variable.");
            }
        }

        var hostVersion = Environment.Version;

        // fallback for .NET Core < 3.1, which always returned 4.0.x
        if (hostVersion.Major == 4) {
            // this will work if runtimeDir is $DOTNET_ROOT/shared/Microsoft.NETCore.App/5.0.0
            var versionPathComponent = Path.GetFileName(runtimeDir);
            if (T4SemVersion.TryParse(versionPathComponent, out var hostSemVersion)) {
                hostVersion = new Version(hostSemVersion.Major, hostSemVersion.Minor, hostSemVersion.Patch);
            } else {
                return FromError(RuntimeKind.NetCore, "Could not determine host runtime version");
            }
        }

        // find the highest available C# compiler. we don't load it in process, so its runtime doesn't matter.
        static string MakeCscPath(string d) => Path.Combine(d, "Roslyn", "bincore", "csc.dll");
        var sdkDir = FindHighestVersionedDirectory(Path.Combine(dotnetRoot, "sdk"), d => File.Exists(MakeCscPath(d)), out var sdkVersion);

        // it's okay if cscPath is null as we may be using the in-process compiler
        string cscPath = sdkDir == null ? null : MakeCscPath(sdkDir);
        var maxCSharpVersion = T4CSharpLangVersionHelper.FromNetCoreSdkVersion(sdkVersion);

        // it's ok if this is null, we may be running on an older SDK that didn't support packs
        //in which case we fall back to resolving from the runtime dir
        var refAssembliesDir = FindHighestVersionedDirectory(
            Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref"),
            d => File.Exists(Path.Combine(d, $"net{hostVersion.Major}.{hostVersion.Minor}", "System.Runtime.dll")),
            out _
        );



        return new T4RuntimeInfo(
            RuntimeKind.NetCore,
            runtimeDir: runtimeDir,
            runtimeVersion: hostVersion,
            refAssembliesDir: refAssembliesDir,
            runtimeFacadesDir: null,
            cscPath: MakeCscPath(sdkDir),
            cscMaxLangVersion: maxCSharpVersion,
            runtimeLangVersion: T4CSharpLangVersionHelper.FromNetCoreSdkVersion(sdkVersion)
        );
    }

    private static string FindHighestVersionedDirectory(string parentFolder, Func<string, bool> validate, out T4SemVersion bestVersion) {
        string bestMatch = null;
        bestVersion = T4SemVersion.Zero;
        foreach (var dir in Directory.EnumerateDirectories(parentFolder)) {
            var name = Path.GetFileName(dir);
            if (T4SemVersion.TryParse(name, out var version) && version.Major >= 0) {
                if (version > bestVersion && (validate == null || validate(dir))) {
                    bestVersion = version;
                    bestMatch = dir;
                }
            }
        }
        return bestMatch;
    }
}
