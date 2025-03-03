namespace Brimborium.Macro;

[System.AttributeUsage(
    System.AttributeTargets.Class
    | System.AttributeTargets.Struct
    | System.AttributeTargets.Enum
    | System.AttributeTargets.Property
    | System.AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = false)]
public sealed class MacroAttribute : System.Attribute {
    public MacroAttribute() {
    }

    public MacroAttribute(string name) {
        this.Name = name;
    }

    public string? Name { get; set; }
}

[System.AttributeUsage(
    System.AttributeTargets.Class
    | System.AttributeTargets.Struct
    | System.AttributeTargets.Enum
    | System.AttributeTargets.Property
    | System.AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = false)]
public sealed class MacroSourceAttribute : System.Attribute {
    public MacroSourceAttribute() {
    }

    public MacroSourceAttribute(string name) {
        this.Name = name;
    }

    public string? Name { get; set; }
}
