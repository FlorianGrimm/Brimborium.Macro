//
// TemplateEnginePreprocessTemplateTests.cs
//
// Author:
//       Matt Ward
//
// Copyright (c) 2011 Matt Ward
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

using System;
using System.CodeDom.Compiler;

using Xunit;

namespace Mono.TextTemplating.Tests;

public class TemplateEnginePreprocessTemplateTests {
    [Fact]
    public void PreprocessSimple() {
        string input =
            "<#@ template language=\"C#\" #>\r\n" +
            "Test\r\n";

        string expectedOutput = TemplatingEngineHelper.CleanCodeDom(OutputSample1, "\n");
        string output = Preprocess(input);

        Assert.Equal(expectedOutput, output);
    }

    [Fact]
    public void ControlBlockAfterIncludedTemplateWithClassFeatureBlock() {
        string input = InputTemplate_ControlBlockAfterIncludedTemplateWithClassFeatureBlock.NormalizeNewlines();
        DummyHost host = CreateDummyHostForControlBlockAfterIncludedTemplateWithClassFeatureBlockTest();
        host.HostOptions.Add("UseRelativeLinePragmas", true);

        string expectedOutput = TemplatingEngineHelper.CleanCodeDom(Output_ControlBlockAfterIncludedTemplateWithClassFeatureBlock.NormalizeNewlines(), "\n");
        string output = Preprocess(input, host);

        Assert.Equal(expectedOutput, output);
    }

    [Fact]
    public void CaptureEncodingAndExtension() {
        string input = InputTemplate_CaptureEncodingAndExtension.NormalizeNewlines();
        string output = Preprocess(input);
        string expectedOutput = TemplatingEngineHelper.CleanCodeDom(Output_CaptureEncodingAndExtension.NormalizeNewlines(), "\n");

        Assert.Equal(expectedOutput, output);
    }

    #region Helpers

    static string Preprocess(string input) {
        var host = new DummyHost();
        return Preprocess(input, host);
    }

    static string Preprocess(string input, DummyHost host) {
        string className = "PreprocessedTemplate";
        string classNamespace = "Templating";

        var engine = new T4TemplatingEngine();
        string output = engine.PreprocessTemplate(input, host, className, classNamespace, out _, out _);
        ReportErrors(host.Errors);
        if (output != null) {
            return TemplatingEngineHelper.CleanCodeDom(output, "\n");
        }
        return null;
    }

    static void ReportErrors(CompilerErrorCollection errors) {
        foreach (CompilerError error in errors) {
            Console.WriteLine(error.ErrorText);
        }
    }

    static DummyHost CreateDummyHostForControlBlockAfterIncludedTemplateWithClassFeatureBlockTest() {
        var host = new DummyHost();

        string includeTemplateRequestedFileName = @"Some\Requested\Path\IncludedFile.tt";
        if (System.IO.Path.DirectorySeparatorChar == '/') {
            includeTemplateRequestedFileName = includeTemplateRequestedFileName.Replace('\\', '/');
        }

        string includeTemplateResolvedFileName = @"Some\Resolved\Path\IncludedFile.tt";
        host.Locations.Add(includeTemplateRequestedFileName, includeTemplateResolvedFileName);
        host.Contents.Add(includeTemplateResolvedFileName, IncludedTemplate_ControlBlockAfterIncludedTemplate.NormalizeNewlines());

        return host;
    }

    #endregion

    #region Input templates

    const string InputTemplate_ControlBlockAfterIncludedTemplateWithClassFeatureBlock =
@"
<#@ template debug=""false"" language=""C#"" #>
<#@ output extension="".cs"" #>
Text Block 1
<#
    this.TemplateMethod();
#>
Text Block 2
<#@ include file=""Some\Requested\Path\IncludedFile.tt"" #>
Text Block 3
<#
    this.IncludedMethod();
#>
<#+
    void TemplateMethod()
    {
    }
#>
";

