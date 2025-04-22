using Brimborium.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Diagnostics.CodeAnalysis;

namespace Brimborium.Macro.Model;

public record class MacroRegionStart(
    StringSlice Text,
    StringSlice Payload,
    LocationTag LocationTag,
    SyntaxNodeType Kind,
    Nullable<SyntaxTrivia> SyntaxTrivia,
    RegionDirectiveTriviaSyntax? RegionDirective,
    AttributeSyntax? Attribute,
    SyntaxNode? SyntaxNode,
    Location? Location
    ) : MacroRegionNode(Text, Location)
    , IMacroRegionNode<MacroRegionStartBuilder> {

    /// <summary>
    /// for syntaxTrivia - comment
    /// </summary>
    /// <param name="Text">The captured Text</param>
    /// <param name="LocationTag">the last part #line</param>
    /// <param name="SyntaxTrivia">syntaxTrivia means comment</param>
    /// <param name="Location">the location of this Text</param>
    public MacroRegionStart(
        StringSlice Text,
        StringSlice Payload,
        LocationTag LocationTag,
        SyntaxTrivia SyntaxTrivia,
        Location? Location = default)
        : this(Text, Payload, LocationTag,
              SyntaxNodeType.SyntaxTrivia, SyntaxTrivia, default,
              null, null,
              Location ?? SyntaxTrivia.GetLocation()) { }


    /// <summary>
    /// for regionDirective - #region
    /// </summary>
    /// <param name="Text">The captured Text</param>
    /// <param name="LocationTag">the last part #line</param>
    /// <param name="RegionDirective">regionDirective means #region</param>
    /// <param name="Location">the location of this Text</param>
    public MacroRegionStart(
        StringSlice Text,
        StringSlice Payload,
        LocationTag LocationTag,
        RegionDirectiveTriviaSyntax RegionDirective,
        Location? Location = default)
        : this(Text, Payload, LocationTag,
              SyntaxNodeType.RegionDirectiveTriviaSyntax, default, RegionDirective,
              null, null,
              Location ?? RegionDirective.GetLocation()) { }

    public MacroRegionStart(
        StringSlice Text,
        StringSlice Payload,
        LocationTag LocationTag,
        AttributeSyntax Attribute,
        SyntaxNode SyntaxNode,
        Location? Location = default)
        : this(Text, Payload, LocationTag,
              SyntaxNodeType.AttributeSyntax, default, default,
              Attribute, SyntaxNode,
              Location ?? Attribute.GetLocation()) { }

    /// <summary>
    /// Gets the parsed Text associated with the start of the Text.
    /// </summary>
    public string? ParsedText =>
        this.SyntaxTrivia is { } syntaxTrivia ? syntaxTrivia.ToString()
        : this.RegionDirective is { } regionDirective ? regionDirective.ToString()
        : null;

    /// <summary>
    /// Gets the file path associated with the start of the Text.
    /// </summary>
    public string? FilePath => this.Location?.SourceTree?.FilePath;

    /// <summary>
    /// Gets the line number associated with the start of the Text.
    /// </summary>
    public int Line => this.Location is { } location
        ? location.GetLineSpan().StartLinePosition.Line + 1
        : 0;

    /// <summary>
    /// Tries to get the syntaxTrivia associated with the start of the region block.
    /// </summary>
    /// <param name="syntaxTrivia">the syntaxTrivia if kind is syntaxTrivia.</param>
    /// <param name="location">the location of the syntaxTrivia</param>
    /// <returns>true if found.</returns>
    public bool TryGetSyntaxTrivia(
        [MaybeNullWhen(false)] out SyntaxTrivia syntaxTrivia,
        [MaybeNullWhen(false)] out Location location
        ) {
        if (this.Kind == SyntaxNodeType.SyntaxTrivia) {
            if (this.SyntaxTrivia is { } thisSyntaxTrivia) {
                syntaxTrivia = thisSyntaxTrivia;
                location = this.Location ?? thisSyntaxTrivia.GetLocation();
                return true;
            } else {
                throw new Exception();
            }
        } else {
            syntaxTrivia = default;
            location = default;
            return false;
        }
    }
    /// <summary>
    /// Tries to get the end region directive associated with the end of the region block.
    /// </summary>
    /// <param name="regionDirective">The end region directive, if available.</param>
    /// <param name="location">The location of the end region directive, if available.</param>
    /// <returns>True if the end region directive is available; otherwise, false.</returns>
    public bool TryGetRegionDirective(
        [MaybeNullWhen(false)] out RegionDirectiveTriviaSyntax regionDirective,
        [MaybeNullWhen(false)] out Location location
        ) {
        if (this.Kind == SyntaxNodeType.RegionDirectiveTriviaSyntax) {
            if (this.RegionDirective is { } thisRegionDirective) {
                regionDirective = thisRegionDirective!;
                location = this.Location ?? thisRegionDirective.GetLocation();
                return true;
            } else {
                throw new Exception();
            }
        } else {
            regionDirective = default;
            location = default;
            return false;
        }
    }

    /// <summary>
    /// Tries to get the location associated with the end of the region block.
    /// </summary>
    /// <param name="location">The location, if available.</param>
    /// <returns>True if the location is available; otherwise, false.</returns>
    public bool TryGetLocation([MaybeNullWhen(false)] out Location location) {
        if (this.Kind == SyntaxNodeType.SyntaxTrivia) {
            if (this.SyntaxTrivia is { } thisSyntaxTrivia) {
                location = this.Location ?? thisSyntaxTrivia.GetLocation();
                return true;
            } else {
                throw new Exception();
            }
        } else if (this.Kind == SyntaxNodeType.RegionDirectiveTriviaSyntax) {
            if (this.RegionDirective is { } thisRegionDirective) {
                location = this.Location ?? thisRegionDirective.GetLocation();
                return true;
            } else {
                throw new Exception();
            }
        } else {
            location = this.Location;
            return null != this.Location;
        }
    }

    public MacroRegionStart WithLocationTag(LocationTag locationTag) {
        if (this.LocationTag.Equals(locationTag)) {
            return this;
        } else {
            return this with {
                LocationTag = locationTag
            };
        }
    }

    public override IMacroRegionNodeBuilder ConvertToBuilder() => this.ToBuilder();
    public MacroRegionStartBuilder ToBuilder()
        => new MacroRegionStartBuilder(this);

    public bool IsEmpty => this.Kind == SyntaxNodeType.None;

    public bool HasValue => !(this.Kind == SyntaxNodeType.None);

}

