namespace Brimborium.Text;

public static class ParserUtility {
    /// <summary>
    /// Attempts to trim a specified text sequence from the beginning of a span.
    /// </summary>
    /// <param name="text">The span to process. If trimming succeeds, contains the remaining text after removing the target sequence.</param>
    /// <param name="lookingFor">The text sequence to look for and remove from the start of the span.</param>
    /// <param name="comparisonType">The type of comparison to use. Defaults to Ordinal case-insensitive comparison.</param>
    /// <returns>
    /// <c>true</c> if the text sequence was found and trimmed; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// If the sequence is found, it is removed from the start of the span,
    /// and the remaining text is returned in the <paramref name="text"/> parameter.
    /// </remarks>
    /// <example>
    /// var span = new StringSlice("MacroTest");
    /// TrimLeftText(ref span, "Macro".AsSpan()) -> returns true, span becomes "Test"
    /// </example>
    public static bool TrimLeftText(ref StringSlice text, ReadOnlySpan<char> lookingFor, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) {
        if (!text.StartsWith(lookingFor, comparisonType)) { return false; }
        text = text.Substring(lookingFor.Length);
        return true;
    }

    /// <summary>
    /// Attempts to trim a specified text sequence from the end of a span.
    /// </summary>
    /// <param name="text">The span to process. If trimming succeeds, contains the remaining text after removing the target sequence.</param>
    /// <param name="lookingFor">The text sequence to look for and remove from the end of the span.</param>
    /// <param name="comparisonType">The type of string comparison to use. Defaults to Ordinal case-insensitive comparison.</param>
    /// <returns>
    /// <c>true</c> if the text sequence was found and trimmed from the end; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// If the sequence is found at the end of the span, it is removed,
    /// and the remaining text is returned in the <paramref name="text"/> parameter.
    /// </remarks>
    /// <example>
    /// var span = new StringSlice("TestMacro");
    /// TrimRightText(ref span, "Macro".AsSpan()) -> returns true, span becomes "Test"
    /// </example>
    public static bool TrimRightText(ref StringSlice text, ReadOnlySpan<char> lookingFor, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) {
        if (!text.EndsWith(lookingFor, comparisonType)) { return false; }
        text = text[..^(lookingFor.Length)];
        return true;
    }

    /// <summary>
    /// Trims whitespace characters (excluding newlines) from the left side of the span.
    /// </summary>
    /// <param name="text">The span of characters to trim.</param>
    /// <returns><c>true</c> if any characters were trimmed; otherwise, <c>false</c>.</returns>
    public static bool TrimLeftWhitespaceNotNewLine(ref StringSlice text) {
        bool result = false;
        while ((text.Length > 0)
                && IsWhitespaceNotNewLine(text[0])
                ) {
            text = text[1..];
            result = true;
        }
        return result;
    }

    /// <summary>
    /// Trims all whitespace characters (including newlines) from the left side of the span.
    /// </summary>
    /// <param name="text">The span of characters to trim.</param>
    /// <returns><c>true</c> if any characters were trimmed; otherwise, <c>false</c>.</returns>
    public static bool TrimLeftWhitespaceWithNewLine(ref StringSlice text) {
        bool result = false;
        while ((text.Length > 0)
                && IsWhitespaceWithNewLine(text[0])) {
            text = text[1..];
            result = true;
        }
        return result;
    }

    /// <summary>
    /// Trims whitespace characters (excluding newlines) from the right side of the span.
    /// </summary>
    /// <param name="text">The span of characters to trim.</param>
    /// <returns><c>true</c> if any characters were trimmed; otherwise, <c>false</c>.</returns>
    public static bool TrimRightWhitespaceNotNewLine(ref StringSlice text) {
        bool result = false;
        while ((text.Length > 0)
            && (IsWhitespaceNotNewLine(text[text.Length - 1]))) {
            text = text[0..^1];
            result = true;
        }
        return result;
    }

