namespace Brimborium.Macro.Sample;

internal partial class Sample002 {
    public required string Name { get; set; }

    /* Macro TestMe */
    public required int Age { get; set; }
    /* EndMacro */

    /* Macro TestMe #10 */
    public required System.DateTime Birthday { get; set; }
    /* EndMacro #10 */

    #region Macro TestMe #20
    public required string Nickname1 { get; set; }

    #region Macro TestMe #30
    public required string Nickname2 { get; set; }
    #endregion #30

    #endregion #20
}