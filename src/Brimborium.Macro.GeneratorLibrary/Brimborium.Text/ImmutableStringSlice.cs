
namespace Brimborium.Text;

/// <summary>
/// Represents an immutable slice of a string, storing the original string and a range.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class ImmutableStringSlice : IEqualityComparer<ImmutableStringSlice> {
    private readonly string _Text;
    private readonly Range _Range;

    /// <summary>
    /// Initializes a new instance of the ImmutableStringSlice class.
    /// </summary>
    /// <param name="text">The source string to slice. Cannot be null.</param>
    /// <param name="range">The range within the source string. Must be valid for the string length.</param>
    /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when range is invalid for the given text.</exception>
    public ImmutableStringSlice(
        string text,
        Range range) {
        if (text == null) {
            throw new ArgumentNullException(nameof(text));
        }
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
    /// Gets the source text.
    /// </summary>
    public string Text { get => this._Text; }

    /// <summary>
    /// Gets the range within the source text.
    /// </summary>
    public Range Range { get => this._Range; }

    /// <summary>
    /// Gets the length of the slice.
    /// </summary>
    /// <remarks>
    /// The length is calculated based on the range within the source text.
    /// This property returns the number of characters in the slice, not the length of the underlying text.
    /// </remarks>
    /// <example>
    /// <code>
    /// var slice = new ImmutableStringSlice("Hello World", 0..5);
    /// Console.WriteLine(slice.Length); // Outputs: 5
    /// </code>
    /// </example>
    /// <returns>The number of characters in the slice.</returns>
    public int Length {
        get {
            var (_, length) = this._Range.GetOffsetAndLength(this._Text.Length);
            return length;
        }
    }

    /// <summary>
    /// Deconstructs the ImmutableStringSlice into its text and range components.
    /// </summary>
    /// <param name="text">The source text.</param>
    /// <param name="range">The range within the source text.</param>
    public void Deconstruct(out string text, out Range range) {
        text = this.Text;
        range = this.Range;
    }

    /// <summary>
    /// Compares two ImmutableStringSlice instances for equality.
    /// </summary>
    /// <param name="x">The first ImmutableStringSlice to compare.</param>
    /// <param name="y">The second ImmutableStringSlice to compare.</param>
    /// <returns>True if the slices represent the same text content; otherwise, false.</returns>
    public bool Equals(ImmutableStringSlice? x, ImmutableStringSlice? y) {
        if (ReferenceEquals(x, y)) {
            return true;
        }

        if (x is null || y is null) {
            return false;
        }

        // If they reference the same text and have the same range, they're equal
        if (ReferenceEquals(x.Text, y.Text) && x.Range.Equals(y.Range)) {
            return true;
        }

        // Compare the actual string content
        return x.ToString().Equals(y.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets a hash code for the specified ImmutableStringSlice.
    /// </summary>
    /// <param name="obj">The ImmutableStringSlice for which to get a hash code.</param>
    /// <returns>A hash code for the specified object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when obj is null.</exception>
    public int GetHashCode([DisallowNull] ImmutableStringSlice obj) {
        if (obj is null) {
            throw new ArgumentNullException(nameof(obj));
        }

        return obj.ToString().GetHashCode();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current ImmutableStringSlice.
    /// </summary>
    /// <param name="obj">The object to compare with the current ImmutableStringSlice.</param>
    /// <returns>True if the specified object represents the same content as the current ImmutableStringSlice; otherwise, false.</returns>
    public override bool Equals(object? obj) {
        if (obj is ImmutableStringSlice other) {
            return this.Equals(this, other);
        }
        return false;
    }

    /// <summary>
    /// Gets a hash code for the current ImmutableStringSlice.
    /// </summary>
    /// <returns>A hash code value for the current ImmutableStringSlice.</returns>
    public override int GetHashCode() {
        return this.GetHashCode(this);
    }

    private string? _ToString = null;

    /// <summary>
    /// Returns a string that represents the current ImmutableStringSlice.
    /// The result is cached for subsequent calls.
    /// </summary>
    /// <returns>A string representing the slice of the source text.</returns>
    public override string ToString() {
        return this._ToString ??= this.Text[this.Range];
    }

    private string GetDebuggerDisplay() {
        return $"Range:{this.Range}; ToString:{this.ToString()}";
    }

    /// <summary>
    /// Returns a read-only span over the characters in this slice.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the characters in the slice.</returns>
    public ReadOnlySpan<char> AsSpan() {
        var (offset, length) = this._Range.GetOffsetAndLength(this._Text.Length);
        return this._Text.AsSpan(offset, length);
    }

    /// <summary>
    /// Explicitly converts a <see cref="MutableStringSlice"/> to an <see cref="ImmutableStringSlice"/>.
    /// </summary>
    /// <param name="slice">The mutable string slice to convert.</param>
    /// <returns>A new immutable string slice with the same text and range.</returns>
    public static explicit operator ImmutableStringSlice(StringSlice slice) {
        return new ImmutableStringSlice(slice.Text, slice.Range);
    }

    /// <summary>
    /// Explicitly converts a <see cref="MutableStringSlice"/> to an <see cref="ImmutableStringSlice"/>.
    /// </summary>
    /// <param name="slice">The mutable string slice to convert.</param>
    /// <returns>A new immutable string slice with the same text and range.</returns>
    public static explicit operator ImmutableStringSlice(MutableStringSlice slice) {
        return new ImmutableStringSlice(slice.Text, slice.Range);
    }

    /// <summary>
    /// Creates a <see cref="StringSlice"/> from this immutable slice.
    /// </summary>
    /// <remarks>
    /// This method creates a new <see cref="StringSlice"/> that references the same underlying text
    /// and uses the same range as this immutable slice. The resulting <see cref="StringSlice"/> 
    /// is a value type that provides the same view of the text but with different performance characteristics.
    /// </remarks>
    /// <example>
    /// <code>
    /// var immutableSlice = new ImmutableStringSlice("Hello World", 0..5);
    /// var stringSlice = immutableSlice.AsStringSlice();
    /// Console.WriteLine(stringSlice.ToString()); // Outputs: "Hello"
    /// </code>
    /// </example>
    /// <returns>A new <see cref="StringSlice"/> with the same text and range as this immutable slice.</returns>
    public StringSlice AsStringSlice() => new StringSlice(this.Text, this.Range);

    /// <summary>
    /// Creates a <see cref="MutableStringSlice"/> from this immutable slice.
    /// </summary>
    /// <remarks>
    /// This method creates a new <see cref="MutableStringSlice"/> that references the same underlying text
    /// and uses the same range as this immutable slice. The resulting <see cref="MutableStringSlice"/> 
    /// is a reference type that provides the same view of the text but allows modification of the range.
    /// </remarks>
    /// <example>
    /// <code>
    /// var immutableSlice = new ImmutableStringSlice("Hello World", 0..5);
    /// var mutableSlice = immutableSlice.AsMutableStringSlice();
    /// Console.WriteLine(mutableSlice.ToString()); // Outputs: "Hello"
    /// </code>
    /// </example>
    /// <returns>A new <see cref="MutableStringSlice"/> with the same text and range as this immutable slice.</returns>
    public MutableStringSlice AsMutableStringSlice() => new MutableStringSlice(this.Text, this.Range);
}
