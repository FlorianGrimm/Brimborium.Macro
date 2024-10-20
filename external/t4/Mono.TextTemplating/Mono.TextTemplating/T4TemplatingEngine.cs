//
// Engine.cs
//
// Author:
//       Mikayla Hutchinson <m.j.hutchinson@gmail.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
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

namespace Mono.TextTemplating;

public partial class T4TemplatingEngine :
#if FEATURE_APPDOMAINS
    MarshalByRefObject,
#endif
#pragma warning disable 618
    ITextTemplatingEngine
#pragma warning restore 618
{
    private Func<T4RuntimeInfo, CodeCompilation.T4CodeCompiler> createCompilerFunc;
    private CodeCompilation.T4CodeCompiler cachedCompiler;

    internal void SetCompilerFunc(Func<T4RuntimeInfo, CodeCompilation.T4CodeCompiler> createCompiler) {
        this.cachedCompiler = null;
        this.createCompilerFunc = createCompiler;
    }

    private CodeCompilation.T4CodeCompiler GetOrCreateCompiler() {
        if (this.cachedCompiler == null) {
            var runtime = T4RuntimeInfo.GetRuntime();
            if (runtime.Error != null) {
                throw new T4TemplatingEngineException(runtime.Error);
            }
            this.cachedCompiler = this.createCompilerFunc?.Invoke(runtime) ?? new RoslynCodeCompiler(runtime);
        }
        return this.cachedCompiler;
    }

    [Obsolete("Use ProcessTemplateAsync")]
    public string ProcessTemplate(string content, ITextTemplatingEngineHost host) {
        return this.ProcessTemplateAsync(content, host).Result;
    }

    public async Task<string> ProcessTemplateAsync(string content, ITextTemplatingEngineHost host, CancellationToken token = default) {
        using var tpl = await this.CompileTemplateAsync(content, host, token).ConfigureAwait(false);
        return tpl?.Process();
    }

    public async Task<string> ProcessTemplateAsync(T4ParsedTemplate pt, string content, T4TemplateSettings settings, ITextTemplatingEngineHost host, CancellationToken token = default) {
        var tpl = await this.CompileTemplateAsync(pt, content, host, settings, token).ConfigureAwait(false);
        using (tpl?.template) {
            return tpl?.template.Process();
        }
    }

    public string PreprocessTemplate(string content, ITextTemplatingEngineHost host, string className,
        string classNamespace, out string language, out string[] references) {
        if (content == null) {
            throw new ArgumentNullException(nameof(content));
        }

        if (host == null) {
            throw new ArgumentNullException(nameof(host));
        }

        if (className == null) {
            throw new ArgumentNullException(nameof(className));
        }

        language = null;
        references = null;

        var pt = T4ParsedTemplate.FromTextInternal(content, host);
        if (pt.Errors.HasErrors) {
            host.LogErrors(pt.Errors);
            return null;
        }
        return PreprocessTemplateInternal(pt, content, host, className, classNamespace, out language, out references);
    }

    [Obsolete("Use static method")]
    public string PreprocessTemplate(T4ParsedTemplate pt, string content, T4TemplateSettings settings, ITextTemplatingEngineHost host, out string language, out string[] references) {
        var result = PreprocessTemplate(pt, content, settings, host, out references);
        language = settings.Language;
        return result;
    }

    [Obsolete("Use TemplateGenerator")]
    public string PreprocessTemplate(T4ParsedTemplate pt, string content, ITextTemplatingEngineHost host, string className,
        string classNamespace, out string language, out string[] references, T4TemplateSettings settings = null) {
        if (content == null) {
            throw new ArgumentNullException(nameof(content));
        }

        if (pt == null) {
            throw new ArgumentNullException(nameof(pt));
        }

        if (host == null) {
            throw new ArgumentNullException(nameof(host));
        }

        if (className == null) {
            throw new ArgumentNullException(nameof(className));
        }

        return PreprocessTemplateInternal(pt, content, host, className, classNamespace, out language, out references, settings);
    }

    public static string PreprocessTemplate(T4ParsedTemplate pt, string content, T4TemplateSettings settings, ITextTemplatingEngineHost host, out string[] references) {
        if (pt is null) {
            throw new ArgumentNullException(nameof(pt));
        }

        if (string.IsNullOrEmpty(content)) {
            throw new ArgumentException($"'{nameof(content)}' cannot be null or empty.", nameof(content));
        }

        if (settings is null) {
            throw new ArgumentNullException(nameof(settings));
        }

        if (host is null) {
            throw new ArgumentNullException(nameof(host));
        }

        return PreprocessTemplateInternal(pt, content, settings, host, out references);
    }

    private static string PreprocessTemplateInternal(T4ParsedTemplate pt, string content, ITextTemplatingEngineHost host, string className,
        string classNamespace, out string language, out string[] references, T4TemplateSettings settings = null) {
        settings ??= GetSettings(host, pt);
        language = settings.Language;

        if (pt.Errors.HasErrors) {
            host.LogErrors(pt.Errors);
            language = null;
            references = null;
            return null;
        }

        if (className != null) {
            settings.Name = className;
        }
        if (classNamespace != null) {
            settings.Namespace = classNamespace;
        }

        return PreprocessTemplateInternal(pt, content, settings, host, out references);
    }

    internal static string PreprocessTemplateInternal(T4ParsedTemplate pt, string content, T4TemplateSettings settings, ITextTemplatingEngineHost host, out string[] references) {
        settings.IncludePreprocessingHelpers = string.IsNullOrEmpty(settings.Inherits);
        settings.IsPreprocessed = true;

        var ccu = GenerateCompileUnit(host, content, pt, settings);
        references = ProcessReferences(host, pt, settings).ToArray();

        host.LogErrors(pt.Errors);
        if (pt.Errors.HasErrors) {
            return null;
        }

        var options = new CodeGeneratorOptions();
        using var sw = new StringWriter();
        settings.Provider.GenerateCodeFromCompileUnit(ccu, sw, options);
        return sw.ToString();
    }

    [Obsolete("Use CompileTemplateAsync")]
    public T4CompiledTemplate CompileTemplate(string content, ITextTemplatingEngineHost host)
        => this.CompileTemplateAsync(content, host, CancellationToken.None).Result;

    public async Task<T4CompiledTemplate> CompileTemplateAsync(string content, ITextTemplatingEngineHost host, CancellationToken token) {
        if (content == null) {
            throw new ArgumentNullException(nameof(content));
        }

        if (host == null) {
            throw new ArgumentNullException(nameof(host));
        }

        var pt = T4ParsedTemplate.FromTextInternal(content, host);
        if (pt.Errors.HasErrors) {
            host.LogErrors(pt.Errors);
            return null;
        }

        var tpl = await this.CompileTemplateInternal(pt, content, host, null, token).ConfigureAwait(false);
        return tpl?.template;
    }

    public Task<(T4CompiledTemplate template, string[] references)?> CompileTemplateAsync(
        T4ParsedTemplate pt,
        string content,
        ITextTemplatingEngineHost host,
        T4TemplateSettings settings = null,
        CancellationToken token = default) {
        if (pt == null) {
            throw new ArgumentNullException(nameof(pt));
        }

        if (host == null) {
            throw new ArgumentNullException(nameof(host));
        }

        return this.CompileTemplateInternal(pt, content, host, settings, token);
    }

    private async Task<(T4CompiledTemplate template, string[] references)?> CompileTemplateInternal(
        T4ParsedTemplate pt,
        string content,
        ITextTemplatingEngineHost host,
        T4TemplateSettings settings,
        CancellationToken token
        ) {

        settings ??= GetSettings(host, pt);
        if (pt.Errors.HasErrors) {
            host.LogErrors(pt.Errors);
            return null;
        }

        if (!string.IsNullOrEmpty(settings.Extension)) {
            host.SetFileExtension(settings.Extension);
        }
        if (settings.Encoding != null) {
            //FIXME: when is this called with false?
            host.SetOutputEncoding(settings.Encoding, true);
        }

        var ccu = GenerateCompileUnit(host, content, pt, settings);
        var references = ProcessReferences(host, pt, settings);
        if (pt.Errors.HasErrors) {
            host.LogErrors(pt.Errors);
            return null;
        }

        (var results, var assembly) = await this.CompileCode(references, settings, ccu, token).ConfigureAwait(false);
        if (results.Errors.HasErrors) {
            host.LogErrors(pt.Errors);
            host.LogErrors(results.Errors);
            return null;
        }

        var compiledTemplate = new T4CompiledTemplate(host, assembly, settings.GetFullName(), settings.Culture, references);
#if FEATURE_APPDOMAINS
        compiledTemplate.SetTemplateContentForAppDomain(content);
#endif
        return (compiledTemplate, references);
    }

    private async Task<(CompilerResults, T4CompiledAssemblyData)> CompileCode(IEnumerable<string> references, T4TemplateSettings settings, CodeCompileUnit ccu, CancellationToken token) {
        string sourceText;
        var genOptions = new CodeGeneratorOptions();
        using (var sw = new StringWriter()) {
            settings.Provider.GenerateCodeFromCompileUnit(ccu, sw, genOptions);
            sourceText = sw.ToString();
        }

        T4CompiledAssemblyData compiledAssembly = null;

        // this may throw, so do it before writing source files
        var compiler = this.GetOrCreateCompiler();

        // GetTempFileName guarantees that the returned file name is unique, but
        // there are no equivalent for directories, so we create a directory
        // based on the file name, which *should* be unique as long as the file
        // exists.
        var tempFolder = TempSubdirectoryHelper.Create("t4-").FullName;

        if (settings.Log != null) {
            settings.Log.WriteLine($"Generating code in '{tempFolder}'");
        }

        var sourceFilename = Path.Combine(tempFolder, settings.Name + "." + settings.Provider.FileExtension);
        File.WriteAllText(sourceFilename, sourceText);

        var args = new T4CodeCompilerArguments();
        args.AssemblyReferences.AddRange(references);
        args.Debug = settings.Debug;
        args.SourceFiles.Add(sourceFilename);

        if (settings.CompilerOptions != null) {
            args.AdditionalArguments = settings.CompilerOptions;
        }

        args.OutputPath = Path.Combine(tempFolder, settings.Name + ".dll");
        args.TempDirectory = tempFolder;
        args.LangVersion = settings.LangVersion;

        var result = await compiler.CompileFile(args, settings.Log, token).ConfigureAwait(false);

        var r = new CompilerResults(new TempFileCollection(tempFolder));
        r.TempFiles.AddFile(sourceFilename, false);

        if (result.ResponseFile != null) {
            r.TempFiles.AddFile(result.ResponseFile, false);
        }

        r.NativeCompilerReturnValue = result.ExitCode;
        r.Output.AddRange(result.Output.ToArray());
        r.Errors.AddRange(result.Errors.Select(e => new CompilerError(e.Origin ?? "", e.Line, e.Column, e.Code, e.Message) { IsWarning = !e.IsError }).ToArray());

        if (result.Success) {
            r.TempFiles.AddFile(args.OutputPath, args.Debug);

            // load the assembly in memory so we can fully clean our temporary folder
            // NOTE: we do NOT assembly.load it here, as it will likely need to be loaded
            // into a different AssemblyLoadContext or AppDomain
            byte[] assembly = File.ReadAllBytes(args.OutputPath);
            byte[] debugSymbols = null;

            if (args.Debug) {
                var symbolsPath = Path.ChangeExtension(args.OutputPath, ".pdb");
                // if the symbols are embedded the symbols file doesn't exist
                if (File.Exists(symbolsPath)) {
                    r.TempFiles.AddFile(symbolsPath, true);
                    debugSymbols = File.ReadAllBytes(symbolsPath);
                }
            }

            compiledAssembly = new T4CompiledAssemblyData(assembly, debugSymbols);
        } else if (!r.Errors.HasErrors) {
            r.Errors.Add(new CompilerError(null, 0, 0, null, $"The compiler exited with code {result.ExitCode}"));
        }

        if (!args.Debug && !r.Errors.HasErrors) {
            r.TempFiles.Delete();
            // we can delete our temporary file after our temporary folder is deleted.
            Directory.Delete(tempFolder);
        }

        return (r, compiledAssembly);
    }

    private static string[] ProcessReferences(ITextTemplatingEngineHost host, T4ParsedTemplate pt, T4TemplateSettings settings) {
        var resolved = new Dictionary<string, string>();

        foreach (string assem in settings.Assemblies.Union(host.StandardAssemblyReferences)) {
            if (resolved.ContainsValue(assem)) {
                continue;
            }

            string resolvedAssem = host.ResolveAssemblyReference(assem);
            if (!string.IsNullOrEmpty(resolvedAssem)) {
                var assemblyName = resolvedAssem;
                if (File.Exists(resolvedAssem)) {
                    assemblyName = AssemblyName.GetAssemblyName(resolvedAssem).FullName;
                }

                resolved[assemblyName] = resolvedAssem;
            } else {
                pt.LogError("Could not resolve assembly reference '" + assem + "'");
                return null;
            }
        }
        return resolved.Values.ToArray();
    }

    public static T4TemplateSettings GetSettings(ITextTemplatingEngineHost host, T4ParsedTemplate pt) {
        var settings = new T4TemplateSettings();

        bool relativeLinePragmas = host.GetHostOption("UseRelativeLinePragmas") as bool? ?? false;
        foreach (T4Directive dt in pt.Directives) {
            switch (dt.Name.ToLowerInvariant()) {
                case "template":
                    string val = dt.Extract("language");
                    if (val != null) {
                        settings.Language = val;
                    }

                    if (dt.Extract("langversion") is string langVersion) {
                        settings.LangVersion = langVersion;
                    }

                    val = dt.Extract("debug");
                    if (val != null) {
                        settings.Debug = string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
                    }

                    val = dt.Extract("inherits");
                    if (val != null) {
                        settings.Inherits = val;
                    }

                    val = dt.Extract("culture");
                    if (val != null) {
                        var culture = System.Globalization.CultureInfo.GetCultureInfo(val);
                        if (culture == null) {
                            pt.LogWarning("Could not find culture '" + val + "'", dt.StartLocation);
                        } else {
                            settings.Culture = culture;
                        }
                    }
                    val = dt.Extract("hostspecific");
                    if (val != null) {
                        if (string.Equals(val, "trueFromBase", StringComparison.OrdinalIgnoreCase)) {
                            settings.HostPropertyOnBase = true;
                            settings.HostSpecific = true;
                        } else {
                            settings.HostSpecific = string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    val = dt.Extract("CompilerOptions");
                    if (val != null) {
                        settings.CompilerOptions = val;
                    }
                    val = dt.Extract("relativeLinePragmas");
                    if (val != null) {
                        relativeLinePragmas = string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
                    }
                    val = dt.Extract("linePragmas");
                    if (val != null) {
                        settings.NoLinePragmas = string.Equals(val, "false", StringComparison.OrdinalIgnoreCase);
                    }
                    val = dt.Extract("visibility");
                    if (val != null) {
                        settings.InternalVisibility = string.Equals(val, "internal", StringComparison.OrdinalIgnoreCase);
                    }
                    break;

                case "assembly":
                    string name = dt.Extract("name");
                    if (name == null) {
                        pt.LogError("Missing name attribute in assembly directive", dt.StartLocation);
                    } else {
                        settings.Assemblies.Add(name);
                    }

                    break;

                case "import":
                    string namespac = dt.Extract("namespace");
                    if (namespac == null) {
                        pt.LogError("Missing namespace attribute in import directive", dt.StartLocation);
                    } else {
                        settings.Imports.Add(namespac);
                    }

                    break;

                case "output":
                    settings.Extension = dt.Extract("extension");
                    string encoding = dt.Extract("encoding");
                    if (encoding != null) {
                        settings.Encoding = Encoding.GetEncoding(encoding);
                    }

                    break;

                case "include":
                    throw new InvalidOperationException("Include is handled in the parser");

                case "parameter":
                    AddDirective(settings, host, nameof(ParameterDirectiveProcessor), dt);
                    continue;

                default:
                    string processorName = dt.Extract("Processor") ?? throw new InvalidOperationException("Custom directive '" + dt.Name + "' does not specify a processor");
                    AddDirective(settings, host, processorName, dt);
                    continue;
            }
            ComplainExcessAttributes(dt, pt);
        }

        if (host is T4TemplateGenerator gen) {
            settings.HostType = gen.SpecificHostType;
            foreach (var processor in gen.GetAdditionalDirectiveProcessors()) {
                settings.DirectiveProcessors[processor.GetType().FullName] = processor;
            }
        }

        if (settings.HostType != null) {
            settings.Assemblies.Add(settings.HostType.Assembly.Location);
        }

        //initialize the custom processors
        foreach (var kv in settings.DirectiveProcessors) {
            kv.Value.Initialize(host);

            IRecognizeHostSpecific hs;
            if (settings.HostSpecific || (
                    !((IDirectiveProcessor)kv.Value).RequiresProcessingRunIsHostSpecific &&
                    ((hs = kv.Value as IRecognizeHostSpecific) == null || !hs.RequiresProcessingRunIsHostSpecific))) {
                continue;
            }

            settings.HostSpecific = true;
            pt.LogWarning("Directive processor '" + kv.Key + "' requires hostspecific=true, forcing on.");
        }

        foreach (var kv in settings.DirectiveProcessors) {
            kv.Value.SetProcessingRunIsHostSpecific(settings.HostSpecific);
            if (kv.Value is IRecognizeHostSpecific hs) {
                hs.SetProcessingRunIsHostSpecific(settings.HostSpecific);
            }

            if (kv.Value is IT4SupportCodeGenerationOptions opt) {
                opt.SetCodeGenerationOptions(settings.CodeGenerationOptions);
            }
        }

        if (settings.Name == null) {
            settings.Name = "GeneratedTextTransformation";
        }

        if (settings.Namespace == null) {
            settings.Namespace = $"{typeof(TextTransformation).Namespace}{new Random().Next():x}";
        }

        //resolve the CodeDOM provider
        if (string.IsNullOrEmpty(settings.Language)) {
            settings.Language = "C#";
        }

        if (settings.Language == "C#v3.5") {
            pt.LogWarning("The \"C#3.5\" Language attribute in template directives is deprecated, use the langversion attribute instead");
            settings.Provider = new CSharpCodeProvider(new Dictionary<string, string> {
                    { "CompilerVersion", "v3.5" }
                });
        } else {
            settings.Provider = CodeDomProvider.CreateProvider(settings.Language);
        }

        if (settings.Provider == null) {
            pt.LogError("A provider could not be found for the language '" + settings.Language + "'");
            return settings;
        }

        settings.RelativeLinePragmas = relativeLinePragmas;

#if FEATURE_APPDOMAINS
			settings.CodeGenerationOptions.UseRemotingCallContext = true;
#endif

        return settings;
    }

    private static void AddDirective(T4TemplateSettings settings, ITextTemplatingEngineHost host, string processorName, T4Directive directive) {
        if (!settings.DirectiveProcessors.TryGetValue(processorName, out IDirectiveProcessor processor)) {
            switch (processorName) {
                case "ParameterDirectiveProcessor":
                    processor = new ParameterDirectiveProcessor();
                    break;
                default:
                    Type processorType = host.ResolveDirectiveProcessor(processorName);
                    processor = (IDirectiveProcessor)Activator.CreateInstance(processorType);
                    break;
            }
            settings.DirectiveProcessors[processorName] = processor;
        }

        if (!processor.IsDirectiveSupported(directive.Name)) {
            throw new InvalidOperationException("Directive processor '" + processorName + "' does not support directive '" + directive.Name + "'");
        }

        settings.CustomDirectives.Add(new T4CustomDirective(processorName, directive));
    }

    private static bool ComplainExcessAttributes(T4Directive dt, T4ParsedTemplate pt) {
        if (dt.Attributes.Count == 0) {
            return false;
        }

        var sb = new StringBuilder("Unknown attributes ");
        bool first = true;
        foreach (string key in dt.Attributes.Keys) {
            if (!first) {
                sb.Append(", ");
            } else {
                first = false;
            }
            sb.Append(key);
        }
        sb.Append(" found in ");
        sb.Append(dt.Name);
        sb.Append(" directive.");
        pt.LogWarning(sb.ToString(), dt.StartLocation);
        return false;
    }
}