public class MacroRegionStartBuilder : MacroRegionNodeBuilder<MacroRegionStart> {
    public MacroRegionStartBuilder(MacroRegionStart? source)
        : base(source) {
    }

    public MacroRegionStartBuilder(
        StringSlice text,
        StringSlice payload,
        LocationTag locationTag,
        SyntaxNodeType kind,
        Nullable<SyntaxTrivia> syntaxTrivia,
        RegionDirectiveTriviaSyntax? regionDirective,
        AttributeSyntax? attribute,
        SyntaxNode? syntaxNode,
        Location? location
        ) : base(default) {
        this.Text = text;
        this._LocationTag = locationTag;
        this._Kind = kind;
        this._SyntaxTrivia = syntaxTrivia;
        this._RegionDirective = regionDirective;
        this._Attribute = attribute;
        this._SyntaxNode = syntaxNode;
        this.Location = location;
    }

    /// <summary>
    /// for syntaxTrivia - comment
    /// </summary>
    /// <param name="text">The captured Text</param>
    /// <param name="locationTag">the last part #line</param>
    /// <param name="syntaxTrivia">syntaxTrivia means comment</param>
    /// <param name="location">the location of this Text</param>
    public MacroRegionStartBuilder(
        StringSlice text,
        StringSlice payload,
        LocationTag locationTag,
        SyntaxTrivia syntaxTrivia,
        Location? location = default)
        : this(text,
              payload,
              locationTag,
              SyntaxNodeType.SyntaxTrivia, syntaxTrivia, default,
              null, null,
              location ?? syntaxTrivia.GetLocation()) { }

