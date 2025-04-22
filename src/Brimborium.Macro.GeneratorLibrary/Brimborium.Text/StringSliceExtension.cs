namespace Brimborium.Text;

public static class StringSliceExtension {

    public static StringSlice AsStringSlice(this string value)
        => new StringSlice(value);

    public static StringSlice AsStringSlice(this string value, int pos)
        => new StringSlice(value, new Range(pos, value.Length));

    public static StringSlice AsStringSlice(this string value, int pos, int length)
        => new StringSlice(value, new Range(pos, pos + length));

    public static StringSlice AsStringSlice(this string value, Range range)
        => new StringSlice(value, range);

    public static StringBuilder Append(this StringBuilder stringBuilder, StringSlice value)
        => stringBuilder.Append(value.AsSpan());


    public static bool ReadWordIfMatches(this string word, ref StringSlice slice, StringComparison comparisonType = StringComparison.Ordinal) {
        if (slice.StartsWith(word, comparisonType)) {
            slice = slice.Substring(word.Length);
            return true;
        } else {
            return false;
        }
    }

    public static bool ReadWordIfMatches(this string word, ref StringSlice slice, ref int count, StringComparison comparisonType = StringComparison.Ordinal) {
        if (slice.StartsWith(word, comparisonType)) {
            slice = slice.Substring(word.Length);
            count += word.Length;
            return true;
        } else {
            return false;
        }
    }


    public static bool ReadWhileMatches(
        this char[] matches,
        ref StringSlice slice
        ) {
        var old = slice.Range.Start.Value;
        slice = slice.TrimStart(matches);
        return old != slice.Range.Start.Value;
    }

    public static bool ReadWhileMatches(
        this char[] matches,
        ref StringSlice slice,
        int max,
        out StringSlice result
        ) {
        var startSlice = slice;
        var current = 0;
        var length = slice.Length;
        for (; 0 < max && current < length; current++, max--) {
            bool found = false;
            foreach (var m in matches) {
                if (slice[current] == m) {
                    found = true;
                    break;
                }
            }
            if (!found) {
                result = slice.Substring(0, current);
                slice = slice.Substring(current);
                return startSlice != slice;
            }
        }
        {
            result = slice.Substring(0, current);
            slice = slice.Substring(current);
            return startSlice != slice;
        }
    }
    public static bool ReadWhileNotMatches(
        this char[] matches,
        ref StringSlice slice,
        int max,
        out StringSlice result
        ) {
        var startSlice = slice;
        var oldStart = slice.Range.Start.Value;
        var end = slice.Range.End.Value;
        var current = 0;
        var length = slice.Length;
        for (; 0 < max && current < length; current++, max--) {
            bool found = false;
            foreach (var m in matches) {
                if (slice[current] == m) {
                    found = true;
                    break;
                }
            }
            if (found) {
                result = slice.Substring(0, current);
                slice = slice.Substring(current);
                return oldStart != slice.Range.Start.Value;
            }
        }
        {
            result = slice.Substring(0, current);
            slice = slice.Substring(current);
            return oldStart != slice.Range.Start.Value;
        }
    }
}
