#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0057
#pragma warning disable IDE0301 // Simplify collection initialization

using System.Text;

namespace Brimborium.Macro.Model;

public record struct LocationTag(
    string? FilePath,
    int LineIdentifier) {
    public static LocationTag Empty => new LocationTag(null, 0);

    public void Generate(StringBuilder sbOut) {
        if (this.FilePath is { } filePath && 0 < this.LineIdentifier) {
            sbOut.Append(" #");
            sbOut.Append(filePath);
        } else if (this.FilePath is null && 0 < this.LineIdentifier) {
            sbOut.Append(" #");
            sbOut.Append(this.LineIdentifier);
        }
    }

    public bool IsEmpty => this.LineIdentifier <= 0;

    public bool HasValue() => 0 < this.LineIdentifier;
}