    /// <summary>
    /// for regionDirective - #region
    /// </summary>
    /// <param name="text">The captured Text</param>
    /// <param name="locationTag">the last part #line</param>
    /// <param name="regionDirective">regionDirective means #region</param>
    /// <param name="Location">the location of this Text</param>
    public MacroRegionStartBuilder(
        StringSlice text,
        StringSlice payload,
        LocationTag locationTag,
        RegionDirectiveTriviaSyntax regionDirective,
        Location? Location = default)
        : this(text, payload, locationTag,
              SyntaxNodeType.RegionDirectiveTriviaSyntax, default, regionDirective,
              null, null,
              Location ?? regionDirective.GetLocation()) { }

    public MacroRegionStartBuilder(
        StringSlice text,
        StringSlice payload,
        LocationTag locationTag,
        AttributeSyntax attribute,
        SyntaxNode syntaxNode,
        Location? Location = default)
        : this(text, payload, locationTag,
              SyntaxNodeType.AttributeSyntax, default, default,
              attribute, syntaxNode,
              Location ?? attribute.GetLocation()) { }

    protected override void Awake(MacroRegionStart source) {
        base.Awake(source);
        this._LocationTag = source.LocationTag;
        this._Kind = source.Kind;
        this._SyntaxTrivia = source.SyntaxTrivia;
        this._RegionDirective = source.RegionDirective;
        this._Attribute = source.Attribute;
        this._SyntaxNode = source.SyntaxNode;
    }

    private StringSlice? _Payload;

    public StringSlice? Payload {
        get {
            if (this._Source is { } source) {
                return source.Payload;
            } else {
                return this._Payload;
            }
        }
        set {
            this.EnsureAwake();
            this._Payload = value;
        }
    }

    private LocationTag _LocationTag;
    public LocationTag LocationTag {
        get {
            if (this._Source is { } source) {
                return source.LocationTag;
            } else {
                return this._LocationTag;
            }
        }
        set {
            this.EnsureAwake();
            this._LocationTag = value;
        }
    }

    private SyntaxNodeType _Kind;
    public SyntaxNodeType Kind {
        get {
            if (this._Source is { } source) {
                return source.Kind;
            } else {
                return this._Kind;
            }
        }
        set {
            this.EnsureAwake();
            this._Kind = value;
        }
    }

    private Nullable<SyntaxTrivia> _SyntaxTrivia;
    public Nullable<SyntaxTrivia> SyntaxTrivia {
        get {
            if (this._Source is { } source) {
                return source.SyntaxTrivia;
            } else {
                return this._SyntaxTrivia;
            }
        }
        set {
            this.EnsureAwake();
            this._SyntaxTrivia = value;
        }
    }

    private RegionDirectiveTriviaSyntax? _RegionDirective;
    public RegionDirectiveTriviaSyntax? RegionDirective {
        get {
            if (this._Source is { } source) {
                return source.RegionDirective;
            } else {
                return this._RegionDirective;
            }
        }
        set {
            this.EnsureAwake();
            this._RegionDirective = value;
        }
    }

    private AttributeSyntax? _Attribute;
    public AttributeSyntax? Attribute {
        get {
            if (this._Source is { } source) {
                return source.Attribute;
            } else {
                return this._Attribute;
            }
        }
        set {
            this.EnsureAwake();
            this._Attribute = value;
        }
    }

    private SyntaxNode? _SyntaxNode;
    public SyntaxNode? SyntaxNode {
        get {
            if (this._Source is { } source) {
                return source.SyntaxNode;
            } else {
                return this._SyntaxNode;
            }
        }
        set {
            this.EnsureAwake();
            this._SyntaxNode = value;
        }
    }

    public override MacroRegionStart Build() {
        if ((this._Source is { } source)) {
            return source;
        }
        if (!(this._Payload is { } payload)) {
            throw new Exception("Payload is null");
        }
        return new MacroRegionStart(
            Text: this.Text,
            Payload: payload,
            LocationTag: this.LocationTag,
            Kind: this.Kind,
            SyntaxTrivia: this.SyntaxTrivia,
            RegionDirective: this.RegionDirective,
            Attribute: this.Attribute,
            SyntaxNode: this.SyntaxNode,
            Location: this.Location);
    }
}