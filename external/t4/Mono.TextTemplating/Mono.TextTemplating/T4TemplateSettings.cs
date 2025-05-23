// 
// TemplateSettings.cs
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

public class T4TemplateSettings {
    public T4TemplateSettings() {
        this.Imports = new HashSet<string>();
        this.Assemblies = new HashSet<string>();
        this.CustomDirectives = new List<T4CustomDirective>();
        this.DirectiveProcessors = new Dictionary<string, IDirectiveProcessor>();
    }

    public bool HostSpecific { get; set; }
    public bool HostPropertyOnBase { get; set; }
    public bool Debug { get; set; }
    public TextWriter Log { get; set; }
    public string Inherits { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public HashSet<string> Imports { get; private set; }
    public HashSet<string> Assemblies { get; private set; }
    public System.CodeDom.Compiler.CodeDomProvider Provider { get; set; }
    public string Language { get; set; }
    public string LangVersion { get; set; }
    public string CompilerOptions { get; set; }
    public Encoding Encoding { get; set; }
    public string Extension { get; set; }
    public System.Globalization.CultureInfo Culture { get; set; }
    public List<T4CustomDirective> CustomDirectives { get; private set; }
    public Dictionary<string, IDirectiveProcessor> DirectiveProcessors { get; private set; }
    public bool IncludePreprocessingHelpers { get; set; }
    public bool IsPreprocessed { get; set; }
    public bool RelativeLinePragmas { get; set; }
    public bool NoLinePragmas { get; set; }
    public bool InternalVisibility { get; set; }
    public Type HostType { get; set; }
    public string GetFullName() => string.IsNullOrEmpty(this.Namespace) ? this.Name : this.Namespace + "." + this.Name;

    internal T4CodeGenerationOptions CodeGenerationOptions { get; } = new T4CodeGenerationOptions();

    /// <summary>
    /// Base directory for calculation of relative line pragmas.
    /// Internal until we clean up the settings API.
    /// </summary>
    internal string RelativeLinePragmasBaseDirectory { get; set; }
}

public class T4CustomDirective {
    public T4CustomDirective(string processorName, T4Directive directive) {
        this.ProcessorName = processorName;
        this.Directive = directive;
    }

    public string ProcessorName { get; set; }
    public T4Directive Directive { get; set; }
}