    const string IncludedTemplate_ControlBlockAfterIncludedTemplate =
@"
<#@ template debug=""false"" language=""C#"" #>
<#@ output extension="".cs"" #>
Included Text Block 1
<# this.WriteLine(""Included statement block""); #>
Included Text Block 2
<#+
    void IncludedMethod()
    {
#>
Included Method Body Text Block
<#+
    }
#>
";

    const string InputTemplate_CaptureEncodingAndExtension =
        @"
<#@ template debug=""false"" language=""C#"" inherits=""Foo"" hostspecific=""trueFromBase"" #>
<#@ output extension="".cs"" encoding=""utf-8"" #>
";

    #endregion

    #region Expected output strings

    const string OutputSample1 =
@"
namespace Templating {
    
    
    public partial class PreprocessedTemplate : PreprocessedTemplateBase {
        
        public virtual string TransformText() {
            this.GenerationEnvironment = null;
            
            #line 2 """"

            this.Write(""Test\r\n"");
            
            #line default
            #line hidden
            return this.GenerationEnvironment.ToString();
        }
        
        public virtual void Initialize() {
        }
    }
    
    public class PreprocessedTemplateBase {
        
        private global::System.Text.StringBuilder builder;
        
        private global::System.Collections.Generic.IDictionary<string, object> session;
        
        private global::System.CodeDom.Compiler.CompilerErrorCollection errors;
        
        private string currentIndent = string.Empty;
        
        private global::System.Collections.Generic.Stack<int> indents;
        
        private ToStringInstanceHelper _toStringHelper = new ToStringInstanceHelper();
        
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session {
            get {
                return this.session;
            }
            set {
                this.session = value;
            }
        }
        
        public global::System.Text.StringBuilder GenerationEnvironment {
            get {
                if ((this.builder == null)) {
                    this.builder = new global::System.Text.StringBuilder();
                }
                return this.builder;
            }
            set {
                this.builder = value;
            }
        }
        
        protected global::System.CodeDom.Compiler.CompilerErrorCollection Errors {
            get {
                if ((this.errors == null)) {
                    this.errors = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errors;
            }
        }
        
        public string CurrentIndent {
            get {
                return this.currentIndent;
            }
        }
        
        private global::System.Collections.Generic.Stack<int> Indents {
            get {
                if ((this.indents == null)) {
                    this.indents = new global::System.Collections.Generic.Stack<int>();
                }
                return this.indents;
            }
        }
        
        public ToStringInstanceHelper ToStringHelper {
            get {
                return this._toStringHelper;
            }
        }
        
        public void Error(string message) {
            this.Errors.Add(new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message));
        }
        
        public void Warning(string message) {
            global::System.CodeDom.Compiler.CompilerError val = new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message);
            val.IsWarning = true;
            this.Errors.Add(val);
        }
        
        public string PopIndent() {
            if ((this.Indents.Count == 0)) {
                return string.Empty;
            }
            int lastPos = (this.currentIndent.Length - this.Indents.Pop());
            string last = this.currentIndent.Substring(lastPos);
            this.currentIndent = this.currentIndent.Substring(0, lastPos);
            return last;
        }
        
        public void PushIndent(string indent) {
            this.Indents.Push(indent.Length);
            this.currentIndent = (this.currentIndent + indent);
        }
        
        public void ClearIndent() {
            this.currentIndent = string.Empty;
            this.Indents.Clear();
        }
        
        public void Write(string textToAppend) {
            this.GenerationEnvironment.Append(textToAppend);
        }
        
        public void Write(string format, params object[] args) {
            this.GenerationEnvironment.AppendFormat(format, args);
        }
        
        public void WriteLine(string textToAppend) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendLine(textToAppend);
        }
        
