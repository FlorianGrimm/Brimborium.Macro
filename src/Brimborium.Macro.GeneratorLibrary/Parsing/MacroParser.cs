using Brimborium.Macro.Model;
using Brimborium.Text;

namespace Brimborium.Macro.Parsing;

public static class MacroParser {
    public static readonly string TextSlashStar = "/*";
    public static readonly string TextStarSlash = "*/";
    public static readonly string TextMacro = "Macro";
    public static readonly string TextEndMacro = "EndMacro";

    /// <summary>
    /// Attempts to parse a multi-line comment to identify macro-related content.
    /// </summary>
    /// <param name="commentText">The text content to parse, potentially containing a macro comment.</param>
    /// <param name="macroText">When the method returns, contains the extracted macro text if found, or an empty span if not found.</param>
    /// <returns>
    /// An integer indicating the parsing result:
    /// - 0: No valid macro content found or empty macro
    /// - 1: Found a macro start marker with content
    /// - 2: Found a macro end marker
    /// </returns>
    /// <remarks>
    /// The method looks for comments in the format:
    /// - Macro start: /* Macro [content] */
    /// - Macro end: /* EndMacro [content] */
    /// 
    /// The method trims whitespace and validates the comment structure before extracting the macro content.
    /// </remarks>
    public static int TryGetMultiLineComment(StringSlice commentText, out StringSlice macroText) {
        ParserUtility.TrimLeftWhitespaceWithNewLine(ref commentText);
        ParserUtility.TrimRightWhitespaceWithNewLine(ref commentText);

        if (ParserUtility.TrimLeftText(ref commentText, MacroParser.TextSlashStar.AsSpan())) {
            ParserUtility.TrimLeftWhitespaceNotNewLine(ref commentText);

            if (ParserUtility.TrimRightText(ref commentText, MacroParser.TextStarSlash.AsSpan())) {
                ParserUtility.TrimRightWhitespaceNotNewLine(ref commentText);

                if (ParserUtility.TrimLeftText(ref commentText, MacroParser.TextMacro.AsSpan())) {
                    ParserUtility.TrimLeftWhitespaceNotNewLine(ref commentText);
                    if (commentText.IsEmpty) {
                        macroText = commentText;
                        return 0;
                    } else {
                        macroText = commentText;
                        return 1;
                    }
                }

                if (ParserUtility.TrimLeftText(ref commentText, MacroParser.TextEndMacro.AsSpan())) {
                    ParserUtility.TrimLeftWhitespaceNotNewLine(ref commentText);
                    macroText = commentText;
                    return 2;
                }
            }
        }

        macroText = new StringSlice();
        return 0;
    }

    /// <summary>
    /// Attempts to parse a region block start marker to identify macro-related content.
    /// </summary>
    /// <param name="regionText">The text content to parse, potentially containing a macro region start marker.</param>
    /// <param name="macroText">When the method returns, contains the extracted macro text if found, or an empty span if not found.</param>
    /// <returns>
    /// <c>true</c> if a valid macro region start marker was found and parsed successfully; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The method processes region markers in the format:
    /// #region Macro [content]
    /// 
    /// Processing steps:
    /// 1. Trims leading whitespace (excluding newlines)
    /// 2. Checks for and removes the "Macro" keyword
    /// 3. Trims remaining whitespace
    /// 4. Returns the remaining content if any exists
    /// </remarks>
    /// <example>
    /// Input: "  Macro MyMacro Param1 Param2  "
    /// Output: macroText = "MyMacro Param1 Param2", returns true
    /// </example>
    public static bool TryGetRegionBlockStart(StringSlice regionText, out StringSlice macroText) {
        ParserUtility.TrimLeftWhitespaceNotNewLine(ref regionText);
        if (ParserUtility.TrimLeftText(ref regionText, MacroParser.TextMacro.AsSpan())) {
            ParserUtility.TrimLeftWhitespaceNotNewLine(ref regionText);
            ParserUtility.TrimRightWhitespaceWithNewLine(ref regionText);
            if (!regionText.IsEmpty) {
                macroText = regionText;
                return true;
            }
        }
        macroText = new StringSlice();
        return false;
    }

