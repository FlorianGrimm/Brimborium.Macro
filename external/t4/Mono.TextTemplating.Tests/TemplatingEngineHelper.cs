//
// Test.cs
//
// Author:
//       Mikayla Hutchinson <m.j.hutchinson@gmail.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;

namespace Mono.TextTemplating.Tests;

public static class TemplatingEngineHelper {
    /// <summary>
    /// Cleans CodeDOM generated code so that Windows/Mac and Mono/.NET output can be compared.
    /// </summary>
    public static string CleanCodeDom(string input, string newLine) {
        using var writer = new StringWriter();
        using var reader = new StringReader(input);

        bool afterLineDirective = true;
        bool stripHeader = true;

        string line;
        while ((line = reader.ReadLine()) != null) {

            if (stripHeader) {
                if (line.StartsWith("//", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(line))
                    continue;
                stripHeader = false;
            }

            if (afterLineDirective) {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                afterLineDirective = false;
            }

            if (line.Contains("#line")) {
                afterLineDirective = true;
            }

            writer.Write(line);
            writer.Write(newLine);
        }

        var result = writer.ToString();

        // \r\n can leak into the strings. we can't just sanitize the engine input, as it can also leak in via includes.
        if (newLine == "\n") {
            return result.Replace("\\r\\n", "\\n");
        }

        return result;
    }
}
