using JustLoaded.Filesystem;
using PathLib;

namespace JustLoaded.Filesystem.Tests;

public static class FileHelper {

    public static readonly IPurePath ResourcesPath = PathExtensions.Local.Join("Resources");

    public static void CreateFile(string fileName, string content) {
        var filePath = new PosixPath(ResourcesPath.ToPosix(), fileName);
        filePath.Parent().Mkdir(true);
        File.WriteAllText(filePath.ToPosix(), content);
    }
}