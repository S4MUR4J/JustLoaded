namespace JustLoaded.Filesystem;

static class FilesystemValidator
{
    internal static void AssertCompatible(IEnumerable<IFilesystem> filesystems, string callerName)
    {
        using var e = filesystems.GetEnumerator();
        if (!e.MoveNext())
            return;

        var expected = e.Current.HandlesSource;
        while (e.MoveNext())
        {
            if (e.Current.HandlesSource != expected)
                throw new ArgumentException(
                    $"Cannot mix source-handling and non-source-handling filesystems in {callerName}."
                );
        }
    }
}
