namespace JustLoaded.Filesystem;

/// <summary>
/// Wraps another filesystem and prepends a fixed path prefix to every operation.
/// Callers see paths relative to the prefix; the inner filesystem sees full prefixed paths.
/// </summary>
public class RelativeFilesystem(IFilesystem inner, string prefix) : IFilesystem
{
    public bool HandlesSource => inner.HandlesSource;

    private readonly string _prefix = prefix.StartsWith('/')
        ? throw new ArgumentException("Prefix must be a relative path.", nameof(prefix))
        : prefix.TrimEnd('/');

    public Stream? OpenFile(ModAssetPath path)
    {
        var prependedPath = Prepend(path);
        return inner.OpenFile(prependedPath);
    }

    public IEnumerable<ModAssetPath> ListFiles(
        ModAssetPath path,
        string pattern = "*",
        bool recursive = false
    )
    {
        var prependedPath = Prepend(path);
        return inner
            .ListFiles(path: prependedPath, pattern: pattern, recursive: recursive)
            .Select(Strip);
    }

    public IEnumerable<ModAssetPath> ListPaths(ModAssetPath path)
    {
        var prependedPath = Prepend(path);
        return inner.ListPaths(prependedPath).Select(Strip);
    }

    private ModAssetPath Prepend(ModAssetPath key) =>
        new ModAssetPath(key.modSelector, _prefix + "/" + key.path);

    private ModAssetPath Strip(ModAssetPath result) =>
        new ModAssetPath(
            result.modSelector,
            result.path.StartsWith(_prefix + "/")
                ? result.path[(_prefix.Length + 1)..]
                : result.path
        );
}
