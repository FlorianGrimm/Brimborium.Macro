#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0057
#pragma warning disable IDE1006 // Naming Styles

using Brimborium.Macro.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Brimborium.Macro.Parse;
    public static class MacroParser {
    public static readonly string TextSlashStar = "/*";
    public static readonly string TextStarSlash = "*/";
    public static readonly string TextMacro = "Macro";
    public static readonly string TextEndMacro = "EndMacro";

    public static int TryGetMultiLineComment(ReadOnlySpan<char> commentText, out ReadOnlySpan<char> macroText) {
        MacroParser.TrimLeftWhitespaceWithNewLine(ref commentText);
        MacroParser.TrimRightWhitespaceWithNewLine(ref commentText);

        if (MacroParser.TrimLeftText(ref commentText, MacroParser.TextSlashStar.AsSpan())) {
            if (MacroParser.TrimRightText(ref commentText, MacroParser.TextStarSlash.AsSpan())) {
                MacroParser.TrimLeftWhitespaceNoNewLine(ref commentText);
                MacroParser.TrimRightWhitespaceNoNewLine(ref commentText);

                if (MacroParser.TrimLeftText(ref commentText, MacroParser.TextMacro.AsSpan())) {
                    MacroParser.TrimLeftWhitespaceNoNewLine(ref commentText);
                    if (commentText.IsEmpty) {
                        macroText = commentText;
                        return 0;
                    } else {
                        macroText = commentText;
                        return 1;
                    }
                }

                if (MacroParser.TrimLeftText(ref commentText, MacroParser.TextEndMacro.AsSpan())) {
                    MacroParser.TrimLeftWhitespaceNoNewLine(ref commentText);
                    macroText = commentText;
                    return 2;

                    //if (commentText.IsEmpty) {
                    //    macroText = commentText;
                    //    return 2;
                    //} else {
                    //    macroText = commentText;
                    //    return 2;
                    //}
                }
            }
        }

        macroText = string.Empty.AsSpan();
        return 0;
    }

    public static bool TryGetRegionBlockStart(ReadOnlySpan<char> regionText, out ReadOnlySpan<char> macroText) {
        MacroParser.TrimLeftWhitespaceNoNewLine(ref regionText);
        if (MacroParser.TrimLeftText(ref regionText, MacroParser.TextMacro.AsSpan())) {
            MacroParser.TrimLeftWhitespaceNoNewLine(ref regionText);
            MacroParser.TrimRightWhitespaceWithNewLine(ref regionText);
            if (!regionText.IsEmpty) {
                macroText = regionText;
                return true;
            }
        }
        macroText = string.Empty.AsSpan();
        return false;
    }

    public static bool TryGetRegionBlockEnd(ReadOnlySpan<char> regionText, out ReadOnlySpan<char> macroText, out LocationTag locationTag) {
        MacroParser.TrimLeftWhitespaceNoNewLine(ref regionText);
        if (MacroParser.TrimLeftText(ref regionText, MacroParser.TextEndMacro.AsSpan())) {
            MacroParser.TrimLeftWhitespaceNoNewLine(ref regionText);
            MacroParser.TrimRightWhitespaceNoNewLine(ref regionText);
            MacroParser.SplitLocationTag(regionText, out macroText, out locationTag);
            return true;
        }
        macroText = string.Empty.AsSpan();
        locationTag = new LocationTag();
        return true;
    }

    public static bool TrimLeftText(ref ReadOnlySpan<char> text, ReadOnlySpan<char> lookingFor) {
        if (!text.StartsWith(lookingFor, StringComparison.OrdinalIgnoreCase)) { return false; }
        text = text.Slice(lookingFor.Length);
        return true;
    }

    public static bool TrimRightText(ref ReadOnlySpan<char> text, ReadOnlySpan<char> lookingFor) {
        if (!text.EndsWith(lookingFor, StringComparison.OrdinalIgnoreCase)) { return false; }
        text = text[..^(lookingFor.Length)];
        return true;
    }

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
    public static bool TrimLeftWhitespaceWithNewLine(ref ReadOnlySpan<char> text) {
        bool result = false;
        while ((text.Length > 0)
                && ((text[0] is ' ' or '\t' or '\r' or '\n') || char.IsWhiteSpace(text[0]))) {
            text = text.Slice(1);
            result = true;
        }
        return result;
    }

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

    public static bool IsWhitespaceNotNewLine(char value)
        => (value) switch {
            ' ' => true,
            '\t' => true,
            '\r' => false,
            '\n' => false,
            _ => char.IsWhiteSpace(value)
        };

    public static bool NeedNewLine(string stringBefore, string stringAfter) {
        if (string.IsNullOrEmpty(stringBefore)) { return false; }
        if (string.IsNullOrEmpty(stringAfter)) { return false; }
        var charBeforeLast = stringBefore[stringBefore.Length - 1];
        var charAfterFirst = stringAfter[stringAfter.Length - 1];
        if (charBeforeLast is '\r' or '\n') { return false; }
        if (charAfterFirst is '\r' or '\n') { return false; }
        return true;
    }

    public static int GotoLeftWhileWhitespace(string value, int index) {
        while (0 < index) {
            if (MacroParser.IsWhitespaceNotNewLine(value[index - 1])) {
                index--;
            } else {
                break;
            }
        }
        return index;
    }

    public static int GotoLeftIfNewline(string value, int index) {
        if (0 < index && value[index - 1] == '\n') {
            index--;
        }
        if (0 < index && value[index - 1] == '\r') {
            index--;
        }
        return index;
    }

    public static int GotoRightWhileWhitespace(string value, int index) {
        while (index < value.Length && MacroParser.IsWhitespaceNotNewLine(value[index])) {
            index++;
        }
        return index;
    }

    public static int GotoRightIfNewline(string value, int index) {
        if (index < value.Length && value[index] == '\r') {
            index++;
        }
        if (index < value.Length && value[index] == '\n') {
            index++;
        }
        return index;
    }

    private static char[] _NewLines = "\r\n".ToCharArray();
    private static char[] _Whitespaces = "\r\n\t ".ToCharArray();

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

    public static bool SplitLocationTag(ReadOnlySpan<char> regionText, out ReadOnlySpan<char> macroText, out LocationTag locationTag) {
        // split the regionText into macroText and locationTag - the separator is the last # character.
        int index = regionText.LastIndexOf('#');
        if (0<=index) {
            macroText = regionText.Slice(0, index);
            var locationText = regionText.Slice(index + 1);
            locationTag = ParseLocationTag(locationText);
            return true;
        }
        // not found
        macroText = regionText;
        locationTag = new LocationTag();
        return false;
    }

    public static LocationTag ParseLocationTag(ReadOnlySpan<char> locationText) {
        if (int.TryParse(locationText, out int line)) {
            return new LocationTag(null, line);
        } else { 
            return new LocationTag(null, 0);
        }
    }
}
