namespace JustLoaded.Filesystem;

/// <summary>
/// Routes filesystem operations by mod ID. Each mod name maps to its own filesystem.
/// Wildcard selector <c>"*"</c> fans out to all registered filesystems.
/// </summary>
public class CombinedFilesystem : IFilesystem
{
    public bool HandlesSource =>
        _fileSystems.Count > 0 && _fileSystems.Values.All(fs => fs.HandlesSource);

    private readonly Dictionary<string, IFilesystem> _fileSystems =
        new Dictionary<string, IFilesystem>();

    /// <param name="name">Mod identifier used to route lookups.</param>
    /// <param name="filesystem">Filesystem to register under that name.</param>
    public void AddFileSystem(string name, IFilesystem filesystem)
    {
        if (
            _fileSystems.Count > 0
            && _fileSystems.Values.First().HandlesSource != filesystem.HandlesSource
        )
            throw new ArgumentException(
                $"Cannot mix source-handling and non-source-handling filesystems in {nameof(CombinedFilesystem)}."
            );
        _fileSystems.Add(name, filesystem);
    }

    public Stream? OpenFile(ModAssetPath path)
    {
        var stream = MatchModId(path.modSelector)
            .Select(kvp => kvp.Value.OpenFile(path))
            .FirstOrDefault(f => f != null);
        return stream;
    }

    public IEnumerable<ModAssetPath> ListPaths(ModAssetPath path)
    {
        var paths = MatchModId(path.modSelector)
            .SelectMany(kvp =>
                kvp.Value.ListPaths(path).Select(p => new ModAssetPath(kvp.Key, p.path))
            );
        return paths;
    }

    public IEnumerable<ModAssetPath> ListFiles(
        ModAssetPath path,
        string pattern = "*",
        bool recursive = false
    )
    {
        var files = MatchModId(path.modSelector)
            .SelectMany(kvp =>
                kvp.Value.ListFiles(path, pattern, recursive)
                    .Select(f => new ModAssetPath(kvp.Key, f.path))
            );
        return files;
    }

    private IEnumerable<KeyValuePair<string, IFilesystem>> MatchModId(string modId) =>
        _fileSystems.Where(pair => modId == "*" || pair.Key == modId);
}
