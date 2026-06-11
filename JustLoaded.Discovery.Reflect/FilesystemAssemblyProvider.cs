using System.Reflection;
using JustLoaded.Filesystem;

namespace JustLoaded.Discovery.Reflect;

public class FilesystemAssemblyProvider(IFilesystem filesystem) : IAssemblyProvider
{
    public IEnumerable<Assembly> GetAssemblies()
    {
        var modAssetFiles = filesystem.ListFiles(
            path: ModAssetPath.EmptyModAssetPath,
            pattern: "*.dll",
            recursive: true
        );

        foreach (var file in modAssetFiles)
        {
            using var stream = filesystem.OpenFile(file);
            using var memStream = new MemoryStream();
            stream?.CopyTo(memStream);
            yield return Assembly.Load(memStream.ToArray());
        }
    }
}
