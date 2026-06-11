namespace JustLoaded.Filesystem;

/// <summary>
/// Layers multiple filesystems with priority-based resolution. Earlier filesystems take precedence;
/// duplicate paths from lower-priority filesystems are silently skipped.
/// </summary>
public class UnifiedFileSystem : IFilesystem
{
    private readonly List<IFilesystem> _filesystems;

    public bool HandlesSource { get; }

    /// <param name="filesystems">Ordered highest to lowest priority.</param>
    public UnifiedFileSystem(IEnumerable<IFilesystem> filesystems)
    {
        _filesystems = new List<IFilesystem>(filesystems);

        if (_filesystems.Count == 0)
        {
            HandlesSource = false;
            return;
        }

        HandlesSource = _filesystems.First().HandlesSource;
        if (_filesystems.Any(fs => fs.HandlesSource != HandlesSource))
            throw new ArgumentException(
                $"Cannot mix source-handling and non-source-handling filesystems in {nameof(UnifiedFileSystem)}."
            );
    }

    public Stream? OpenFile(ModAssetPath path)
    {
        var stream = _filesystems.Select(fs => fs.OpenFile(path)).FirstOrDefault(f => f != null);
        return stream;
    }

    public IEnumerable<ModAssetPath> ListFiles(
        ModAssetPath path,
        string pattern = "*",
        bool recursive = false
    )
    {
        var visited = new HashSet<ModAssetPath>();

        var files = _filesystems
            .SelectMany(fs => fs.ListFiles(path: path, pattern: pattern, recursive: recursive))
            .Where(visited.Add);
        return files;
    }

    public IEnumerable<ModAssetPath> ListPaths(ModAssetPath path)
    {
        var visited = new HashSet<ModAssetPath>();

        var paths = _filesystems.SelectMany(fs => fs.ListPaths(path)).Where(visited.Add);
        return paths;
    }
}
