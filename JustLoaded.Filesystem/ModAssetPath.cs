namespace JustLoaded.Filesystem;

/// <summary>
/// A mod-scoped file address combining a mod selector with a file path.
/// Used to uniquely identify assets across multiple mods loaded simultaneously.
/// </summary>
public class ModAssetPath : IEquatable<ModAssetPath> {

    /// <summary>File path within the mod's filesystem.</summary>
    public readonly string path;

    /// <summary>Identifies the owning mod. <c>"*"</c> matches any mod.</summary>
    public readonly string modSelector;

    /// <summary>Wildcard address matching any mod at the root. Use as a query base spanning all mods.</summary>
    public static readonly ModAssetPath EmptyModAssetPath = new ModAssetPath("*", string.Empty);

    /// <param name="modSelector">Mod identifier, or <c>"*"</c> to match any mod.</param>
    /// <param name="path">File path within the mod's filesystem.</param>
    internal ModAssetPath(string modSelector, string path) {
        this.modSelector = modSelector;
        this.path = path;
    }

    public bool Equals(ModAssetPath? other) {
        return other != null && modSelector == other.modSelector && path == other.path;
    }

    public override bool Equals(object? obj) {
        return obj is ModAssetPath other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(modSelector, path);
    }

    /// <summary>Returns a copy of this path with a different mod selector.</summary>
    public ModAssetPath WithMod(string newModSelector) => new ModAssetPath(newModSelector, path);

    /// <returns>String in <c>modSelector:path</c> format.</returns>
    public override string ToString() {
        return $"{modSelector}:{path}";
    }
}
