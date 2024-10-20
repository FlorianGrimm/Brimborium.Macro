//
// Template.cs
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

using Microsoft.VisualStudio.TextTemplating;

namespace Mono.TextTemplating;

public class T4ParsedTemplate {
    private readonly List<IT4Segment> importedHelperSegments = new();
    private readonly string rootFileName;

    public T4ParsedTemplate(string rootFileName) {
        this.rootFileName = rootFileName;
    }

    public List<IT4Segment> RawSegments { get; } = new List<IT4Segment>();

    public IEnumerable<T4Directive> Directives {
        get {
            foreach (IT4Segment seg in this.RawSegments) {
                if (seg is T4Directive dir) {
                    yield return dir;
                }
            }
        }
    }

    public IEnumerable<T4TemplateSegment> Content {
        get {
            foreach (IT4Segment seg in this.RawSegments) {
                if (seg is T4TemplateSegment ts) {
                    yield return ts;
                }
            }
        }
    }

    public CompilerErrorCollection Errors { get; } = new CompilerErrorCollection();

    // this is deprecated to prevent accidentally passing a host without the TemplateFile property set
    [Obsolete("Use TemplateGenerator.ParseTemplate")]
    public static T4ParsedTemplate FromText(string content, ITextTemplatingEngineHost host) => FromTextInternal(content, host);

    internal static T4ParsedTemplate FromTextInternal(string content, ITextTemplatingEngineHost host) {
        var filePath = host.TemplateFile;
        var template = new T4ParsedTemplate(filePath);
        try {
            template.Parse(host, new HashSet<string>(StringComparer.OrdinalIgnoreCase), new T4Tokeniser(filePath, content), true);
        } catch (ParserException ex) {
            template.LogError(ex.Message, ex.Location);
        }
        return template;
    }

    // this is deprecated to prevent accidentally passing a host without the TemplateFile property set
    [Obsolete("Use TemplateGenerator.ParseTemplate")]
    public void Parse(ITextTemplatingEngineHost host, T4Tokeniser tokeniser) => this.Parse(host, new HashSet<string>(StringComparer.OrdinalIgnoreCase), tokeniser, true);

    [Obsolete("Should not have been public")]
    public void ParseWithoutIncludes(T4Tokeniser tokeniser) => this.Parse(null, null, tokeniser, false);

    private void Parse(ITextTemplatingEngineHost host, HashSet<string> includedFiles, T4Tokeniser tokeniser, bool parseIncludes) => this.Parse(host, includedFiles, tokeniser, parseIncludes, false);

    private void Parse(ITextTemplatingEngineHost host, HashSet<string> includedFiles, T4Tokeniser tokeniser, bool parseIncludes, bool isImport) {
        bool skip = false;
        bool addToImportedHelpers = false;
        while ((skip || tokeniser.Advance()) && tokeniser.State != State.EOF) {
            skip = false;
            
            // ISegment seg = null;
            T4TemplateSegment seg = null;
            switch (tokeniser.State) {
                case State.Block:
                    if (!string.IsNullOrEmpty(tokeniser.Value)) {
                        seg = new T4TemplateSegment(T4SegmentType.Block, tokeniser.Value, tokeniser.Location);
                    }

                    break;
                case State.Content:
                    if (!string.IsNullOrEmpty(tokeniser.Value)) {
                        seg = new T4TemplateSegment(T4SegmentType.Content, tokeniser.Value, tokeniser.Location);
                    }

                    break;
                case State.Expression:
                    if (!string.IsNullOrEmpty(tokeniser.Value)) {
                        seg = new T4TemplateSegment(T4SegmentType.Expression, tokeniser.Value, tokeniser.Location);
                    }

                    break;
                case State.Helper:
                    addToImportedHelpers = isImport;
                    if (!string.IsNullOrEmpty(tokeniser.Value)) {
                        seg = new T4TemplateSegment(T4SegmentType.Helper, tokeniser.Value, tokeniser.Location);
                    }

                    break;
                case State.Directive:
                    T4Directive directive = null;
                    string attName = null;
                    while (!skip && tokeniser.Advance()) {
                        switch (tokeniser.State) {
                            case State.DirectiveName:
                                if (directive == null) {
                                    directive = new T4Directive(tokeniser.Value, tokeniser.Location) {
                                        TagStartLocation = tokeniser.TagStartLocation
                                    };
                                    if (!parseIncludes || !string.Equals(directive.Name, "include", StringComparison.OrdinalIgnoreCase)) {
                                        this.RawSegments.Add(directive);
                                    }
                                } else {
                                    attName = tokeniser.Value;
                                }

                                break;
                            case State.DirectiveValue:
                                if (attName != null && directive != null) {
                                    directive.Attributes[attName] = tokeniser.Value;
                                } else {
                                    this.LogError("Directive value without name", tokeniser.Location);
                                }

                                attName = null;
                                break;
                            case State.Directive:
                                if (directive != null) {
                                    directive.EndLocation = tokeniser.TagEndLocation;
                                }

                                break;
                            default:
                                skip = true;
                                break;
                        }
                    }
                    if (parseIncludes && directive != null && string.Equals(directive.Name, "include", StringComparison.OrdinalIgnoreCase)) {
                        this.Import(host, includedFiles, directive, Path.GetDirectoryName(tokeniser.Location.FileName));
                    }

                    break;
                default:
                    throw new InvalidOperationException();
            }
            if (seg != null) {
                seg.TagStartLocation = tokeniser.TagStartLocation;
                seg.EndLocation = tokeniser.TagEndLocation;
                if (addToImportedHelpers) {
                    this.importedHelperSegments.Add(seg);
                } else {
                    this.RawSegments.Add(seg);
                }
            }
        }
        if (!isImport) {
            this.AppendAnyImportedHelperSegments();
        }
    }

