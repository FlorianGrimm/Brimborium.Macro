// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.TextTemplating;

internal static class HostOptionExtensions {
    private const string DisableAlcOptionName = "DisableAssemblyLoadContext";

    private static bool IsOptionTrue(this ITextTemplatingEngineHost host, string optionName) =>
        host.GetHostOption(optionName) is string optionVal
            && (optionVal == "1" || optionVal.Equals("true", StringComparison.OrdinalIgnoreCase));

    public static bool IsAssemblyLoadContextDisabled(this ITextTemplatingEngineHost host) => host.IsOptionTrue(DisableAlcOptionName);
}