    /// <summary>
    /// Splits a location tag from a region block end marker and extracts both the macro text and location information.
    /// </summary>
    /// <param name="regionText">The text content to parse, potentially containing a macro region end marker.</param>
    /// <param name="macroText">When the method returns, contains the extracted macro text if found, or an empty span if not found.</param>
    /// <param name="locationTag">When the method returns, contains the parsed location tag information if found, or an empty location tag if not found.</param>
    /// <returns>
    /// <c>true</c> if the parsing operation completed successfully (even if no EndMacro marker was found); otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The method processes region end markers in the format:
    /// #endregion EndMacro [content] [#LineNumber]
    /// 
    /// Processing steps:
    /// 1. Trims leading whitespace (excluding newlines)
    /// 2. Checks for and removes the "EndMacro" keyword
    /// 3. Trims remaining whitespace from both ends
    /// 4. Splits the remaining text into macro content and location tag
    /// </remarks>
    /// <example>
    /// Input: "  EndMacro MyMacro #123  "
    /// Output: 
    ///   - macroText = "MyMacro"
    ///   - locationTag = {LineNumber: 123}
    ///   - returns true
    /// </example>
    public static bool TryGetRegionBlockEnd(StringSlice regionText, out StringSlice macroText, out LocationTag locationTag) {
        ParserUtility.TrimLeftWhitespaceNotNewLine(ref regionText);
        if (ParserUtility.TrimLeftText(ref regionText, MacroParser.TextEndMacro.AsSpan())) {
            ParserUtility.TrimLeftWhitespaceNotNewLine(ref regionText);
            ParserUtility.TrimRightWhitespaceNotNewLine(ref regionText);
            MacroParser.SplitLocationTag(regionText, out macroText, out locationTag);
            return true;
        }
        macroText = new StringSlice();
        locationTag = new LocationTag();
        return true;
    }

#if false

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
    /// var span = "MacroTest".AsSpan();
    /// TrimLeftText(ref span, "Macro".AsSpan()) -> returns true, span becomes "Test"
    /// </example>
    public static bool TrimLeftText(ref ReadOnlySpan<char> text, ReadOnlySpan<char> lookingFor, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) {
        if (!text.StartsWith(lookingFor, comparisonType)) { return false; }
        text = text.Slice(lookingFor.Length);
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
    /// var span = "TestMacro".AsSpan();
    /// TrimRightText(ref span, "Macro".AsSpan()) -> returns true, span becomes "Test"
    /// </example>
    public static bool TrimRightText(ref ReadOnlySpan<char> text, ReadOnlySpan<char> lookingFor, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase) {
        if (!text.EndsWith(lookingFor, comparisonType)) { return false; }
        text = text[..^(lookingFor.Length)];
        return true;
    }

    /// <summary>
    /// Trims whitespace characters (excluding newlines) from the left side of the span.
    /// </summary>
    /// <param name="text">The span of characters to trim.</param>
    /// <returns><c>true</c> if any characters were trimmed; otherwise, <c>false</c>.</returns>
    public static bool TrimLeftWhitespaceNoNewLine(ref ReadOnlySpan<char> text) {
        bool result = false;
        while ((text.Length > 0)
                && (text[0] is not '\r' or '\n') && ((text[0] is ' ' or '\t')
                    || char.IsWhiteSpace(text[0]))) {
            text = text.Slice(1);
            result = true;
        }
        return result;
    }
    /// <summary>
    /// Trims all whitespace characters (including newlines) from the left side of the span.
    /// </summary>
    /// <param name="text">The span of characters to trim.</param>
    /// <returns><c>true</c> if any characters were trimmed; otherwise, <c>false</c>.</returns>
    public static bool TrimLeftWhitespaceWithNewLine(ref ReadOnlySpan<char> text) {
        bool result = false;
        while ((text.Length > 0)
                && ((text[0] is ' ' or '\t' or '\r' or '\n') || char.IsWhiteSpace(text[0]))) {
            text = text.Slice(1);
            result = true;
        }
        return result;
    }

    /// <summary>
    /// Trims whitespace characters (excluding newlines) from the right side of the span.
    /// </summary>
    /// <param name="text">The span of characters to trim.</param>
    /// <returns><c>true</c> if any characters were trimmed; otherwise, <c>false</c>.</returns>
    public static bool TrimRightWhitespaceNoNewLine(ref ReadOnlySpan<char> text) {
        bool result = false;
        while ((text.Length > 0)
                && (text[text.Length - 1] is not '\r' or '\n')
                && ((text[text.Length - 1] is ' ' or '\t') || (char.IsWhiteSpace(text[text.Length - 1])))
                ) {
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
    public static bool TrimRightWhitespaceWithNewLine(ref ReadOnlySpan<char> text) {
        bool result = false;
        while ((text.Length > 0)
                && ((text[text.Length - 1] is ' ' or '\t' or '\r' or '\n')
                || (char.IsWhiteSpace(text[text.Length - 1])))
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
    public static ReadOnlySpan<char> LeftUntilWhitespace(ref ReadOnlySpan<char> text) {
        int idx = 0;
        while (idx < text.Length) {
            if ((text[0] is ' ' or '\t' or '\r' or '\n')
                || char.IsWhiteSpace(text[idx])) {
                var result = text[0..idx];
                text = text.Slice(idx + 1);
                TrimLeftWhitespaceNoNewLine(ref text);
                return result;
            } else {
                idx++;
            }
        }
        {
            var result = text;
            text = text.Slice(text.Length);
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
            if (MacroParser.IsWhitespaceNotNewLine(value[index - 1])) {
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
        while (index < value.Length && MacroParser.IsWhitespaceNotNewLine(value[index])) {
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
    /// <param name="stringPrevMacroContent">The previous macro content.</param>
    /// <param name="stringNextMacroContent">The next macro content.</param>
    /// <returns><c>true</c> if the contents are equivalent; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The comparison:
    /// - Trims whitespace from the start and end of each line
    /// - Ignores empty lines
    /// - Maintains line-by-line ordering
    /// </remarks>
    public static bool EqualsLines(string stringPrevMacroContent, string stringNextMacroContent) {
        if (string.Equals(stringPrevMacroContent, stringNextMacroContent, StringComparison.Ordinal)) {
            return true;
        }
        //stringOldMacroContent.Split(_NewLines)
        var spanPrevMacroContent = stringPrevMacroContent.AsSpan();
        var spanNextMacroContent = stringNextMacroContent.AsSpan();
        MacroParser.TrimRightWhitespaceWithNewLine(ref spanPrevMacroContent);
        MacroParser.TrimRightWhitespaceWithNewLine(ref spanNextMacroContent);

        MacroParser.TrimLeftWhitespaceNoNewLine(ref spanPrevMacroContent);
        MacroParser.TrimLeftWhitespaceNoNewLine(ref spanNextMacroContent);

        while (!spanPrevMacroContent.IsEmpty && !spanNextMacroContent.IsEmpty) {
            var indexPrev = spanPrevMacroContent.IndexOfAny(_NewLines);
            var indexNext = spanNextMacroContent.IndexOfAny(_NewLines);

            ReadOnlySpan<char> prevLine;
            ReadOnlySpan<char> nextLine;

            if (0 <= indexPrev && 0 <= indexNext) {
                prevLine = spanPrevMacroContent.Slice(0, indexPrev);
                nextLine = spanNextMacroContent.Slice(0, indexNext);
            } else {
                prevLine = spanPrevMacroContent;
                nextLine = spanNextMacroContent;
            }

            var prevLineTrim = prevLine.Trim();
            var nextLineTrim = nextLine.Trim();
            if (prevLineTrim.Length != nextLineTrim.Length) {
                return false;
            }
            if (prevLineTrim.Length == 0 || nextLineTrim.Length == 0) {
                return false;
            }
            if (!prevLineTrim.SequenceEqual(nextLineTrim)) {
                return false;
            }

            spanPrevMacroContent = spanPrevMacroContent.Slice(prevLine.Length);
            spanNextMacroContent = spanNextMacroContent.Slice(nextLine.Length);

            var a = MacroParser.TrimLeftWhitespaceWithNewLine(ref spanPrevMacroContent);
            var b = MacroParser.TrimLeftWhitespaceWithNewLine(ref spanNextMacroContent);
            if (a == b) {
                continue;
            } else {
                return false;
            }
        }

        return spanPrevMacroContent.IsEmpty && spanNextMacroContent.IsEmpty;
    }

#endif

    /// <summary>
    /// Splits a string into macro text and location tag components.
    /// </summary>
    /// <param name="regionText">The text to split.</param>
    /// <param name="macroText">When the method returns, contains the macro text portion.</param>
    /// <param name="locationTag">When the method returns, contains the parsed location tag.</param>
    /// <returns><c>true</c> if a location tag was found and parsed; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// The location tag is identified by the last '#' character in the text.
    /// </remarks>
    public static bool SplitLocationTag(StringSlice regionText, out StringSlice macroText, out LocationTag locationTag) {
        // split the regionText into macroText and locationTag - the separator is the last # character.
        int index = regionText.AsSpan().LastIndexOf('#');
        if (0 <= index) {
            macroText = regionText.Substring(0, index);
            var locationText = regionText.Substring(index + 1);
            locationTag = ParseLocationTag(locationText);
            return true;
        }
        // not found
        macroText = regionText;
        locationTag = new LocationTag();
        return false;
    }

    /// <summary>
    /// Parses a location tag from the given text.
    /// </summary>
    /// <param name="locationText">The text to parse as a location tag.</param>
    /// <returns>A <see cref="LocationTag"/> instance containing the parsed information.</returns>
    /// <remarks>
    /// Returns a LocationTag with line number 0 if parsing fails.
    /// </remarks>
    public static LocationTag ParseLocationTag(StringSlice locationText) {
        if (int.TryParse(locationText.AsSpan(), out int line)) {
            return new LocationTag(null, line);
        } else {
            return new LocationTag(null, 0);
        }
    }
}
