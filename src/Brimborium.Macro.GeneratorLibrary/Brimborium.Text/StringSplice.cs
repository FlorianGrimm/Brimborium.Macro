namespace Brimborium.Text;

/// <summary>
/// Represents a mutable slice of a string that allows for text replacement operations.
/// StringSplice enables replacing parts of a string without creating intermediate string copies,
/// making it memory efficient for string manipulation operations.
/// </summary>
/// <remarks>
/// The class supports three different ways to specify replacements:
/// 1. Direct text replacement using <see cref="SetReplacementText"/>
/// 2. StringBuilder-based replacement using <see cref="GetReplacementBuilder"/>
/// 3. Nested parts using <see cref="CreatePart"/>
/// Only one of these methods can be used for a given splice instance.
/// </remarks>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class StringSplice {
    private readonly StringSlice _Text;
    private readonly Range _Range;
    private List<StringSplice>? _ListPart;
    private StringBuilder? _ReplacementBuilder;
    private string? _ReplacementText;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSplice"/> class with the specified text.
    /// </summary>
    /// <param name="text">The source text to create the splice from.</param>
    public StringSplice(string text) {
        this._Text = new StringSlice(text);
        this._Range = new Range(0, text.Length);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSplice"/> class with the specified <see cref="StringSlice"/>.
    /// </summary>
    /// <param name="text">The source text slice to create the splice from.</param>
    public StringSplice(StringSlice text) {
        this._Text = text;
        this._Range = new Range(0, text.Length);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSplice"/> class with the specified text slice and range parameters.
    /// </summary>
    /// <param name="text">The source text slice.</param>
    /// <param name="start">The starting position of the range.</param>
    /// <param name="length">The length of the range.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="start"/> is negative or greater than the text length,
    /// or when <paramref name="length"/> would result in a range extending beyond the text length.
    /// </exception>
    public StringSplice(
        StringSlice text,
        int start,
        int length) {
        this._Text = text;
        var range = new Range(start, start + length);
        if (range.Start.Value < 0 || range.Start.Value > text.Length) { throw new ArgumentOutOfRangeException(nameof(start)); }
        if (range.End.Value < 0 || range.End.Value > text.Length) { throw new ArgumentOutOfRangeException(nameof(length)); }
        this._Range = range;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringSplice"/> class with the specified text slice and range.
    /// </summary>
    /// <param name="text">The source text slice.</param>
    /// <param name="range">The range within the text slice.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the range is invalid or extends beyond the text boundaries.
    /// </exception>
    public StringSplice(
        StringSlice text,
        Range range) {
        this._Text = text;
        if (range.Start.IsFromEnd || range.End.IsFromEnd) {
            var (rangeOffset, rangeLength) = range.GetOffsetAndLength(text.Length);
            var rangeEnd = rangeOffset + rangeLength;
            range = new Range(rangeOffset, rangeEnd);
        }
        if (range.Start.Value < 0 || range.Start.Value > text.Length) { throw new ArgumentOutOfRangeException(nameof(range)); }
        if (range.End.Value < 0 || range.End.Value > text.Length) { throw new ArgumentOutOfRangeException(nameof(range)); }
        if (range.End.Value < range.Start.Value) { throw new ArgumentOutOfRangeException(nameof(range)); }
        this._Range = range;
    }

    /// <summary>
    /// Gets the current splice as a substring.
    /// </summary>
    /// <returns>A <see cref="StringSlice"/> representing the current splice's text.</returns>
    public StringSlice AsSubString() => this._Text.Substring(this._Range);

    /// <summary>
    /// Gets the text content of the current splice.
    /// </summary>
    /// <returns>The string representation of the current splice.</returns>
    public string GetText() => this.AsSubString().ToString();

    /// <summary>
    /// Gets the range of the current splice within the original text.
    /// </summary>
    public Range Range => _Range;

    /// <summary>
    /// Gets the length of the current splice.
    /// </summary>
    public int Length {
        get {
            var (_, length) = this.Range.GetOffsetAndLength(this._Text.Length);
            return length;
        }
    }

    /// <summary>
    /// Gets the replacement text if one has been set.
    /// </summary>
    /// <returns>The replacement text, or null if none has been set.</returns>
    public string? GetReplacementText() { return this._ReplacementText; }

    /// <summary>
    /// Sets the replacement text for this splice.
    /// </summary>
    /// <param name="value">The text to use as replacement.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a StringBuilder or Parts replacement method is already in use.
    /// </exception>
    public void SetReplacementText(string? value) {
        if (this._ReplacementBuilder is not null) {
            throw new InvalidOperationException("Use only one of ReplacmentText and ReplacmentBuilder.");
        }
        if (this._ListPart is not null) {
            throw new InvalidOperationException("Use only one of ReplacmentText and Parts.");
        }
        this._ReplacementText = value;
    }

    /// <summary>
    /// Sets a StringBuilder for replacement text.
    /// </summary>
    /// <param name="value">The StringBuilder to use for replacement.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a direct text replacement or Parts replacement method is already in use.
    /// </exception>
    public void SetReplacementBuilder(StringBuilder? value) {
        if (this._ReplacementText is not null) {
            throw new InvalidOperationException("Use only one of ReplacmentText and ReplacmentBuilder.");
        }
        if (this._ListPart is not null) {
            throw new InvalidOperationException("Use only one of ReplacmentBuilder and Parts.");
        }

        this._ReplacementBuilder = value;
    }

    /// <summary>
    /// Gets or creates a StringBuilder for replacement text.
    /// </summary>
    /// <returns>A StringBuilder instance for building replacement text.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a direct text replacement or Parts replacement method is already in use.
    /// </exception>
    public StringBuilder GetReplacementBuilder() {
        if (this._ReplacementText is not null) {
            throw new InvalidOperationException("Use only one of ReplacmentText and ReplacmentBuilder.");
        }
        if (this._ListPart is not null) {
            throw new InvalidOperationException("Use only one of ReplacmentBuilder and Parts.");
        }

        return this._ReplacementBuilder ??= StringBuilderPool.GetStringBuilder();
    }

    /// <summary>
    /// Gets an array of all parts created for this splice.
    /// </summary>
    /// <returns>An array of StringSplice parts, or null if no parts exist.</returns>
    public StringSplice[]? GetArrayPart() => this._ListPart?.ToArray();

    /// <summary>
    /// Validates whether a given range is valid within the current splice.
    /// </summary>
    /// <param name="start">The starting position of the range.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>true if the range is valid; otherwise, false.</returns>
    public bool IsRangeValid(
         int start,
         int length
    ) {
        if (start < 0) { return false; }
        if (length < 0) { return false; }
        if (this.Length < (start + length)) { return false; }
        return true;
    }

    /// <summary>
    /// Creates a new part within the current splice at the specified position.
    /// </summary>
    /// <param name="start">The starting position of the new part.</param>
    /// <param name="length">The length of the new part.</param>
    /// <returns>
    /// A new StringSplice instance representing the created part,
    /// or null if the range is invalid or overlaps with existing parts.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a direct text replacement or StringBuilder replacement method is already in use.
    /// </exception>
    public StringSplice? CreatePart(int start, int length) {
        if (!this.IsRangeValid(start, length)) {
            return null;
        }
        if (this._ListPart is null) {
            if (this._ReplacementText is not null) {
                throw new InvalidOperationException("Use only one of ReplacmentText and Parts.");
            }
            if (this._ReplacementBuilder is not null) {
                throw new InvalidOperationException("Use only one of ReplacmentBuilder and Parts.");
            }
            this._ListPart = new List<StringSplice>();
        }

        for (int idx = 0; idx < this._ListPart.Count; idx++) {
            var item = this._ListPart[idx];
            if (item.Range.Start.Value < start) {
                if (item.Range.End.Value > start) {
                    return null;
                } else {
                    continue;
                }
            }
            if (item.Range.Start.Value == start) {
                // special case for length==0 add behind the with this start.
                if (length == 0) {
                    while ((idx + 1) < this._ListPart.Count) {
                        if (item.Range.Start.Value == start) {
                            idx++;
                            continue;
                        } else {
                            break;
                        }
                    }
                    {
                        var result = this.Factory(start, length);
                        if (result is not null) {
                            this._ListPart.Insert(idx + 1, result);
                        }
                        return result;
                    }
                }
                return null;
            }
            {
                // within the span?
                if (item.Range.Start.Value < (start + length)) {
                    return null;
                }
                var result = this.Factory(start, length);
                if (result is not null) {
                    this._ListPart.Insert(idx, result);
                }
                return result;
            }
        }
        {
            var result = this.Factory(start, length);
            if (result is not null) {
                this._ListPart.Add(result);
            }
            return result;
        }
    }

    /// <summary>
    /// Creates a new part within this splice at the specified range.
    /// </summary>
    /// <param name="range">The range within the current splice where the new part should be created.</param>
    /// <returns>A new <see cref="StringSplice"/> representing the specified part, or null if the range is invalid.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a direct text replacement or StringBuilder replacement method is already in use.
    /// </exception>
    public StringSplice? CreatePart(Range range) {
        var (offset, length) = range.GetOffsetAndLength(this._Text.Length);
        return this.CreatePart(offset, length);
    }

    /// <summary>
    /// Gets an existing part at the specified position or creates a new one if none exists.
    /// </summary>
    /// <param name="start">The starting position of the part.</param>
    /// <param name="length">The length of the part.</param>
    /// <returns>
    /// A StringSplice instance representing the part,
    /// or null if the range is invalid.
    /// </returns>
    public StringSplice? GetOrCreatePart(int start, int length) {
        if (!this.IsRangeValid(start, length)) { return null; }

        if (this._ListPart is null) {
            if (this._ReplacementText is not null) {
                throw new InvalidOperationException("Use only one of ReplacmentText and Parts.");
            }
            if (this._ReplacementBuilder is not null) {
                throw new InvalidOperationException("Use only one of ReplacmentBuilder and Parts.");
            }
            this._ListPart = new List<StringSplice>();
        }

        for (int idx = 0; idx < this._ListPart.Count; idx++) {
            var item = this._ListPart[idx];
            if (item.Range.Start.Value < start) {
                continue;
            }
            if (item.Range.Start.Value == start) {
                if (item.Length == length) {
                    return item;
                }
                return null;
            }
            {
                if (item.Range.Start.Value < (start + length)) {
                    return null;
                }
                var result = this.Factory(start, length);
                if (result is not null) {
                    this._ListPart.Insert(idx, result);
                }
                return result;
            }
        }
        {
            var result = this.Factory(start, length);
            if (result is not null) {
                this._ListPart.Add(result);
            }
            return result;
        }
    }

    public IEnumerable<StringSplice> GetLstPartInRange(int start, int length) {
        if (!this.IsRangeValid(start, length)) {
            yield break;
        } else if (this._ListPart is null) {
            yield break;
        } else {
            var end = start + length;
            for (int idx = 0; idx < this._ListPart.Count; idx++) {
                var item = this._ListPart[idx];
                if (start <= item.Range.Start.Value && item.Range.Start.Value < end) {
                    yield return item;
                    continue;
                }
                if (start < item.Range.End.Value && item.Range.End.Value <= end) {
                    yield return item;
                    continue;
                }
                if (item.Range.End.Value < start) {
                    yield break;
                }
            }
        }
    }

    public string BuildReplacement() {
        if (this._ListPart is not null && this._ListPart.Count > 0) {
            var result = StringBuilderPool.GetStringBuilder();
            this.BuildReplacementStringBuilder(result);
            var resultValue = result.ToString();
            return resultValue;
        } else {
            return this.GetText();
        }
    }

    public void BuildReplacementStringBuilder(StringBuilder result) {
        if (this._ListPart is null) {
        } else {
            int posEnd = 0;
            for (int idx = 0; idx < this._ListPart.Count; idx++) {
                var item = this._ListPart[idx];
                if (posEnd < item.Range.Start.Value) {
                    var span = this._Text.Substring(this.Range)
                        .AsSpan()[new Range(posEnd, item.Range.Start.Value)];
                    result.Append(span);
                }

                if (item._ReplacementText is not null) {
                    result.Append(item._ReplacementText!);
                } else if (item._ReplacementBuilder is not null) {
                    result.Append(item._ReplacementBuilder!);
                } else if (item._ListPart is not null) {
                    item.BuildReplacementStringBuilder(result);
                }

                posEnd = item.Range.End.Value;
            }

            // add the tail
            if (posEnd < this.Length) {
                var span = this._Text.Substring(this.Range).AsSpan();
                if (posEnd == 0) {
                    result.Append(span);
                } else {
                    span = span[posEnd..^0];
                    result.Append(span);
                }
            }
        }
    }

    /// <summary>
    /// Returns a string representation of the splice, including any replacements or modifications.
    /// </summary>
    /// <returns>
    /// The final string with all replacements applied. If no replacements are present, 
    /// returns the original substring.
    /// </returns>
    public override string ToString() {
        return this.BuildReplacement();
    }

    // if you want to add any custom data
    protected virtual StringSplice Factory(int start, int length) {
        return new StringSplice(this.AsSubString(), start, length);
    }

    private string GetDebuggerDisplay() {
        var span = this.AsSubString().AsSpan();
        if (span.Length > 32) { span = span[..32]; }
        return $"{span}; #Part:{this._ListPart?.Count};";
    }
}
