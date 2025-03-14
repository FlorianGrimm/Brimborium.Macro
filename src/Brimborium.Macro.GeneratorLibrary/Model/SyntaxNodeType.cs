#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057
#pragma warning disable IDE0301 // Simplify collection initialization

namespace Brimborium.Macro.Model;

/// <summary>
/// Describes a region syntax used.
/// </summary>
public enum SyntaxNodeType {
    None,
    Constant,
    SyntaxTrivia,
    RegionDirectiveTriviaSyntax,
    AttributeSyntax,
};
