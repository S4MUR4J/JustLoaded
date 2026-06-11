using System.Collections;
using FluentAssertions;
using NUnit.Framework;

namespace JustLoaded.Filesystem.Tests.Relative;

public abstract class RelativeFilesystemTester<TSource> : FilesystemTester<RelativeFilesystem, TSource> where TSource : IRelativeFilesystemTestSource, new() {
    protected static IEnumerable SourceSingleFileRelative => new TSource().SourceSingleFileRelative;
    
    [TestCaseSource(nameof(SourceSingleFileRelative))]
    public void GetSingleFileRelative(ModAssetPath file, ModAssetPath query) {
        var fileContent = "A cool file content";
        MakeFile(file, fileContent);

        using var stream = fs.OpenFile(query);
        stream.Should().NotBeNull();

        using var text = new StreamReader(stream);
        text.ReadLine().Should().Be(fileContent);
    }
    
    public override void GetSingleFile(ModAssetPath fileName) {
        Assert.Pass("Not applicable");
    }

    public override void AddGetMultiple(IEnumerable<ModAssetPath> fileNames) {
        Assert.Pass("Not applicable");
    }
}

public interface IRelativeFilesystemTestSource : IFilesystemTestSource {

    public IEnumerable SourceSingleFileRelative => GetSingleFileRelativeSource.Select(file => new TestCaseData(file.file, file.query));
    public IEnumerable<(ModAssetPath file, ModAssetPath query)> GetSingleFileRelativeSource { get; }
}

public abstract class RelativeFilesystemTestSourceBase : IRelativeFilesystemTestSource {
    //Dummy
    public IEnumerable<ModAssetPath> GetFileFlatFileName => Enumerable.Repeat("".AsPath().FromAnyMod(), 1);
    public IEnumerable<ModAssetPath[]> GetFilesFlatFileNames => Enumerable.Repeat(new [] {"".AsPath().FromAnyMod()}, 1);
    
    public abstract IEnumerable<(ModAssetPath file, ModAssetPath query)> GetSingleFileRelativeSource { get; }
    public abstract IEnumerable<(ModAssetPath query, ModAssetPath[] files, ModAssetPath[] results)> GetListDirsSource { get; }
    public abstract IEnumerable<(ModAssetPath query, ModAssetPath[] files, ModAssetPath[] results)> GetListFilesShallowSource { get; }
    public abstract IEnumerable<(ModAssetPath query, ModAssetPath[] files, ModAssetPath[] results)> GetListFilesRecursiveSource { get; }
    public abstract IEnumerable<(string pattern, ModAssetPath[] files, ModAssetPath[] results)> GetListFilesPatternSource { get; }
}