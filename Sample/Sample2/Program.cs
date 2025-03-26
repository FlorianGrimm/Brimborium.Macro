namespace Brimborium.Macro.Sample1;

[Brimborium.Macro.Macro("hugo")]
internal partial class Program {
    static void Main(string[] args) {
        #region Macro {{ Print "Hello, World!" 10 }}
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Hello, World!");
        Console.WriteLine("Hello, World!");
        #endregion
    }
}
