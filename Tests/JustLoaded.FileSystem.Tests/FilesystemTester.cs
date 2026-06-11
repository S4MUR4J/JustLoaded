using System.Collections;
using FluentAssertions;
using NUnit.Framework;

namespace JustLoaded.Filesystem.Tests;

public abstract class FilesystemTester<TFilesystem, TSource> where TSource : IFilesystemTestSource, new() where TFilesystem : IFilesystem {

    protected static IEnumerable SourceSingleFileFlat => new TSource().SourceSingleFileFlat;
    protected static IEnumerable SourceMultipleFilesFlat => new TSource().SourceGetMultipleFiles;
    
    protected static IEnumerable SourceListDirs => new TSource().SourceListDirs;
    protected static IEnumerable SourceListFilesShallow => new TSource().SourceListFilesShallow;
    protected static IEnumerable SourceListFilesRecursive => new TSource().SourceListFilesRecursive;
    protected static IEnumerable SourceListFilesPattern => new TSource().SourceListFilesPattern;
    

    protected TFilesystem fs;
    
    [SetUp]
    public void Setup() {
        fs = SetupFilesystem();
    }
    
    [TestCaseSource(nameof(SourceSingleFileFlat))]
    public virtual void GetSingleFile(ModAssetPath fileName) {
        var fileContent = "A Cool File Content";
        
        MakeFile(fileName, fileContent);
        
        using var stream = fs.OpenFile(fileName);
        stream.Should().NotBeNull();

        using var text = new StreamReader(stream);
        text.ReadLine().Should().Be(fileContent);
    }
    
    [TestCaseSource(nameof(SourceMultipleFilesFlat))]
    public virtual void AddGetMultiple(IEnumerable<ModAssetPath> fileNames) {
        var files = fileNames.ToArray();
        
        for (int i = 0; i < files.Length; i++) {
            MakeFile(files[i], i.ToString());
        }
        
        for (int i = 0; i < files.Length; i++) {
            AssertFileContents(fs, files[i], i.ToString());
        }
    }
    
    [TestCaseSource(nameof(SourceListDirs))]
    public void ListPaths(ModAssetPath listDir, ModAssetPath[] files, ModAssetPath[] expectedDirs) {
        foreach (var file in files) {
            MakeFile(file, file.ToString());
        }

        var dirsSet = new HashSet<ModAssetPath>(expectedDirs);
        foreach (var dir in fs.ListPaths(listDir)) {
            dirsSet.Should().Contain(dir);
            dirsSet.Remove(dir);
        }

        dirsSet.Should().BeEmpty();
    }
    
    [TestCaseSource(nameof(SourceListFilesShallow))]
    public void ListFilesShallow(ModAssetPath listDir, ModAssetPath[] files, ModAssetPath[] expectedFiles) {
        DoListFiles(listDir, files, expectedFiles, "*", false);
    }

    [TestCaseSource(nameof(SourceListFilesRecursive))]
    public void ListFilesRecursive(ModAssetPath listDir, ModAssetPath[] files, ModAssetPath[] expectedFiles) {
        DoListFiles(listDir, files, expectedFiles, "*", true);
    }
    
    [TestCaseSource(nameof(SourceListFilesPattern))]
    public void ListFilesPattern(string pattern, ModAssetPath[] files, ModAssetPath[] expectedFiles) {
        DoListFiles(ModAssetPath.Empty, files, expectedFiles, pattern, false);
    }
    
    protected abstract TFilesystem SetupFilesystem();
    
    protected abstract void MakeFile(ModAssetPath fileName, string content);
    
    protected void DoListFiles(ModAssetPath listDir, ModAssetPath[] files, ModAssetPath[] expectedFiles, string pattern, bool recursive) {
        foreach (var file in files) {
            MakeFile(file, file.ToString());
        }
        
        var dirsSet = new HashSet<ModAssetPath>(expectedFiles);
        foreach (var dir in fs.ListFiles(listDir, pattern, recursive)) {
            dirsSet.Should().Contain(dir);
            dirsSet.Remove(dir);
        }

        dirsSet.Should().BeEmpty();
    }
    
    protected void AssertFileContents(TFilesystem vfs, ModAssetPath file, string expectedContent) {
        using var stream1 = vfs.OpenFile(file);
        stream1.Should().NotBeNull();
        using var text = new StreamReader(stream1);
        text.ReadLine().Should().Be(expectedContent);
    }
}