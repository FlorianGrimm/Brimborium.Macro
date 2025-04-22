using Brimborium.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Diagnostics.CodeAnalysis;

namespace Brimborium.Macro.Model;

public record class MacroRegionEnd(
    StringSlice Text,
    StringSlice Payload,
    LocationTag LocationTag,
    SyntaxNodeType Kind,
    SyntaxTrivia? SyntaxTrivia,
    EndRegionDirectiveTriviaSyntax? RegionDirective,
    Location? Location
) : MacroRegionNode(Text, Location) {
    /// <summary>
    /// Initializes a new instance of the <see cref="MacroRegionEnd"/> struct with syntax trivia.
    /// </summary>
    /// <param name="Text">The text associated with the end of the region block.</param>
    /// <param name="LocationTag">The location tag</param>
    /// <param name="SyntaxTrivia">The syntax trivia associated with the end of the region block.</param>
    /// <param name="Location">The location of the end of the region block.</param>
    public MacroRegionEnd(StringSlice Text, StringSlice Payload, LocationTag LocationTag, SyntaxTrivia SyntaxTrivia, Location? Location = default)
        : this(Text, Payload, LocationTag, SyntaxNodeType.SyntaxTrivia, SyntaxTrivia, default, Location ?? SyntaxTrivia.GetLocation()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroRegionEnd"/> struct with an end region directive.
    /// </summary>
    /// <param name="Text">The text associated with the end of the region block.</param>
    /// <param name="LocationTag">The location tag</param>
    /// <param name="RegionDirective">The end region directive trivia syntax.</param>
    /// <param name="Location">The location of the end of the region block.</param>
    public MacroRegionEnd(StringSlice Text, StringSlice Payload, LocationTag LocationTag, EndRegionDirectiveTriviaSyntax RegionDirective, Location? Location = default)
        : this(Text, Payload, LocationTag, SyntaxNodeType.RegionDirectiveTriviaSyntax, default, RegionDirective, Location ?? RegionDirective.GetLocation()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacroRegionEnd"/> struct with an end region directive.
    /// </summary>
    /// <param name="Text">The text associated with the end of the region block.</param>
    /// <param name="LocationTag">The location tag</param>
    /// <param name="Location">The location of the end of the region block.</param>
    public MacroRegionEnd(StringSlice Text, StringSlice Payload, LocationTag LocationTag, Location Location)
        : this(Text, Payload, LocationTag, SyntaxNodeType.AttributeSyntax, default, default, Location) { }

    /// <summary>
    /// Tries to get the syntax trivia associated with the end of the region block.
    /// </summary>
    /// <param name="syntaxTrivia">The syntax trivia, if available.</param>
    /// <param name="location">The location of the syntax trivia, if available.</param>
    /// <returns>True if the syntax trivia is available; otherwise, false.</returns>
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


    public bool TryGetRegionDirective(
        [MaybeNullWhen(false)] out EndRegionDirectiveTriviaSyntax regionDirective,
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
    /// <param name="location">the location</param>
    /// <returns>true if found</returns>
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

    public MacroRegionEnd WithLocationTag(LocationTag locationTag) {
        if (this.LocationTag.Equals(locationTag)) {
            return this;
        } else {
            return this with {
                LocationTag = locationTag
            };
        }
    }

    public override IMacroRegionNodeBuilder ConvertToBuilder() => this.ToBuilder();
    public MacroRegionEndBuilder ToBuilder() => new MacroRegionEndBuilder(this);

    public bool HasValue => !(this.Kind == SyntaxNodeType.None);
    public bool IsEmpty => this.Kind == SyntaxNodeType.None;
}

public class MacroRegionEndBuilder : MacroRegionNodeBuilder<MacroRegionEnd> {
    public MacroRegionEndBuilder(MacroRegionEnd? source)
        : base(source) {
    }

    public MacroRegionEndBuilder(
        StringSlice text,
        StringSlice payload,
        LocationTag locationTag,
        SyntaxNodeType kind,
        Nullable<SyntaxTrivia> syntaxTrivia,
        EndRegionDirectiveTriviaSyntax? regionDirective,
        //AttributeSyntax? attribute,
        //SyntaxNode? syntaxNode,
        Location? location
        )
        : base(default) {
        this.Text = text;
        this._Payload = payload;
        this._LocationTag = locationTag;
        this._Kind = kind;
        this._SyntaxTrivia = syntaxTrivia;
        this._RegionDirective = regionDirective;
        this.Location = location;
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
    private SyntaxNodeType Kind {
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

    private SyntaxTrivia? _SyntaxTrivia;
    public SyntaxTrivia? SyntaxTrivia {
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

    private EndRegionDirectiveTriviaSyntax? _RegionDirective;
    public EndRegionDirectiveTriviaSyntax? RegionDirective {
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

    protected override void Awake(MacroRegionEnd source) {
        base.Awake(source);
        this._SyntaxTrivia = source.SyntaxTrivia;
        this._RegionDirective = source.RegionDirective;
        this._LocationTag = source.LocationTag;
    }

    public override MacroRegionEnd Build() {
        if (this._Source is { } source) {
            return source;
        }
        return new MacroRegionEnd(
            Text: this.Text,
            Payload: this.Payload ?? StringSlice.Empty,
            LocationTag: this.LocationTag,
            Kind: this.Kind,
            SyntaxTrivia: this.SyntaxTrivia,
            RegionDirective: this.RegionDirective,
            Location: this.Location);
    }
}