    /// <summary>
    /// Trims all whitespace characters (including newlines) from the right side of the span.
    /// </summary>
    /// <param name="text">The span of characters to trim.</param>
    /// <returns><c>true</c> if any characters were trimmed; otherwise, <c>false</c>.</returns>
    public static bool TrimRightWhitespaceWithNewLine(ref StringSlice text) {
        bool result = false;
        while ((text.Length > 0)
                && (IsWhitespaceWithNewLine(text[text.Length - 1]))
                ) {
            text = text[0..^1];
            result = true;
        }
        return result;
    }

    /// <summary>
    /// Extracts the leftmost sequence of non-whitespace characters from the span.
    /// </summary>
    /// <param name="text">The span to process. After the operation, contains the remaining text after the extracted portion.</param>
    /// <returns>A span containing the extracted non-whitespace characters.</returns>
    /// <remarks>
    /// After extraction, the remaining text is trimmed of leading whitespace (excluding newlines).
    /// </remarks>
    public static StringSlice LeftUntilWhitespace(ref StringSlice text) {
        int idx = 0;
        while (idx < text.Length) {
            if (IsWhitespaceWithNewLine(text[0])) {
                var result = text[0..idx];
                text = text.Substring(idx + 1);
                TrimLeftWhitespaceNotNewLine(ref text);
                return result;
            } else {
                idx++;
            }
        }
        {
            var result = text;
            text = text.Substring(text.Length);
            return result;
        }
    }

    /// <summary>
    /// Determines if a character is whitespace but not a newline character.
    /// </summary>
    /// <param name="value">The character to check.</param>
    /// <returns><c>true</c> if the character is whitespace but not a newline; otherwise, <c>false</c>.</returns>
    public static bool IsWhitespaceNotNewLine(char value)
        => (value) switch {
            ' ' => true,
            '\t' => true,
            '\r' => false,
            '\n' => false,
            _ => char.IsWhiteSpace(value)
        };

    /// <summary>
    /// Determines if a character is any type of whitespace, including newline characters.
    /// </summary>
    /// <param name="value">The character to check.</param>
    /// <returns>
    /// <c>true</c> if the character is whitespace (including space, tab, carriage return, line feed);
    /// otherwise, <c>false</c> for non-whitespace characters.
    /// </returns>
    public static bool IsWhitespaceWithNewLine(char value)
        => (value) switch {
            ' ' => true,
            '\t' => true,
            '\r' => true,
            '\n' => true,
            _ => char.IsWhiteSpace(value)
        };

    /// <summary>
    /// Determines if a newline is needed between two strings.
    /// </summary>
    /// <param name="stringBefore">The string before the potential newline.</param>
    /// <param name="stringAfter">The string after the potential newline.</param>
    /// <returns><c>true</c> if a newline is needed; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// A newline is needed if:
    /// - Both strings are non-empty
    /// - Neither string ends/starts with a newline character
    /// </remarks>
    public static bool NeedNewLine(string stringBefore, string stringAfter) {
        if (string.IsNullOrEmpty(stringBefore)) { return false; }
        if (string.IsNullOrEmpty(stringAfter)) { return false; }
        var charBeforeLast = stringBefore[stringBefore.Length - 1];
        var charAfterFirst = stringAfter[0];
        if (charBeforeLast is '\r' or '\n') { return false; }
        if (charAfterFirst is '\r' or '\n') { return false; }
        return true;
    }

    /// <summary>
    /// Moves the index left while encountering whitespace characters (excluding newlines).
    /// </summary>
    /// <param name="value">The string to traverse.</param>
    /// <param name="index">The starting index.</param>
    /// <returns>The new index position after moving left through whitespace.</returns>
    public static int GotoLeftWhileWhitespaceNotNewLine(string value, int index) {
        while (0 < index) {
            if (IsWhitespaceNotNewLine(value[index - 1])) {
                index--;
            } else {
                break;
            }
        }
        return index;
    }

