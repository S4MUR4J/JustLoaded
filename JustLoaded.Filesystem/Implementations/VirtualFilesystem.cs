using System.IO.Enumeration;
using System.Text;

namespace JustLoaded.Filesystem.Implementations;

/// <summary>
/// In-memory filesystem backed by a flat path → bytes dictionary.
/// </summary>
public class VirtualFilesystem : IFilesystem
{
    public bool HandlesSource => false;
    private readonly Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>();

    public void AddFile(string path, string content)
    {
        var normalized = NormalizePath(path);
        var data = Encoding.UTF8.GetBytes(content);

        _files[normalized] = data;
    }

    public Stream? OpenFile(ModAssetPath path)
    {
        var normalized = NormalizePath(path.path);
        if (!_files.TryGetValue(normalized, out var content))
            return null;

        return new MemoryStream(content);
    }

    public IEnumerable<ModAssetPath> ListFiles(
        ModAssetPath path,
        string pattern = "*",
        bool recursive = false
    )
    {
        var prefix = NormalizePrefix(path.path);
        return _files
            .Keys.Where(k => k.StartsWith(prefix))
            .Where(k => recursive || !k[prefix.Length..].Contains('/'))
            .Where(k => FileSystemName.MatchesSimpleExpression(pattern, Path.GetFileName(k)))
            .Select(k => new ModAssetPath(path.modSelector, k));
    }

    public IEnumerable<ModAssetPath> ListPaths(ModAssetPath path)
    {
        var prefix = NormalizePrefix(path.path);
        return _files
            .Keys.Where(k => k.StartsWith(prefix))
            .Select(k => k[prefix.Length..].Split('/')[0])
            .Where(segment => !string.IsNullOrEmpty(segment))
            .Distinct()
            .Where(segment => _files.Keys.Any(k => k.StartsWith(prefix + segment + "/")))
            .Select(segment => new ModAssetPath(path.modSelector, prefix + segment));
    }

    private static string NormalizePath(string path)
    {
        var stack = new Stack<string>();
        foreach (var part in path.Split('/'))
        {
            if (part == ".." && stack.Count > 0)
                stack.Pop();
            else if (part != "." && !string.IsNullOrEmpty(part))
                stack.Push(part);
        }
        return string.Join('/', stack.Reverse());
    }

    private static string NormalizePrefix(string path) =>
        string.IsNullOrEmpty(path) ? string.Empty : NormalizePath(path) + "/";
}
