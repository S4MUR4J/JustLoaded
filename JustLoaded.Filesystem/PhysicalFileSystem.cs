namespace JustLoaded.Filesystem;

/// <summary>
/// Disk-backed filesystem rooted at a given directory. Paths are resolved relative to the root.
/// </summary>
public class PhysicalFilesystem(string rootParam) : IFilesystem
{
    private string Root { get; } = rootParam;

    public bool HandlesSource => false;

    public Stream OpenFile(ModAssetPath path)
    {
        var concretePath = Path.Combine(Root, path.path);

        if (!File.Exists(concretePath))
            throw new FileNotFoundException($"File not found: {path}");

        return File.Open(path: concretePath, mode: FileMode.Open);
    }

    public IEnumerable<ModAssetPath> ListFiles(
        ModAssetPath path,
        string pattern = "*",
        bool recursive = false
    )
    {
        var concretePath = Path.Combine(Root, path.path);

        if (!Directory.Exists(concretePath))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var modAssetPaths = Directory
            .EnumerateFiles(path: concretePath, searchPattern: pattern, searchOption: searchOption)
            .Select(filePath => new ModAssetPath(
                path.modSelector,
                Path.GetRelativePath(Root, filePath)
            ));
        return modAssetPaths;
    }

    public IEnumerable<ModAssetPath> ListPaths(ModAssetPath path)
    {
        var concretePath = Path.Combine(Root, path.path);

        if (!Directory.Exists(concretePath))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        var modAssetPaths = Directory
            .EnumerateDirectories(
                path: concretePath,
                searchPattern: "*",
                searchOption: SearchOption.TopDirectoryOnly
            )
            .Select(filePath => new ModAssetPath(
                path.modSelector,
                Path.GetRelativePath(Root, filePath)
            ));
        return modAssetPaths;
    }
}