    private static string FixWindowsPath(string path) => Path.DirectorySeparatorChar == '/' ? path.Replace('\\', '/') : path;

    private void Import(ITextTemplatingEngineHost host, HashSet<string> includedFiles, T4Directive includeDirective, string relativeToDirectory) {
        if (!includeDirective.Attributes.TryGetValue("file", out string rawFilename)) {
            this.LogError("Include directive has no file attribute", includeDirective.StartLocation);
            return;
        }

        string fileName = FixWindowsPath(rawFilename);

        bool once = false;
        if (includeDirective.Attributes.TryGetValue("once", out var onceStr)) {
            if (!bool.TryParse(onceStr, out once)) {
                this.LogError($"Include once attribute has unknown value '{onceStr}'", includeDirective.StartLocation);
            }
        }

        //try to resolve path relative to the file that included it
        if (relativeToDirectory != null && !Path.IsPathRooted(fileName)) {
            string possible = Path.Combine(relativeToDirectory, fileName);
            if (File.Exists(possible)) {
                fileName = Path.GetFullPath(possible);
            }
        }

        if (host.LoadIncludeText(fileName, out string content, out string resolvedName)) {
            // unfortunately we can't use the once check to avoid actually reading the file
            // as the host resolves the filename and reads the file in a single call
            if (!includedFiles.Add(resolvedName) && once) {
                return;
            }
            this.Parse(host, includedFiles, new T4Tokeniser(resolvedName, content), true, true);
        } else {
            this.LogError($"Could not resolve include file '{rawFilename}'.", includeDirective.StartLocation);
        }
    }

    private void AppendAnyImportedHelperSegments() {
        this.RawSegments.AddRange(this.importedHelperSegments);
        this.importedHelperSegments.Clear();
    }

    private void LogError(string message, T4Location location, bool isWarning) {
        var err = new CompilerError {
            ErrorText = message
        };
        if (location.FileName != null) {
            err.Line = location.Line;
            err.Column = location.Column;
            err.FileName = location.FileName ?? string.Empty;
        } else {
            err.FileName = this.rootFileName ?? string.Empty;
        }
        err.IsWarning = isWarning;
        this.Errors.Add(err);
    }

    public void LogError(string message) => this.LogError(message, T4Location.Empty, false);

    public void LogWarning(string message) => this.LogError(message, T4Location.Empty, true);

    public void LogError(string message, T4Location location) => this.LogError(message, location, false);

    public void LogWarning(string message, T4Location location) => this.LogError(message, location, true);
}

public interface IT4Segment {
    T4Location StartLocation { get; }
    T4Location EndLocation { get; set; }
    T4Location TagStartLocation { get; set; }
}

public class T4TemplateSegment : IT4Segment {
    public T4TemplateSegment(T4SegmentType type, string text, T4Location start) {
        this.Type = type;
        this.StartLocation = start;
        this.Text = text;
    }

    public T4SegmentType Type { get; private set; }
    public string Text { get; private set; }
    public T4Location TagStartLocation { get; set; }
    public T4Location StartLocation { get; private set; }
    public T4Location EndLocation { get; set; }
}

public class T4Directive : IT4Segment {
    public T4Directive(string name, T4Location start) {
        this.Name = name;
        this.Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        this.StartLocation = start;
    }

    public string Name { get; private set; }
    public Dictionary<string, string> Attributes { get; private set; }
    public T4Location TagStartLocation { get; set; }
    public T4Location StartLocation { get; private set; }
    public T4Location EndLocation { get; set; }

    public string Extract(string key) {
        if (!this.Attributes.TryGetValue(key, out var value)) {
            return null;
        }

        this.Attributes.Remove(key);
        return value;
    }
}

public enum T4SegmentType {
    Block,
    Expression,
    Content,
    Helper
}

public struct T4Location : IEquatable<T4Location> {
    public T4Location(string fileName, int line, int column) : this() {
        this.FileName = fileName;
        this.Column = column;
        this.Line = line;
    }

    public int Line { get; private set; }
    public int Column { get; private set; }
    public string FileName { get; private set; }

    public static T4Location Empty => new(null, -1, -1);

    public T4Location AddLine() => new(this.FileName, this.Line + 1, 1);

    public T4Location AddCol() => this.AddCols(1);

    public T4Location AddCols(int number) => new(this.FileName, this.Line, this.Column + number);

    public override string ToString() => $"[{this.FileName} ({this.Line},{this.Column})]";

    public bool Equals(T4Location other)
        => other.Line == this.Line && other.Column == this.Column && other.FileName == this.FileName;

    public override bool Equals(object obj) => obj is T4Location loc && this.Equals(loc);

    public override int GetHashCode() => (this.FileName, this.Line, this.Column).GetHashCode();

    public static bool operator ==(T4Location left, T4Location right) => left.Equals(right);

    public static bool operator !=(T4Location left, T4Location right) => !(left == right);
}
