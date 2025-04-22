namespace Brimborium.Text;

/// <summary>
/// Provides a shared pool of reusable StringBuilder instances to improve memory efficiency.
/// This class is a wrapper around Microsoft.Extensions.ObjectPool.ObjectPool&lt;StringBuilder&gt;
/// that provides a default singleton instance.
/// </summary>
public sealed class StringBuilderPool {
    private static StringBuilderPool? _Instance;
    
    /// <summary>
    /// Gets the singleton instance of the StringBuilderPool.
    /// </summary>
    public static StringBuilderPool Instance => _Instance ??= new StringBuilderPool();

    /// <summary>
    /// Gets a StringBuilder from the default pool instance.
    /// </summary>
    /// <returns>A StringBuilder that can be used temporarily and should be returned to the pool when no longer needed.</returns>
    public static StringBuilder GetStringBuilder() => Instance.Get();

    /// <summary>
    /// Returns a StringBuilder to the default pool instance.
    /// </summary>
    /// <param name="stringBuilder">The StringBuilder to return to the pool. Must not be null.</param>
    public static void ReturnStringBuilder(StringBuilder stringBuilder) => Instance.Return(stringBuilder);
    
    private readonly ObjectPool<StringBuilder> _Pool;

    /// <summary>
    /// Initializes a new instance of the StringBuilderPool class using the default pool provider.
    /// </summary>
    public StringBuilderPool() {
        this._Pool = new DefaultObjectPoolProvider().CreateStringBuilderPool();
    }

    /// <summary>
    /// Gets a StringBuilder from this pool instance.
    /// </summary>
    /// <returns>A StringBuilder that can be used temporarily and should be returned to the pool when no longer needed.</returns>
    public StringBuilder Get() => this._Pool.Get();

    /// <summary>
    /// Returns a StringBuilder to this pool instance.
    /// </summary>
    /// <param name="stringBuilder">The StringBuilder to return to the pool. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when stringBuilder is null.</exception>
    public void Return(StringBuilder stringBuilder) => this._Pool.Return(stringBuilder);
}