        public void WriteLine(string format, params object[] args) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendFormat(format, args);
            this.GenerationEnvironment.AppendLine();
        }
        
        public class ToStringInstanceHelper {
            
            private global::System.IFormatProvider formatProvider = global::System.Globalization.CultureInfo.InvariantCulture;
            
            public global::System.IFormatProvider FormatProvider {
                get {
                    return this.formatProvider;
                }
                set {
                    if ((value != null)) {
                        this.formatProvider = value;
                    }
                }
            }
            
            public string ToStringWithCulture(object objectToConvert) {
                if ((objectToConvert == null)) {
                    throw new global::System.ArgumentNullException(""objectToConvert"");
                }
                global::System.Type type = objectToConvert.GetType();
                global::System.Type iConvertibleType = typeof(global::System.IConvertible);
                if (iConvertibleType.IsAssignableFrom(type)) {
                    return ((global::System.IConvertible)(objectToConvert)).ToString(this.formatProvider);
                }
                global::System.Reflection.MethodInfo methInfo = type.GetMethod(""ToString"", new global::System.Type[] {
                            iConvertibleType});
                if ((methInfo != null)) {
                    return ((string)(methInfo.Invoke(objectToConvert, new object[] {
                                this.formatProvider})));
                }
                return objectToConvert.ToString();
            }
        }
    }
}
";

    const string Output_ControlBlockAfterIncludedTemplateWithClassFeatureBlock =
@"
namespace Templating {
    
    
    public partial class PreprocessedTemplate : PreprocessedTemplateBase {
        
        
        #line 14 """"
        
    void TemplateMethod()
    {
    }

        #line default
        #line hidden
        
        
        #line 7 ""Some\Resolved\Path\IncludedFile.tt""
        
    void IncludedMethod()
    {

        #line default
        #line hidden
        
        
        #line 11 ""Some\Resolved\Path\IncludedFile.tt""
        this.Write(""Included Method Body Text Block\n"");

        #line default
        #line hidden
        
        
        #line 12 ""Some\Resolved\Path\IncludedFile.tt""
        
    }

        #line default
        #line hidden
        
        public virtual string TransformText() {
            this.GenerationEnvironment = null;
            
            #line 1 """"
            this.Write(""\n"");
            
            #line default
            #line hidden
            
            #line 4 """"
            this.Write(""Text Block 1\n"");
            
            #line default
            #line hidden
            
            #line 5 """"

    this.TemplateMethod();

            
            #line default
            #line hidden
            
            #line 8 """"
            this.Write(""Text Block 2\n"");
            
            #line default
            #line hidden
            
            #line 1 ""Some\Resolved\Path\IncludedFile.tt""
            this.Write(""\n"");
            
            #line default
            #line hidden
            
            #line 4 ""Some\Resolved\Path\IncludedFile.tt""
            this.Write(""Included Text Block 1\n"");
            
            #line default
            #line hidden
            
            #line 5 ""Some\Resolved\Path\IncludedFile.tt""
 this.WriteLine(""Included statement block""); 
            
            #line default
            #line hidden
            
            #line 6 ""Some\Resolved\Path\IncludedFile.tt""
            this.Write(""Included Text Block 2\n"");
            
            #line default
            #line hidden
            
            #line 10 """"
            this.Write(""Text Block 3\n"");
            
            #line default
            #line hidden
            
            #line 11 """"

    this.IncludedMethod();

            
            #line default
            #line hidden
            return this.GenerationEnvironment.ToString();
        }
        
        public virtual void Initialize() {
        }
    }
    
    public class PreprocessedTemplateBase {
        
        private global::System.Text.StringBuilder builder;
        
        private global::System.Collections.Generic.IDictionary<string, object> session;
        
        private global::System.CodeDom.Compiler.CompilerErrorCollection errors;
        
        private string currentIndent = string.Empty;
        
        private global::System.Collections.Generic.Stack<int> indents;
        
        private ToStringInstanceHelper _toStringHelper = new ToStringInstanceHelper();
        
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session {
            get {
                return this.session;
            }
            set {
                this.session = value;
            }
        }
        
