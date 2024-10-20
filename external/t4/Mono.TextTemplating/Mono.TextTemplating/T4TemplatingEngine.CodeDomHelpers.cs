// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Mono.TextTemplating;

// NOTE: these should not have been public, but keep them here so as not to break API
public partial class T4TemplatingEngine {
    /// <summary>
    /// An implementation of CodeDomProvider.GenerateCodeFromMember that works on Mono.
    /// </summary>
    internal static void GenerateCodeFromMembers(CodeDomProvider provider, CodeGeneratorOptions options, StringWriter sw, IEnumerable<CodeTypeMember> members)
        => IndentHelpers.GenerateCodeFromMembers(provider, options, sw, members);
    
    internal static string GenerateIndentedClassCode(CodeDomProvider provider, params CodeTypeMember[] members)
        => IndentHelpers.GenerateIndentedClassCode(provider, members);

    internal static string GenerateIndentedClassCode(CodeDomProvider provider, IEnumerable<CodeTypeMember> members)
        => IndentHelpers.GenerateIndentedClassCode(provider, members);

    internal static string IndentSnippetText(CodeDomProvider provider, string text, string indent)
        => IndentHelpers.IndentSnippetText(provider, text, indent);

    internal static string IndentSnippetText(string text, string indent)
        => IndentHelpers.IndentSnippetText(text, indent);
}
