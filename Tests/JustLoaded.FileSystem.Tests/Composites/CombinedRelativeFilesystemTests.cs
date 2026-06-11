using System.Diagnostics.CodeAnalysis;
using JustLoaded.Filesystem.Composites;
using JustLoaded.Filesystem.Implementations;
namespace JustLoaded.Filesystem.Tests.Composites;

//TODO Write test cases
public class CombinedRelativeFilesystemTests : RelativeFilesystemTester<CombinedRelativeSource>
{
    [NotNull]
    private VirtualFilesystem? Vfs { get; set; }

    protected override RelativeFilesystem SetupFilesystem()
    {
        Vfs = new VirtualFilesystem();
        var cfs = new CombinedFilesystem();
        cfs.AddFileSystem("mod", Vfs);
        return new RelativeFilesystem(cfs, "prefix".AsPath());
    }

    protected override void MakeFile(ModAssetPath fileName, string content)
    {
        Vfs.AddFile(fileName.path, content);
    }
}

public class CombinedRelativeSource : RelativeFilesystemTestSourceBase
{
    public override IEnumerable<(ModAssetPath file, ModAssetPath query)> GetSingleFileRelativeSource
    {
        get { yield break; }
    }

    public override IEnumerable<(
        ModAssetPath query,
        ModAssetPath[] files,
        ModAssetPath[] results
    )> GetListDirsSource
    {
        get { yield break; }
    }

    public override IEnumerable<(
        ModAssetPath query,
        ModAssetPath[] files,
        ModAssetPath[] results
    )> GetListFilesShallowSource
    {
        get { yield break; }
    }

    public override IEnumerable<(
        ModAssetPath query,
        ModAssetPath[] files,
        ModAssetPath[] results
    )> GetListFilesRecursiveSource
    {
        get { yield break; }
    }

    public override IEnumerable<(
        string pattern,
        ModAssetPath[] files,
        ModAssetPath[] results
    )> GetListFilesPatternSource
    {
        get { yield break; }
    }
}