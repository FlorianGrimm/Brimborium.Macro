//
// Copyright (c) Microsoft Corp (https://www.microsoft.com)
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

namespace Mono.TextTemplating.CodeCompilation;

public struct SemVersion : IComparable, IComparable<SemVersion>, IEquatable<SemVersion> {
    public static SemVersion Zero { get; } = new SemVersion(0, 0, 0, null, null, "0.0.0");

    private static readonly Regex SemVerRegex = new(
            @"(?<Major>0|(?:[1-9]\d*))(?:\.(?<Minor>0|(?:[1-9]\d*))(?:\.(?<Patch>0|(?:[1-9]\d*)))?(?:\-(?<PreRelease>[0-9A-Z\.-]+))?(?:\+(?<Meta>[0-9A-Z\.-]+))?)?",
            RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
        );


    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string PreRelease { get; }
    public string Meta { get; }
    public bool IsPreRelease { get; }
    public bool HasMeta { get; }
    public string VersionString { get; }

    public SemVersion(int major, int minor, int patch, string preRelease = null, string meta = null) :
            this(major, minor, patch, preRelease, meta, null) {
    }

    private SemVersion(int major, int minor, int patch, string preRelease, string meta, string versionString) {
        this.Major = major;
        this.Minor = minor;
        this.Patch = patch;
        this.IsPreRelease = !string.IsNullOrEmpty(preRelease);
        this.HasMeta = !string.IsNullOrEmpty(meta);
        this.PreRelease = this.IsPreRelease ? preRelease : null;
        this.Meta = this.HasMeta ? meta : null;

        if (!string.IsNullOrEmpty(versionString)) {
            this.VersionString = versionString;
        } else {
            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}.{1}.{2}", this.Major, this.Minor, this.Patch);

            if (this.IsPreRelease) {
                sb.AppendFormat(CultureInfo.InvariantCulture, "-{0}", this.PreRelease);
            }

            if (this.HasMeta) {
                sb.AppendFormat(CultureInfo.InvariantCulture, "+{0}", this.Meta);
            }

            this.VersionString = sb.ToString();
        }
    }

    public static bool TryParse(string version, out SemVersion semVersion) {
        semVersion = Zero;

        if (string.IsNullOrEmpty(version)) {
            return false;
        }

        var match = SemVerRegex.Match(version);
        if (!match.Success) {
            return false;
        }

        if (!int.TryParse(
                match.Groups["Major"].Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var major) ||
            !int.TryParse(
                match.Groups["Minor"].Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var minor) ||
            !int.TryParse(
                match.Groups["Patch"].Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var patch)) {
            return false;
        }

        semVersion = new SemVersion(
            major,
            minor,
            patch,
            match.Groups["PreRelease"]?.Value,
            match.Groups["Meta"]?.Value,
            version);

        return true;
    }



    public bool Equals(SemVersion other) {
        return this.Major == other.Major
               && this.Minor == other.Minor
               && this.Patch == other.Patch
               && string.Equals(this.PreRelease, other.PreRelease, StringComparison.OrdinalIgnoreCase)
               && string.Equals(this.Meta, other.Meta, StringComparison.OrdinalIgnoreCase);
    }

    public int CompareTo(SemVersion other) {
        if (this.Equals(other)) {
            return 0;
        }

        if (this.Major > other.Major) {
            return 1;
        }

        if (this.Major < other.Major) {
            return -1;
        }

        if (this.Minor > other.Minor) {
            return 1;
        }

        if (this.Minor < other.Minor) {
            return -1;
        }

        if (this.Patch > other.Patch) {
            return 1;
        }

        if (this.Patch < other.Patch) {
            return -1;
        }

        return StringComparer.InvariantCultureIgnoreCase.Compare(this.PreRelease, other.PreRelease) switch {
            1 => 1,
            -1 => -1,
            _ => StringComparer.InvariantCultureIgnoreCase.Compare(this.Meta, other.Meta)
        };
    }

    public int CompareTo(object obj) => (obj is SemVersion semVersion) ? this.CompareTo(semVersion) : -1;

    public override bool Equals(object obj) => (obj is SemVersion semVersion) && this.Equals(semVersion);

    public override int GetHashCode() {
        unchecked {
            var hashCode = this.Major;
            hashCode = (hashCode * 397) ^ this.Minor;
            hashCode = (hashCode * 397) ^ this.Patch;
            hashCode = (hashCode * 397) ^ (this.PreRelease != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.PreRelease) : 0);
            hashCode = (hashCode * 397) ^ (this.Meta != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Meta) : 0);
            return hashCode;
        }
    }

    public override string ToString() => this.VersionString;

    public static bool operator >(SemVersion left, SemVersion right) => left.CompareTo(right) == 1;
    public static bool operator <(SemVersion left, SemVersion right) => left.CompareTo(right) == -1;
    public static bool operator >=(SemVersion left, SemVersion right) => left.CompareTo(right) >= 0;
    public static bool operator <=(SemVersion left, SemVersion right) => left.CompareTo(right) <= 0;
    public static bool operator ==(SemVersion left, SemVersion right) => left.Equals(right);
    public static bool operator !=(SemVersion left, SemVersion right) => !(left == right);
}