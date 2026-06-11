namespace JustLoaded.Filesystem;

/// <summary>
/// Read-only, mod-scoped filesystem abstraction. Implementations can map to disk, memory, or compose other filesystems.
/// </summary>
public interface IFilesystem {

    public bool HandlesSource { get; }

    public Stream? OpenFile(ModAssetPath path);

    public IEnumerable<ModAssetPath> ListPaths(ModAssetPath path);

    public IEnumerable<ModAssetPath> ListFiles(ModAssetPath path, string pattern = "*", bool recursive = false);
}