    /// <summary>
    /// Moves the index left if newline characters are encountered.
    /// </summary>
    /// <param name="value">The string to traverse.</param>
    /// <param name="index">The starting index.</param>
    /// <returns>The new index position after moving past any newline characters.</returns>
    /// <remarks>
    /// Handles both '\r\n' and '\n' line endings.
    /// </remarks>
    public static int GotoLeftIfNewline(string value, int index) {
        if (0 < index && value[index - 1] == '\n') {
            index--;
        }
        if (0 < index && value[index - 1] == '\r') {
            index--;
        }
        return index;
    }

    /// <summary>
    /// Moves the index right while encountering whitespace characters (excluding newlines).
    /// </summary>
    /// <param name="value">The string to traverse.</param>
    /// <param name="index">The starting index.</param>
    /// <returns>The new index position after moving right through whitespace.</returns>
    public static int GotoRightWhileWhitespaceNotNewLine(string value, int index) {
        while (index < value.Length && IsWhitespaceNotNewLine(value[index])) {
            index++;
        }
        return index;
    }

    /// <summary>
    /// Moves the index right if newline characters are encountered.
    /// </summary>
    /// <param name="value">The string to traverse.</param>
    /// <param name="index">The starting index.</param>
    /// <returns>The new index position after moving past any newline characters.</returns>
    /// <remarks>
    /// Handles both '\r\n' and '\n' line endings.
    /// </remarks>
    public static int GotoRightIfNewline(string value, int index) {
        if (index < value.Length && value[index] == '\r') {
            index++;
        }
        if (index < value.Length && value[index] == '\n') {
            index++;
        }
        return index;
    }

    private static readonly char[] _NewLines = "\r\n".ToCharArray();
    private static readonly char[] _Whitespaces = "\r\n\t ".ToCharArray();

    /// <summary>
    /// Compares two strings line by line, ignoring whitespace differences.
    /// </summary>
    /// <param name="leftContent">The previous content.</param>
    /// <param name="rightContent">The next content.</param>
    /// <returns><c>true</c> if the contents are equivalent; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The comparison:
    /// - Trims whitespace from the start and end of each line
    /// - Ignores empty lines
    /// - Maintains line-by-line ordering
    /// </remarks>
    public static bool EqualsLines(StringSlice leftContent, StringSlice rightContent) {
        if (StringSlice.Equals(leftContent, rightContent, StringComparison.Ordinal)) {
            return true;
        }
        //stringOldMacroContent.Split(_NewLines)
        // var spanPrevMacroContent = prevContent.AsSpan();
        // var spanNextMacroContent = nextContent.AsSpan();
        TrimRightWhitespaceWithNewLine(ref leftContent);
        TrimRightWhitespaceWithNewLine(ref rightContent);

        TrimLeftWhitespaceNotNewLine(ref leftContent);
        TrimLeftWhitespaceNotNewLine(ref rightContent);

        while (!leftContent.IsEmpty && !rightContent.IsEmpty) {
            var indexPrev = leftContent.IndexOfAny(_NewLines);
            var indexNext = rightContent.IndexOfAny(_NewLines);

            StringSlice leftLine;
            StringSlice rightLine;

            if (0 <= indexPrev && 0 <= indexNext) {
                leftLine = leftContent.Substring(0, indexPrev);
                rightLine = rightContent.Substring(0, indexNext);
            } else {
                leftLine = leftContent;
                rightLine = rightContent;
            }

            var prevLineTrim = leftLine.Trim();
            var nextLineTrim = rightLine.Trim();
            if (prevLineTrim.Length != nextLineTrim.Length) {
                return false;
            }
            if (prevLineTrim.Length == 0 || nextLineTrim.Length == 0) {
                return false;
            }
            
            if (!StringSlice.Equals(prevLineTrim, nextLineTrim, StringComparison.Ordinal)) {
                return false;
            }

            leftContent = leftContent.Substring(leftLine.Length);
            rightContent = rightContent.Substring(rightLine.Length);

            var a = TrimLeftWhitespaceWithNewLine(ref leftContent);
            var b = TrimLeftWhitespaceWithNewLine(ref rightContent);
            if (a == b) {
                continue;
            } else {
                return false;
            }
        }

        return leftContent.IsEmpty && rightContent.IsEmpty;
    }
}

