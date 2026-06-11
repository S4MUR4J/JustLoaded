using System.Diagnostics.CodeAnalysis;
using JustLoaded.Filesystem.Composites;
using JustLoaded.Filesystem.Implementations;

namespace JustLoaded.Filesystem.Tests.Composites;

public class UnifiedFilesystemTests : FilesystemTester<UnifiedFilesystem, FilesystemTestSource>
{
    [NotNull]
    private VirtualFilesystem? Primary { get; set; }

    protected override UnifiedFilesystem SetupFilesystem()
    {
        Primary = new VirtualFilesystem();
        var secondary = new VirtualFilesystem();
        return new UnifiedFilesystem([Primary, secondary]);
    }

    protected override void MakeFile(ModAssetPath fileName, string content)
    {
        Primary.AddFile(fileName.path, content);
    }
}