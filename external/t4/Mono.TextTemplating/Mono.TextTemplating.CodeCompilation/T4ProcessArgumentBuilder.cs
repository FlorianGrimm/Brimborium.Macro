// 
// Derived from https://github.com/mono/monodevelop/blob/master/main/src/core/MonoDevelop.Core/MonoDevelop.Core.Execution/ProcessArgumentBuilder.cs
// at 388f883011578279a41be0a3fa8c3f1e53b55ab0
//  
// Author:
//       Mikayla Hutchinson <mhutchinson@novell.com>
// 
// Copyright (c) 2010 Novell, Inc.
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

// TODO: WEICHEI - remove unnecessary parts

/// <summary>
/// Builds a process argument string.
/// </summary>
internal class T4ProcessArgumentBuilder {
    private readonly StringBuilder sb = new();

    public string ProcessPath { get; }

    // .NET doesn't allow escaping chars other than " and \ inside " quotes
    private const string escapeDoubleQuoteCharsStr = "\\\"";

    public T4ProcessArgumentBuilder() {

    }

    public T4ProcessArgumentBuilder(string processPath) {
        this.ProcessPath = processPath;
    }

    /// <summary>
    /// Adds an argument without escaping or quoting.
    /// </summary>
    public void Add(string argument) {
        if (this.sb.Length > 0) {
            this.sb.Append(' ');
        }

        this.sb.Append(argument);
    }

    /// <summary>
    /// Adds multiple arguments without escaping or quoting.
    /// </summary>
    public void Add(params string[] args) {
        foreach (var a in args) {
            this.Add(a);
        }
    }

    /// <summary>Adds an argument, quoting and escaping as necessary.</summary>
    /// <remarks>The .NET process class does not support escaped 
    /// arguments, only quoted arguments with escaped quotes.</remarks>
    public void AddQuoted(string argument) {
        if (argument == null) {
            return;
        }

        if (this.sb.Length > 0) {
            this.sb.Append(' ');
        }

        this.sb.Append('"');
        AppendEscaped(this.sb, escapeDoubleQuoteCharsStr, argument);
        this.sb.Append('"');
    }

    /// <summary>
    /// Adds multiple arguments, quoting and escaping each as necessary.
    /// </summary>
    public void AddQuoted(params string[] args) {
        foreach (var a in args) {
            this.AddQuoted(a);
        }
    }

    /// <summary>Quotes a string, escaping if necessary.</summary>
    /// <remarks>The .NET process class does not support escaped 
    /// arguments, only quoted arguments with escaped quotes.</remarks>
    public static string Quote(string s) {
        var sb = new StringBuilder();
        sb.Append('"');
        AppendEscaped(sb, escapeDoubleQuoteCharsStr, s);
        sb.Append('"');
        return sb.ToString();
    }

    public override string ToString() {
        return this.sb.ToString();
    }

    private static void AppendEscaped(StringBuilder sb, string escapeChars, string s) {
        for (int i = 0; i < s.Length; i++) {
            char c = s[i];
            if (escapeChars.IndexOf(c) > -1) {
                sb.Append('\\');
            }

            sb.Append(c);
        }
    }

    private static string GetArgument(StringBuilder builder, string buf, int startIndex, out int endIndex, out Exception ex) {
        bool escaped = false;
        char qchar, c = '\0';
        int i = startIndex;

        builder.Clear();
        switch (buf[startIndex]) {
            case '\'': qchar = '\''; i++; break;
            case '"': qchar = '"'; i++; break;
            default: qchar = '\0'; break;
        }

        while (i < buf.Length) {
            c = buf[i];

            if (c == qchar && !escaped) {
                // unescaped qchar means we've reached the end of the argument
                i++;
                break;
            }

            if (c == '\\') {
                escaped = true;
            } else if (escaped) {
                builder.Append(c);
                escaped = false;
            } else if (qchar == '\0' && (c == ' ' || c == '\t')) {
                break;
            } else if (qchar == '\0' && (c == '\'' || c == '"')) {
                string sofar = builder.ToString();
                string embedded;

                if ((embedded = GetArgument(builder, buf, i, out endIndex, out ex)) == null) {
                    return null;
                }

                i = endIndex;
                builder.Clear();
                builder.Append(sofar);
                builder.Append(embedded);
                continue;
            } else {
                builder.Append(c);
            }

            i++;
        }

        if (escaped || (qchar != '\0' && c != qchar)) {
            ex = new FormatException(escaped ? "Incomplete escape sequence." : "No matching quote found.");
            endIndex = -1;
            return null;
        }

        endIndex = i;
        ex = null;

        return builder.ToString();
    }

    private static bool TryParse(string commandline, out string[] argv, out Exception ex) {
        var builder = new StringBuilder();
        var args = new List<string>();
        string argument;
        int i = 0;
        char c;

        while (i < commandline.Length) {
            c = commandline[i];
            if (c != ' ' && c != '\t') {
                if ((argument = GetArgument(builder, commandline, i, out int j, out ex)) == null) {
                    argv = null;
                    return false;
                }

                args.Add(argument);
                i = j;
            }

            i++;
        }

        argv = args.ToArray();
        ex = null;

        return true;
    }

    public static bool TryParse(string commandline, out string[] argv)
        => TryParse(commandline, out argv, out _);

    public static string[] Parse(string commandline)
        => TryParse(commandline, out string[] argv, out Exception ex)
            ? argv
            : throw ex;
}
