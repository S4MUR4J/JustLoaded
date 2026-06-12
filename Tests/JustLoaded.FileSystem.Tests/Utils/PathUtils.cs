namespace JustLoaded.Filesystem.Tests.Utils;

public static class PathUtils
{
    public static ModAssetPath At(string path, string mod = "*") => new ModAssetPath(mod, path);

    public static ModAssetPath[] Many(params string[] paths) =>
        paths.Select(p => new ModAssetPath("*", p)).ToArray();

    public static ModAssetPath[] ManyForMod(string mod, params string[] paths) =>
        paths.Select(p => new ModAssetPath(mod, p)).ToArray();
}
