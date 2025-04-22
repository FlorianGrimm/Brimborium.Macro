using Brimborium.Text;

using Microsoft.CodeAnalysis;

namespace Brimborium.Macro.Model;

public interface IMacroRegionNode {
    StringSlice Text { get; }
    Location? Location { get; }
    IMacroRegionNodeBuilder ConvertToBuilder();
}

public interface IMacroRegionNode<TBuilder>
    : IMacroRegionNode
    where TBuilder : class, IMacroRegionNodeBuilder {
    //public TBuilder ToBuilder() { return (TBuilder) this.ConvertToBuilder(); }
    TBuilder ToBuilder();
}

public interface IMacroRegionNodeBuilder {
    StringSlice Text { get; set; }
    Location? Location { get; set; }
    MacroRegionNode ConvertToInstance();
}

public interface IMacroRegionNodeBuilder<TInstance>
    : IMacroRegionNodeBuilder
    where TInstance : MacroRegionNode {
    //public TInstance Build() { return (TInstance) this.ConvertToInstance(); }
    TInstance Build();
}


public abstract record class MacroRegionNode(
    StringSlice Text,
    Location? Location
    ) : IMacroRegionNode {
    public abstract IMacroRegionNodeBuilder ConvertToBuilder();
}

public abstract class MacroRegionNodeBuilder<T>
    : IMacroRegionNodeBuilder
    where T : MacroRegionNode {
    protected T? _Source;

    public MacroRegionNodeBuilder(T? source) {
        this._Source = source;
    }

    private StringSlice _Text;

    public StringSlice Text {
        get {
            if (this._Source is { } source) {
                return source.Text;
            } else {
                return this._Text;
            }
        }
        set {
            this.EnsureAwake();
            this._Text = value;
        }
    }

    private Location? _Location;

    public Location? Location {
        get {
            if (this._Source is { } source) {
                return source.Location;
            } else {
                return this._Location;
            }
        }
        set {
            this.EnsureAwake();
            this._Location = value;
        }
    }


    protected void EnsureAwake() {
        if (this._Source is { } source) {
            this._Source = null;
            this.Awake(source);
        }
    }

    protected virtual void Awake(T source) {
        this._Text = source.Text;
        this._Location = source.Location;
    }

    MacroRegionNode IMacroRegionNodeBuilder.ConvertToInstance() => this.Build();

    public abstract T Build();
}
