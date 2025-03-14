namespace Brimborium.Macro.Sample;

[Brimborium.Macro.Macro("hugo")]
internal partial class Sample021 {
    public required string Name { get; set; }

    /* Macro TestMe */
    public required int Age { get; set; }
    /* EndMacro */

    /* Macro TestMe #10 */
    public required System.DateTime Birthday { get; set; }
    /* EndMacro #10 */

    #region Macro TestMe #20
    public required string Nickname { get; set; }
    #endregion #20
}

internal partial class Sample021B {
    public required string Value { get; set; }
}