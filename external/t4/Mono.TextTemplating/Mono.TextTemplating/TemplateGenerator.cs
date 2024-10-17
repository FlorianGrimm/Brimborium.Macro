//
// TemplatingHost.cs
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

public class TemplateGenerator :
#if FEATURE_APPDOMAINS
		MarshalByRefObject,
#endif
        ITextTemplatingEngineHost, ITextTemplatingSessionHost {
    private static readonly Dictionary<string, string> KnownAssemblies = new(StringComparer.OrdinalIgnoreCase)
        {
            { "System.Core.dll", typeof(System.Linq.Enumerable).Assembly.Location },
            { "System.Data.dll", typeof(System.Data.DataTable).Assembly.Location },
            { "System.Linq.dll", typeof(System.Linq.Enumerable).Assembly.Location },
            { "System.Xml.dll", typeof(System.Xml.XmlAttribute).Assembly.Location },
            { "System.Xml.Linq.dll", typeof(System.Xml.Linq.XDocument).Assembly.Location },

            { "System.Core", typeof(System.Linq.Enumerable).Assembly.Location },
            { "System.Data", typeof(System.Data.DataTable).Assembly.Location },
            { "System.Linq", typeof(System.Linq.Enumerable).Assembly.Location },
            { "System.Xml", typeof(System.Xml.XmlAttribute).Assembly.Location },
            { "System.Xml.Linq", typeof(System.Xml.Linq.XDocument).Assembly.Location }
        };

    //re-usable
    private TemplatingEngine engine;

    //per-run variables
    private Encoding encoding;

    //host properties for consumers to access
    public CompilerErrorCollection Errors { get; } = new CompilerErrorCollection();
    public List<string> Refs { get; } = new List<string>();
    public List<string> Imports { get; } = new List<string>();
    public List<string> IncludePaths { get; } = new List<string>();
    public List<string> ReferencePaths { get; } = new List<string>();
    public string OutputFile { get; protected set; }
    public string TemplateFile { get; protected set; }
    public bool UseRelativeLinePragmas { get; set; }

    public TemplateGenerator() {
        this.Refs.Add(typeof(TextTransformation).Assembly.Location);
        this.Refs.Add(typeof(Uri).Assembly.Location);
        this.Refs.Add(typeof(File).Assembly.Location);
        this.Refs.Add(typeof(StringReader).Assembly.Location);
        this.Imports.Add("System");
    }

    [Obsolete("Use CompileTemplateAsync")]
    public CompiledTemplate CompileTemplate(string content) => this.CompileTemplateAsync(content, CancellationToken.None).Result;

    public Task<CompiledTemplate> CompileTemplateAsync(string content, CancellationToken token = default) {
        if (string.IsNullOrEmpty(content)) {
            throw new ArgumentNullException(nameof(content));
        }

        this.InitializeForRun();
        return this.Engine.CompileTemplateAsync(content, this, token);
    }

    protected internal TemplatingEngine Engine {
        get {
            if (this.engine == null) {
                this.engine = new TemplatingEngine();
            }

            return this.engine;
        }
    }

    private void InitializeForRun(string inputFileName = null, string outputFileName = null, Encoding encoding = null) {
        this.Errors.Clear();
        this.encoding = encoding ?? Utf8.BomlessEncoding;

        if (outputFileName is null && inputFileName is not null) {
            outputFileName = Path.ChangeExtension(inputFileName, ".txt");
        }

        this.TemplateFile = inputFileName;
        this.OutputFile = outputFileName;
    }

    [Obsolete("Use ProcessTemplateAsync")]
    public bool ProcessTemplate(string inputFile, string outputFile)
        => this.ProcessTemplateAsync(inputFile, outputFile, CancellationToken.None).Result;

    public async Task<bool> ProcessTemplateAsync(string inputFile, string outputFile, CancellationToken token = default) {
        if (string.IsNullOrEmpty(inputFile)) {
            throw new ArgumentNullException(nameof(inputFile));
        }

        if (string.IsNullOrEmpty(outputFile)) {
            throw new ArgumentNullException(nameof(outputFile));
        }

        this.InitializeForRun(inputFile, outputFile);

        string content;
        try {
#if NETCOREAPP2_1_OR_GREATER
            content = await File.ReadAllTextAsync(inputFile, token).ConfigureAwait(false);
#else
				content = File.ReadAllText (inputFile);
#endif
        } catch (IOException ex) {
            this.AddError("Could not read input file '" + inputFile + "':\n" + ex);
            return false;
        }

        var result = await this.ProcessTemplateAsync(inputFile, content, outputFile, token).ConfigureAwait(false);

        try {
            if (!this.Errors.HasErrors) {
#if NETCOREAPP2_1_OR_GREATER
                await File.WriteAllTextAsync(result.fileName, result.content, encoding, token).ConfigureAwait(false);
#else
					File.WriteAllText (result.fileName, result.content, this.encoding);
#endif
            }
        } catch (IOException ex) {
            this.AddError("Could not write output file '" + outputFile + "':\n" + ex);
        }

        return !this.Errors.HasErrors;
    }

    [Obsolete("Use ProcessTemplateAsync")]
    public bool ProcessTemplate(string inputFileName, string inputContent, ref string outputFileName, out string outputContent) {
        (outputFileName, outputContent, var success) = this.ProcessTemplateAsync(inputFileName, inputContent, outputFileName, CancellationToken.None).Result;
        return success;
    }

    public async Task<(string fileName, string content, bool success)> ProcessTemplateAsync(string inputFileName, string inputContent, string outputFileName, CancellationToken token = default) {
        this.InitializeForRun(inputFileName, outputFileName);

        var outputContent = await this.Engine.ProcessTemplateAsync(inputContent, this, token).ConfigureAwait(false);

        return (this.OutputFile, outputContent, !this.Errors.HasErrors);
    }

    public bool PreprocessTemplate(string inputFile, string className, string classNamespace,
        string outputFile, Encoding encoding, out string language, out string[] references) {
        language = null;
        references = null;

        if (string.IsNullOrEmpty(inputFile)) {
            throw new ArgumentNullException(nameof(inputFile));
        }

        if (string.IsNullOrEmpty(outputFile)) {
            throw new ArgumentNullException(nameof(outputFile));
        }

        this.InitializeForRun(inputFile, outputFile, encoding);

        string content;
        try {
            content = File.ReadAllText(inputFile);
        } catch (IOException ex) {
            this.AddError("Could not read input file '" + inputFile + "':\n" + ex);
            return false;
        }

        this.PreprocessTemplate(inputFile, className, classNamespace, content, out language, out references, out var output);

        try {
            if (!this.Errors.HasErrors) {
                File.WriteAllText(outputFile, output, encoding);
            }
        } catch (IOException ex) {
            this.AddError("Could not write output file '" + outputFile + "':\n" + ex);
        }

        return !this.Errors.HasErrors;
    }

    public bool PreprocessTemplate(string inputFileName, string className, string classNamespace, string inputContent,
        out string language, out string[] references, out string outputContent) {
        this.InitializeForRun(null, inputFileName);

        outputContent = this.Engine.PreprocessTemplate(inputContent, this, className, classNamespace, out language, out references);

        return !this.Errors.HasErrors;
    }

    private CompilerError AddError(string error) {
        var err = new CompilerError { ErrorText = error };
        this.Errors.Add(err);
        return err;
    }

    public ParsedTemplate ParseTemplate(string inputFile, string inputContent) {
        this.TemplateFile = inputFile;
        return ParsedTemplate.FromTextInternal(inputContent, this);
    }

    [Obsolete("Use overload without redundant `language` parameter")]
    public string PreprocessTemplate(
        ParsedTemplate pt,
        string inputFile,
        string inputContent,
        TemplateSettings settings,
        out string language,
        out string[] references) {
        this.InitializeForRun(inputFileName: inputFile);
        var result = TemplatingEngine.PreprocessTemplate(pt, inputContent, settings, this, out references);
        language = settings.Language;
        return result;
    }

    public string PreprocessTemplate(
        ParsedTemplate pt,
        string inputFile,
        string inputContent,
        TemplateSettings settings,
        out string[] references) {
        this.InitializeForRun(inputFileName: inputFile);
        return TemplatingEngine.PreprocessTemplate(pt, inputContent, settings, this, out references);
    }

    public async Task<(string fileName, string content)> ProcessTemplateAsync(
        ParsedTemplate pt,
        string inputFileName,
        string inputContent,
        string outputFileName,
        TemplateSettings settings,
        CancellationToken token = default) {
        this.InitializeForRun(inputFileName, outputFileName);

        var outputContent = await this.Engine.ProcessTemplateAsync(pt, inputContent, settings, this, token).ConfigureAwait(false);

        return (this.OutputFile, outputContent);
    }

    #region Virtual members

    public virtual object GetHostOption(string optionName)
        => optionName switch {
            "UseRelativeLinePragmas" => this.UseRelativeLinePragmas,
            _ => null,
        };

    public virtual AppDomain ProvideTemplatingAppDomain(string content) {
        return null;
    }

    protected virtual string ResolveAssemblyReference(string assemblyReference) {
        if (Path.IsPathRooted(assemblyReference)) {
            return assemblyReference;
        }

        foreach (string referencePath in this.ReferencePaths) {
            var path = Path.Combine(referencePath, assemblyReference);
            if (File.Exists(path)) {
                return path;
            }
        }

        var assemblyName = new AssemblyName(assemblyReference);
        if (assemblyName.Version != null)//Load via GAC and return full path
{
            return Assembly.Load(assemblyName).Location;
        }

        if (KnownAssemblies.TryGetValue(assemblyReference, out string mappedAssemblyReference)) {
            return mappedAssemblyReference;
        }

        if (!assemblyReference.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && !assemblyReference.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
            return assemblyReference + ".dll";
        }

        return assemblyReference;
    }

    protected virtual string ResolveParameterValue(string directiveId, string processorName, string parameterName) {
        var key = new ParameterKey(processorName, directiveId, parameterName);
        if (this.parameters.TryGetValue(key, out var value)) {
            return value;
        }

        if (processorName != null || directiveId != null) {
            return this.ResolveParameterValue(null, null, parameterName);
        }

        return null;
    }

    protected virtual Type ResolveDirectiveProcessor(string processorName) {
        if (!this.directiveProcessors.TryGetValue(processorName, out KeyValuePair<string, string> value)) {
            throw new TemplatingEngineException($"No directive processor registered as '{processorName}'");
        }

        var asmPath = this.ResolveAssemblyReference(value.Value) ?? throw new TemplatingEngineException($"Could not resolve assembly '{value.Value}' for directive processor '{processorName}'");
        var asm = Assembly.LoadFrom(asmPath);
        return asm.GetType(value.Key, true);
    }

    protected virtual string ResolvePath(string path) {
        if (!string.IsNullOrEmpty(path)) {
            path = Environment.ExpandEnvironmentVariables(path);
            if (Path.IsPathRooted(path)) {
                return path;
            }
        }

        // Get the template directory, or working directory if there is no file.
        // This can happen if the template text is passed in on the commandline.
        var dir = string.IsNullOrEmpty(this.TemplateFile)
            ? Environment.CurrentDirectory
            : Path.GetDirectoryName(Path.GetFullPath(this.TemplateFile));

        // if the user passed in null or string.empty, they just want the directory.
        if (string.IsNullOrEmpty(path)) {
            return dir;
        }

        var test = Path.Combine(dir, path);
        if (File.Exists(test) || Directory.Exists(test)) {
            return test;
        }

        return path;
    }

    #endregion

    private readonly Dictionary<ParameterKey, string> parameters = new();
    private readonly Dictionary<string, KeyValuePair<string, string>> directiveProcessors = new();

    public void AddDirectiveProcessor(string name, string klass, string assembly) {
        this.directiveProcessors.Add(name, new KeyValuePair<string, string>(klass, assembly));
    }

    public void AddParameter(string processorName, string directiveName, string parameterName, string value) {
        this.parameters.Add(new ParameterKey(processorName, directiveName, parameterName), value);
    }

    /// <summary>
    /// Parses a parameter and adds it.
    /// </summary>
    /// <returns>Whether the parameter was parsed successfully.</returns>
    /// <param name="unparsedParameter">Parameter in name=value or processor!directive!name!value format.</param>
    public bool TryAddParameter(string unparsedParameter) {
        if (TryParseParameter(unparsedParameter, out string processor, out string directive, out string name, out string value)) {
            this.AddParameter(processor, directive, name, value);
            return true;
        }
        return false;
    }

    internal static bool TryParseParameter(string parameter, out string processor, out string directive, out string name, out string value) {
        processor = directive = name = value = "";

        int start = 0;
        int end = parameter.IndexOfAny(parameterInitialSplitChars);
        if (end < 0) {
            return false;
        }

        //simple format n=v
        if (parameter[end] == '=') {
            name = parameter.Substring(start, end);
            value = parameter.Substring(end + 1);
            return !string.IsNullOrEmpty(name);
        }

        //official format, p!d!n!v
        processor = parameter.Substring(start, end);

        start = end + 1;
        end = parameter.IndexOf('!', start);
        if (end < 0) {
            //unlike official version, we allow you to omit processor/directive
            name = processor;
            value = parameter.Substring(start);
            processor = "";
            return !string.IsNullOrEmpty(name);
        }

        directive = parameter.Substring(start, end - start);


        start = end + 1;
        end = parameter.IndexOf('!', start);
        if (end < 0) {
            //we also allow you just omit the processor
            name = directive;
            directive = processor;
            value = parameter.Substring(start);
            processor = "";
            return !string.IsNullOrEmpty(name);
        }

        name = parameter.Substring(start, end - start);
        value = parameter.Substring(end + 1);

        return !string.IsNullOrEmpty(name);
    }

    protected virtual bool LoadIncludeText(string requestFileName, out string content, out string location) {
        content = "";
        location = this.ResolvePath(requestFileName);

        if (location == null || !File.Exists(location)) {
            foreach (string path in this.IncludePaths) {
                string f = Path.Combine(path, requestFileName);
                if (File.Exists(f)) {
                    location = f;
                    break;
                }
            }
        }

        if (location == null) {
            return false;
        }

        try {
            content = File.ReadAllText(location);
            return true;
        } catch (IOException ex) {
            this.AddError("Could not read included file '" + location + "':\n" + ex);
        }
        return false;
    }

    #region Explicit ITextTemplatingEngineHost implementation

    bool ITextTemplatingEngineHost.LoadIncludeText(string requestFileName, out string content, out string location) {
        return this.LoadIncludeText(requestFileName, out content, out location);
    }

    void ITextTemplatingEngineHost.LogErrors(CompilerErrorCollection errors) {
        this.Errors.AddRange(errors);
    }

    string ITextTemplatingEngineHost.ResolveAssemblyReference(string assemblyReference) {
        return this.ResolveAssemblyReference(assemblyReference);
    }

    string ITextTemplatingEngineHost.ResolveParameterValue(string directiveId, string processorName, string parameterName) {
        return this.ResolveParameterValue(directiveId, processorName, parameterName);
    }

    Type ITextTemplatingEngineHost.ResolveDirectiveProcessor(string processorName) {
        return this.ResolveDirectiveProcessor(processorName);
    }

    string ITextTemplatingEngineHost.ResolvePath(string path) {
        return this.ResolvePath(path);
    }

    void ITextTemplatingEngineHost.SetFileExtension(string extension) {
        if (this.OutputFile is not null) {
            extension = extension.TrimStart('.');
            if (Path.HasExtension(this.OutputFile)) {
                this.OutputFile = Path.ChangeExtension(this.OutputFile, extension);
            } else {
                this.OutputFile = this.OutputFile + "." + extension;
            }
        }
    }

    void ITextTemplatingEngineHost.SetOutputEncoding(Encoding encoding, bool fromOutputDirective) {
        this.encoding = encoding;
    }

    IList<string> ITextTemplatingEngineHost.StandardAssemblyReferences {
        get { return this.Refs; }
    }

    IList<string> ITextTemplatingEngineHost.StandardImports {
        get { return this.Imports; }
    }

    #endregion

    #region ITextTemplatingSession

    private ITextTemplatingSession session;

    /// <summary>
    /// Returns the current session instance, creating it if necessary.
    /// </summary>
    public ITextTemplatingSession GetOrCreateSession() => this.session ??= this.CreateSession();

    /// <summary>
    /// Called to create a session instance.
    /// Can be overridden to return a different <see cref="ITextTemplatingSession"/> implementation.
    /// </summary>
    protected virtual ITextTemplatingSession CreateSession() => new TextTemplatingSession();


    // Implement the session host interface for the template to use but hide the
    // API so we can expose a better one
    ITextTemplatingSession ITextTemplatingSessionHost.Session { get => this.session; set => this.session = value; }
    ITextTemplatingSession ITextTemplatingSessionHost.CreateSession() => this.session = this.CreateSession();

    public void ClearSession() => this.session = null;

    #endregion ITextTemplatingSession

    private struct ParameterKey : IEquatable<ParameterKey> {
        public ParameterKey(string processorName, string directiveName, string parameterName) {
            this.processorName = processorName ?? "";
            this.directiveName = directiveName ?? "";
            this.parameterName = parameterName ?? "";
            unchecked {
                this.hashCode = this.processorName.GetHashCode()
                        ^ this.directiveName.GetHashCode()
                        ^ this.parameterName.GetHashCode();
            }
        }

        private readonly string processorName, directiveName, parameterName;
        private readonly int hashCode;

        public override bool Equals(object obj) => obj is ParameterKey other && this.Equals(other);

        public bool Equals(ParameterKey other)
            => this.processorName == other.processorName && this.directiveName == other.directiveName && this.parameterName == other.parameterName;

        public override int GetHashCode() => this.hashCode;
    }

    /// <summary>
    /// If non-null, the template's Host property will be the full type of this host.
    /// </summary>
    public virtual Type SpecificHostType { get { return null; } }

    private static readonly char[] parameterInitialSplitChars = new[] { '=', '!' };

    /// <summary>
    /// Gets any additional directive processors to be included in the processing run.
    /// </summary>
    public virtual IEnumerable<IDirectiveProcessor> GetAdditionalDirectiveProcessors() {
        yield break;
    }
}
