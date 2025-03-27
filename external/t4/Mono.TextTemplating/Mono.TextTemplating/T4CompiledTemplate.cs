#pragma warning disable CS0649 // HACK

//
// CompiledTemplate.cs
//
// Author:
//       Nathan Baulch <nathan.baulch@gmail.com>
//
// Copyright (c) 2009 Nathan Baulch
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

using Microsoft.VisualStudio.TextTemplating;

namespace Mono.TextTemplating;

public sealed partial class T4CompiledTemplate :
#if FEATURE_APPDOMAINS
		MarshalByRefObject,
#endif
        IDisposable {
    private readonly ITextTemplatingEngineHost host;
    private readonly CultureInfo culture;
    private readonly string templateClassFullName;
    private readonly T4CompiledAssemblyData templateAssemblyData;
    private readonly string templateAssemblyFile;

    internal string[] ReferencedAssemblyFiles { get; }

    internal T4CompiledTemplate(ITextTemplatingEngineHost host, T4CompiledAssemblyData templateAssemblyData, string fullName, CultureInfo culture, string[] referencedAssemblyFiles)
        : this(fullName, host, culture, referencedAssemblyFiles) {
        this.templateAssemblyData = templateAssemblyData;
    }

    private T4CompiledTemplate(string templateClassFullName, ITextTemplatingEngineHost host, CultureInfo culture, string[] referencedAssemblyFiles) {
        this.templateClassFullName = templateClassFullName;
        this.host = host;
        this.culture = culture;
        this.ReferencedAssemblyFiles = referencedAssemblyFiles;
    }

#if FEATURE_APPDOMAINS
    string templateContentForAppDomain;
    internal void SetTemplateContentForAppDomain(string content) {
        templateContentForAppDomain = content;
    }

    TemplateProcessor CreateTemplateProcessor() {
        var domain = host.ProvideTemplatingAppDomain(templateContentForAppDomain);

        // hosts are supposed to return null of they don't want to use a domain
        // but check for CurrentDomain too so we can optimize if they do that
        if (domain == null || domain == AppDomain.CurrentDomain) {
            return new TemplateProcessor();
        }

        var templateProcessorType = typeof(TemplateProcessor);

        try {
            var obj = domain.CreateInstanceAndUnwrap(templateProcessorType.Assembly.FullName, templateProcessorType.FullName);
            return (TemplateProcessor)obj;
        } catch (Exception ex) when (ex is MissingMethodException || ex is System.IO.FileNotFoundException || ex is System.Runtime.Serialization.SerializationException) {
            throw new TemplatingEngineException(
                $"Could not instantiate type {templateProcessorType.FullName} in templating AppDomain '{domain.FriendlyName ?? "(no name)"}'. " +
                $"The assembly '{templateProcessorType.Assembly.FullName}' must be resolvable in the domain. " +
                $"The AppDomain's base directory may be incorrect: '{domain.BaseDirectory}'",
                ex);
        }
    }
#else
    private static TemplateProcessor CreateTemplateProcessor() => new();
#endif

    public string Process() {
        TemplateProcessor processor = CreateTemplateProcessor();
        return processor.CreateAndProcess(this.host, this.templateAssemblyData, this.templateAssemblyFile, this.templateClassFullName, this.culture, this.ReferencedAssemblyFiles);
    }

    public void Dispose() {
    }
}
