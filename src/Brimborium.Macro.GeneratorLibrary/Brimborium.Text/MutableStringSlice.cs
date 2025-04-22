namespace Brimborium.Text;

/// <summary>
/// Represents a mutable slice of a string, storing the text and a range that defines the slice boundaries.
/// Unlike <see cref="StringSlice"/>, this class allows modification of the range while maintaining
/// a read-only reference to the underlying text.
/// </summary>
/// <remarks>
/// MutableStringSlice provides a way to work with portions of strings while maintaining the ability to modify
/// the range. The underlying text remains immutable after construction. It implements equality comparison 
/// and can be used in collections.
/// </remarks>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class MutableStringSlice : IEquatable<MutableStringSlice> {
    private readonly string _Text;
    private Range _Range;

    /// <summary>
    /// Initializes a new instance of the <see cref="MutableStringSlice"/> class with an empty string.
    /// </summary>
    public MutableStringSlice() {
        this._Text = string.Empty;
        this._Range = 0..0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MutableStringSlice"/> class with the specified text.
    /// The range is set to cover the entire text.
    /// </summary>
    /// <param name="text">The text to initialize with. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    public MutableStringSlice(string text) {
        ArgumentNullException.ThrowIfNull(text);
        this._Text = text;
        this._Range = 0..text.Length;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MutableStringSlice"/> class with the specified text and range.
    /// </summary>
    /// <param name="text">The text to initialize with. Cannot be null.</param>
    /// <param name="range">The range within the text that defines the slice.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the range is invalid for the given text.</exception>
    public MutableStringSlice(string text, Range range) {
        ArgumentNullException.ThrowIfNull(text);

        var (offset, length) = range.GetOffsetAndLength(text.Length);
        if (offset < 0 
            || length < 0 
            || text.Length < offset 
            || text.Length < (offset + length)) {
            throw new ArgumentOutOfRangeException(nameof(range));
        }

        this._Text = text;
        this._Range = range;
    }

    /// <summary>
    /// Gets the underlying text.
    /// </summary>
    public string Text => this._Text;

    /// <summary>
    /// Gets or sets the range that defines the slice within the text.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the new range is invalid for the current text.</exception>
    public Range Range { 
        get => this._Range; 
        set {
            if (this._Range.Equals(value)) { return; }
            
            var (offset, length) = value.GetOffsetAndLength(this._Text.Length);
            if (offset < 0 
                || length < 0 
                || this._Text.Length < offset 
                || this._Text.Length < (offset + length)) {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            this._Range = value;
        }
    }

    /// <summary>
    /// Gets the length of the slice.
    /// </summary>
    public int Length {
        get {
            var (_, length) = this._Range.GetOffsetAndLength(this._Text.Length);
            return length;
        }
    }

    /// <summary>
    /// Creates an immutable <see cref="StringSlice"/> from this instance.
    /// </summary>
    /// <returns>A new <see cref="StringSlice"/> with the same text and range.</returns>
    public StringSlice AsStringSlice() => new StringSlice(this._Text, this._Range);

    /// <summary>
    /// Deconstructs the instance into its text and range components.
    /// </summary>
    /// <param name="text">The underlying text.</param>
    /// <param name="range">The range defining the slice.</param>
    public void Deconstruct(out string text, out Range range) {
        text = this._Text;
        range = this._Range;
    }

    /// <summary>
    /// Determines whether this instance equals another <see cref="MutableStringSlice"/>.
    /// </summary>
    /// <param name="other">The instance to compare with.</param>
    /// <returns>true if the instances are equal; otherwise, false.</returns>
    /// <remarks>
    /// The comparison is optimized to first check reference equality and then text reference equality
    /// with range equality before performing a character-by-character comparison.
    /// </remarks>
    public bool Equals(MutableStringSlice? other) {
        if (other is null) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        // Add optimization: if same text reference and same range, they're equal
        if (ReferenceEquals(this._Text, other._Text) && this._Range.Equals(other._Range)) {
            return true;
        }

        var thisSpan = this._Text.AsSpan(this._Range);
        var otherSpan = other._Text.AsSpan(other._Range);
        
        return thisSpan.SequenceEqual(otherSpan);
    }

    /// <summary>
    /// Compares two <see cref="MutableStringSlice"/> instances for equality.
    /// </summary>
    /// <param name="x">The first instance to compare.</param>
    /// <param name="y">The second instance to compare.</param>
    /// <returns>true if the instances are equal; otherwise, false.</returns>
    public bool Equals(MutableStringSlice? x, MutableStringSlice? y) {
        if (ReferenceEquals(x, y)) {
            return true;
        }

        if (x is null || y is null) {
            return false;
        }

        return x.Equals(y);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) {
        return obj is MutableStringSlice other && Equals(other);
    }

    /// <summary>
    /// Computes a hash code for the specified <see cref="MutableStringSlice"/>.
    /// </summary>
    /// <param name="obj">The instance to compute the hash code for.</param>
    /// <returns>A hash code value for the specified instance.</returns>
    public int GetHashCode([DisallowNull] MutableStringSlice obj) {
        return string.GetHashCode(obj._Text.AsSpan(obj._Range));
    }

    /// <inheritdoc/>
    public override int GetHashCode() {
        return string.GetHashCode(this._Text.AsSpan(this._Range));
    }

    /// <summary>
    /// Returns a string that represents the current slice.
    /// </summary>
    /// <returns>The string representation of the slice.</returns>
    public override string ToString() {
        return this._Text[this._Range];
    }

    private string GetDebuggerDisplay() {
        return $"Text: {this._Text}, Range: {this._Range}, Value: {this.ToString()}";
    }

    /// <summary>
    /// Determines whether two <see cref="MutableStringSlice"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns>true if the instances are equal; otherwise, false.</returns>
    public static bool operator ==(MutableStringSlice? left, MutableStringSlice? right) {
        if (left is null) {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="MutableStringSlice"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns>true if the instances are not equal; otherwise, false.</returns>
    public static bool operator !=(MutableStringSlice? left, MutableStringSlice? right) {
        return !(left == right);
    }

    /// <summary>
    /// Explicitly converts a <see cref="StringSlice"/> to a <see cref="MutableStringSlice"/>.
    /// </summary>
    /// <param name="slice">The string slice to convert.</param>
    /// <returns>A new mutable string slice with the same text and range.</returns>
    public static explicit operator MutableStringSlice(StringSlice slice) {
        return new MutableStringSlice(slice.Text, slice.Range);
    }

    /// <summary>
    /// Explicitly converts an <see cref="ImmutableStringSlice"/> to a <see cref="MutableStringSlice"/>.
    /// </summary>
    /// <param name="slice">The immutable string slice to convert.</param>
    /// <returns>A new mutable string slice with the same text and range.</returns>
    public static explicit operator MutableStringSlice(ImmutableStringSlice slice) {
        return new MutableStringSlice(slice.Text, slice.Range);
    }

    /// <summary>
    /// Determines whether a <see cref="MutableStringSlice"/> equals an <see cref="ImmutableStringSlice"/>.
    /// </summary>
    /// <param name="left">The mutable string slice to compare.</param>
    /// <param name="right">The immutable string slice to compare.</param>
    /// <returns>true if the slices are equal; otherwise, false.</returns>
    public static bool operator ==(MutableStringSlice? left, ImmutableStringSlice? right) {
        if (left is null) {
            return right is null;
        }

        if (right is null) {
            return false;
        }

        return left.AsStringSlice().Equals(right.AsStringSlice());
    }

    /// <summary>
    /// Determines whether a <see cref="MutableStringSlice"/> does not equal an <see cref="ImmutableStringSlice"/>.
    /// </summary>
    /// <param name="left">The mutable string slice to compare.</param>
    /// <param name="right">The immutable string slice to compare.</param>
    /// <returns>true if the slices are not equal; otherwise, false.</returns>
    public static bool operator !=(MutableStringSlice? left, ImmutableStringSlice? right) {
        return !(left == right);
    }


    /// <summary>
    /// Determines whether a <see cref="MutableStringSlice"/> equals a <see cref="StringSlice"/>.
    /// </summary>
    /// <param name="left">The mutable string slice to compare.</param>
    /// <param name="right">The string slice to compare.</param>
    /// <returns>true if the slices are equal; otherwise, false.</returns>
    public static bool operator ==(MutableStringSlice? left, StringSlice right) {
        if (left is null) {
            return right.IsEmpty;
        }

        return left.AsStringSlice().Equals(right);
    }

    /// <summary>
    /// Determines whether a <see cref="MutableStringSlice"/> does not equal a <see cref="StringSlice"/>.
    /// </summary>
    /// <param name="left">The mutable string slice to compare.</param>
    /// <param name="right">The string slice to compare.</param>
    /// <returns>true if the slices are not equal; otherwise, false.</returns>
    public static bool operator !=(MutableStringSlice? left, StringSlice right) {
        return !(left == right);
    }

    /// <summary>
    /// Determines whether a <see cref="StringSlice"/> equals a <see cref="MutableStringSlice"/>.
    /// </summary>
    /// <param name="left">The string slice to compare.</param>
    /// <param name="right">The mutable string slice to compare.</param>
    /// <returns>true if the slices are equal; otherwise, false.</returns>
    public static bool operator ==(StringSlice left, MutableStringSlice? right) {
        if (right is null) {
            return false;
        }

        return left.AsSpan().SequenceEqual(right.Text.AsSpan(right.Range));
    }

    /// <summary>
    /// Determines whether a <see cref="StringSlice"/> does not equal a <see cref="MutableStringSlice"/>.
    /// </summary>
    /// <param name="left">The string slice to compare.</param>
    /// <param name="right">The mutable string slice to compare.</param>
    /// <returns>true if the slices are not equal; otherwise, false.</returns>
    public static bool operator !=(StringSlice left, MutableStringSlice? right) {
        return !(left == right);
    }

    /// <summary>
    /// Returns a read-only span over the characters in this slice.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the characters in the slice.</returns>
    public ReadOnlySpan<char> AsSpan() {
        var (offset, length) = this._Range.GetOffsetAndLength(this._Text.Length);
        return this._Text.AsSpan(offset, length);
    }
}