        public global::System.Text.StringBuilder GenerationEnvironment {
            get {
                if ((this.builder == null)) {
                    this.builder = new global::System.Text.StringBuilder();
                }
                return this.builder;
            }
            set {
                this.builder = value;
            }
        }
        
        protected global::System.CodeDom.Compiler.CompilerErrorCollection Errors {
            get {
                if ((this.errors == null)) {
                    this.errors = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errors;
            }
        }
        
        public string CurrentIndent {
            get {
                return this.currentIndent;
            }
        }
        
        private global::System.Collections.Generic.Stack<int> Indents {
            get {
                if ((this.indents == null)) {
                    this.indents = new global::System.Collections.Generic.Stack<int>();
                }
                return this.indents;
            }
        }
        
        public ToStringInstanceHelper ToStringHelper {
            get {
                return this._toStringHelper;
            }
        }
        
        public void Error(string message) {
            this.Errors.Add(new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message));
        }
        
        public void Warning(string message) {
            global::System.CodeDom.Compiler.CompilerError val = new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message);
            val.IsWarning = true;
            this.Errors.Add(val);
        }
        
        public string PopIndent() {
            if ((this.Indents.Count == 0)) {
                return string.Empty;
            }
            int lastPos = (this.currentIndent.Length - this.Indents.Pop());
            string last = this.currentIndent.Substring(lastPos);
            this.currentIndent = this.currentIndent.Substring(0, lastPos);
            return last;
        }
        
        public void PushIndent(string indent) {
            this.Indents.Push(indent.Length);
            this.currentIndent = (this.currentIndent + indent);
        }
        
        public void ClearIndent() {
            this.currentIndent = string.Empty;
            this.Indents.Clear();
        }
        
        public void Write(string textToAppend) {
            this.GenerationEnvironment.Append(textToAppend);
        }
        
        public void Write(string format, params object[] args) {
            this.GenerationEnvironment.AppendFormat(format, args);
        }
        
        public void WriteLine(string textToAppend) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendLine(textToAppend);
        }
        
        public void WriteLine(string format, params object[] args) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendFormat(format, args);
            this.GenerationEnvironment.AppendLine();
        }
        
        public class ToStringInstanceHelper {
            
            private global::System.IFormatProvider formatProvider = global::System.Globalization.CultureInfo.InvariantCulture;
            
            public global::System.IFormatProvider FormatProvider {
                get {
                    return this.formatProvider;
                }
                set {
                    if ((value != null)) {
                        this.formatProvider = value;
                    }
                }
            }
            
            public string ToStringWithCulture(object objectToConvert) {
                if ((objectToConvert == null)) {
                    throw new global::System.ArgumentNullException(""objectToConvert"");
                }
                global::System.Type type = objectToConvert.GetType();
                global::System.Type iConvertibleType = typeof(global::System.IConvertible);
                if (iConvertibleType.IsAssignableFrom(type)) {
                    return ((global::System.IConvertible)(objectToConvert)).ToString(this.formatProvider);
                }
                global::System.Reflection.MethodInfo methInfo = type.GetMethod(""ToString"", new global::System.Type[] {
                            iConvertibleType});
                if ((methInfo != null)) {
                    return ((string)(methInfo.Invoke(objectToConvert, new object[] {
                                this.formatProvider})));
                }
                return objectToConvert.ToString();
            }
        }
    }
}
";

    const string Output_CaptureEncodingAndExtension =

    @"namespace Templating {
    
    
    public partial class PreprocessedTemplate : Foo {
        
        public override string TransformText() {
            this.GenerationEnvironment = null;
            
            #line 1 """"
            this.Write(""\n"");
            
            #line default
            #line hidden
            return this.GenerationEnvironment.ToString();
        }
        
        public override void Initialize() {
            if ((this.Host != null)) {
                this.Host.SetFileExtension("".cs"");
                this.Host.SetOutputEncoding(System.Text.Encoding.GetEncoding(65001, true));
            }
            base.Initialize();
        }
    }
}";
    #endregion
}
