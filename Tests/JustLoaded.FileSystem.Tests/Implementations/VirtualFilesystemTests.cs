using System.Text;
using FluentAssertions;
using JustLoaded.Filesystem.Implementations;
using JustLoaded.Filesystem.Tests.Utils;
using Xunit;

namespace JustLoaded.Filesystem.Tests.Implementations;

public class VirtualFilesystemTests
{
    private const string AssetsFolder = "assets";
    private const string AssetFileName = "asset.json";

    private const string AssetFileContentOne = "content_value_one_1";
    private const string AssetFileContentTwo = "content_value_two_2";

    private const string ModSelectorOne = "ModOne";
    private const string ModSelectorTwo = "ModTwo";

    private static readonly string[] Files =
    [
        "assetRoot.json",
        "assetReadme",
        "assetNotes.txt",
        "assetMemo.txt",
        "dirAssetsOne/assetOne.json",
        "dirAssetsOne/assetTwo.json",
        "dirAssetsTwo/assetThree.png",
        "dirAssetsThree/assetFour.json",
        "dirAssetsThree/dirNestedOne/assetFive.json",
        "dirAssetsThree/dirNestedOne/assetSix.json",
        "dirAssetsThree/dirNestedTwo/assetSeven.png",
        "dirDeep/dirInnerOne/dirInnerTwo/dirInnerThree/assetDeep.txt",
    ];

    private const string CanonicalPath = AssetsFolder + "/" + AssetFileName;
    private const string LeadingDotSlashPath = "./" + CanonicalPath;
    private const string DoubleSlashPath = AssetsFolder + "//" + AssetFileName;
    private const string MidDotSegmentPath = AssetsFolder + "/./" + AssetFileName;
    private const string DotDotSegmentPath = AssetsFolder + "/sibling/../" + AssetFileName;
    private const string LeadingSlashPath = "/" + CanonicalPath;
    private const string TrailingSlashPath = CanonicalPath + "/";

    [Fact]
    public void HandlesSourceByDefaultIsFalse()
    {
        // Arrange & Act
        var handlesSource = new VirtualFilesystem().HandlesSource;

        // Assert
        handlesSource.Should().BeFalse();
    }

    public class OpenFile
    {
        [Fact]
        public void ReturnsStreamWithAddedContent()
        {
            // Arrange
            var virtualFilesystem = new VirtualFilesystem();
            var path = Path.Combine(AssetsFolder, AssetFileName);
            virtualFilesystem.AddFile(path, AssetFileContentOne);

            // Act
            var modAssetPath = PathUtils.At(path);
            var stream = virtualFilesystem.OpenFile(modAssetPath);
            var content = ReadAll(stream);

            // Assert
            content.Should().Be(AssetFileContentOne);
        }

        [Fact]
        public void ReturnsNullWhenFileMissing()
        {
            // Arrange & Act
            var modAssetPath = PathUtils.At(AssetsFolder, AssetFileName);
            var stream = new VirtualFilesystem().OpenFile(modAssetPath);

            // Assert
            stream.Should().BeNull();
        }

        [Fact]
        public void AddingExistingFileOverridesPreviousContent()
        {
            // Arrange
            var virtualFilesystem = new VirtualFilesystem();
            virtualFilesystem.AddFile(AssetFileName, AssetFileContentOne);
            virtualFilesystem.AddFile(AssetFileName, AssetFileContentTwo);

            // Act
            var modAssetPath = PathUtils.At(AssetFileName);
            var stream = virtualFilesystem.OpenFile(modAssetPath);
            var content = ReadAll(stream);

            // Assert
            content.Should().Be(AssetFileContentTwo);
        }

        [Fact]
        public void ReturnsEmptyStreamWhenAddedContentIsEmpty()
        {
            // Arrange
            var virtualFilesystem = new VirtualFilesystem();
            virtualFilesystem.AddFile(AssetFileName, string.Empty);

            // Act
            var modAssetPath = PathUtils.At(AssetFileName);
            var stream = virtualFilesystem.OpenFile(modAssetPath);

            // Assert
            stream.Should().NotBeNull();
            stream.Length.Should().Be(0);
        }

        [Fact]
        public void PreservesUtf8MultibyteContentByteExact()
        {
            // Arrange
            const string multibyteContent = "żółć — 漢字";
            var virtualFilesystem = new VirtualFilesystem();
            virtualFilesystem.AddFile(AssetFileName, multibyteContent);

            // Act
            var modAssetPath = PathUtils.At(AssetFileName);
            var stream = virtualFilesystem.OpenFile(modAssetPath);
            using var memoryStream = new MemoryStream();
            stream!.CopyTo(memoryStream);

            // Assert
            memoryStream.ToArray().Should().Equal(Encoding.UTF8.GetBytes(multibyteContent));
        }

        [Fact]
        public void IgnoresQueryModSelector()
        {
            // Arrange
            var virtualFilesystem = new VirtualFilesystem();
            virtualFilesystem.AddFile(AssetFileName, AssetFileContentOne);

            // Act
            var firstModContent = ReadAll(
                virtualFilesystem.OpenFile(PathUtils.At(AssetFileName, ModSelectorOne))
            );
            var secondModContent = ReadAll(
                virtualFilesystem.OpenFile(PathUtils.At(AssetFileName, ModSelectorTwo))
            );

            // Assert
            firstModContent.Should().Be(AssetFileContentOne);
            secondModContent.Should().Be(AssetFileContentOne);
        }

        [Fact]
        public void ReturnsNullWhenQueryNormalizesToEmpty()
        {
            // Arrange
            var virtualFilesystem = new VirtualFilesystem();
            virtualFilesystem.AddFile(AssetFileName, AssetFileContentOne);

            // Act
            var emptyResult = virtualFilesystem.OpenFile(PathUtils.At(string.Empty));
            var dotResult = virtualFilesystem.OpenFile(PathUtils.At("./"));
            var dotDotResult = virtualFilesystem.OpenFile(PathUtils.At($"{AssetFileName}/.."));

            // Assert
            emptyResult.Should().BeNull();
            dotResult.Should().BeNull();
            dotDotResult.Should().BeNull();
        }
    }

    public class PathNormalization
    {
        [Theory]
        [InlineData(CanonicalPath)]
        [InlineData(LeadingDotSlashPath)]
        [InlineData(DoubleSlashPath)]
        [InlineData(MidDotSegmentPath)]
        [InlineData(DotDotSegmentPath)]
        [InlineData(LeadingSlashPath)]
        [InlineData(TrailingSlashPath)]
        public void AddFileWithEquivalentPathOverwritesCanonicalFile(string equivalentPath)
        {
            // Arrange
            var virtualFilesystem = new VirtualFilesystem();
            virtualFilesystem.AddFile(CanonicalPath, AssetFileContentOne);
            virtualFilesystem.AddFile(equivalentPath, AssetFileContentTwo);

            // Act
            var modAssetPath = PathUtils.At(CanonicalPath);
            var content = ReadAll(virtualFilesystem.OpenFile(modAssetPath));

            // Assert
            content.Should().Be(AssetFileContentTwo);
        }

        [Theory]
        [InlineData(LeadingDotSlashPath)]
        [InlineData(DoubleSlashPath)]
        [InlineData(MidDotSegmentPath)]
        [InlineData(DotDotSegmentPath)]
        [InlineData(LeadingSlashPath)]
        [InlineData(TrailingSlashPath)]
        public void OpenFileResolvesEquivalentPathToCanonicalFile(string equivalentPath)
        {
            // Arrange
            var virtualFilesystem = new VirtualFilesystem();
            virtualFilesystem.AddFile(CanonicalPath, AssetFileContentOne);

            // Act
            var modAssetPath = PathUtils.At(equivalentPath);
            var content = ReadAll(virtualFilesystem.OpenFile(modAssetPath));

            // Assert
            content.Should().Be(AssetFileContentOne);
        }

        [Fact]
        public void AddFileWithDotDotAtRootDoesNotCreateFile()
        {
            // Arrange
            var virtualFilesystem = new VirtualFilesystem();
            virtualFilesystem.AddFile("..", AssetFileContentOne);

            // Act
            var emptyResult = virtualFilesystem.OpenFile(PathUtils.At(string.Empty));
            var assetFileResult = virtualFilesystem.OpenFile(PathUtils.At(AssetFileName));

            // Assert
            emptyResult.Should().BeNull();
            assetFileResult.Should().BeNull();
        }
    }

    public class ListFilesShallow
    {
        [Theory]
        [InlineData(
            "",
            new[] { "assetRoot.json", "assetReadme", "assetNotes.txt", "assetMemo.txt" }
        )]
        [InlineData(
            "dirAssetsOne",
            new[] { "dirAssetsOne/assetOne.json", "dirAssetsOne/assetTwo.json" }
        )]
        [InlineData("dirAssetsTwo", new[] { "dirAssetsTwo/assetThree.png" })]
        [InlineData("dirAssetsThree", new[] { "dirAssetsThree/assetFour.json" })]
        [InlineData(
            "dirAssetsThree/dirNestedOne",
            new[]
            {
                "dirAssetsThree/dirNestedOne/assetFive.json",
                "dirAssetsThree/dirNestedOne/assetSix.json",
            }
        )]
        [InlineData(
            "dirAssetsOne/",
            new[] { "dirAssetsOne/assetOne.json", "dirAssetsOne/assetTwo.json" }
        )]
        [InlineData("dirMissing", new string[] { })]
        public void ReturnsOnlyDirectChildren(string query, string[] expected)
        {
            // Arrange
            var virtualFilesystem = Seeded(Files);

            // Act
            var listedFiles = virtualFilesystem.ListFiles(PathUtils.At(query));

            // Assert
            listedFiles.Should().BeEquivalentTo(PathUtils.Many(expected));
        }

        [Fact]
        public void EmptyFilesystemReturnsEmpty()
        {
            // Arrange & Act
            var listedFiles = new VirtualFilesystem().ListFiles(PathUtils.At(string.Empty));

            // Assert
            listedFiles.Should().BeEmpty();
        }
    }

    public class ListFilesRecursive
    {
        [Theory]
        [InlineData(
            "dirAssetsOne",
            new[] { "dirAssetsOne/assetOne.json", "dirAssetsOne/assetTwo.json" }
        )]
        [InlineData(
            "dirAssetsThree",
            new[]
            {
                "dirAssetsThree/assetFour.json",
                "dirAssetsThree/dirNestedOne/assetFive.json",
                "dirAssetsThree/dirNestedOne/assetSix.json",
                "dirAssetsThree/dirNestedTwo/assetSeven.png",
            }
        )]
        [InlineData(
            "dirDeep",
            new[] { "dirDeep/dirInnerOne/dirInnerTwo/dirInnerThree/assetDeep.txt" }
        )]
        [InlineData("dirMissing", new string[] { })]
        public void ReturnsAllFilesUnderPrefix(string query, string[] expected)
        {
            // Arrange
            var virtualFilesystem = Seeded(Files);

            // Act
            var listedFiles = virtualFilesystem.ListFiles(PathUtils.At(query), recursive: true);

            // Assert
            listedFiles.Should().BeEquivalentTo(PathUtils.Many(expected));
        }

        [Fact]
        public void RootQueryReturnsEverything()
        {
            // Arrange
            var virtualFilesystem = Seeded(Files);

            // Act
            var listedFiles = virtualFilesystem.ListFiles(
                PathUtils.At(string.Empty),
                recursive: true
            );

            // Assert
            listedFiles.Should().BeEquivalentTo(PathUtils.Many(Files));
        }
    }

    public class ListFilesPattern
    {
        [Theory]
        [InlineData(
            "*",
            new[] { "assetRoot.json", "assetReadme", "assetNotes.txt", "assetMemo.txt" }
        )]
        [InlineData("*.*", new[] { "assetRoot.json", "assetNotes.txt", "assetMemo.txt" })]
        [InlineData("*.txt", new[] { "assetNotes.txt", "assetMemo.txt" })]
        [InlineData("assetRoot.*", new[] { "assetRoot.json" })]
        [InlineData("*.md", new string[] { })]
        public void FiltersBySimpleWildcard(string pattern, string[] expected)
        {
            // Arrange
            var virtualFilesystem = Seeded(Files);

            // Act
            var listedFiles = virtualFilesystem.ListFiles(PathUtils.At(string.Empty), pattern);

            // Assert
            listedFiles.Should().BeEquivalentTo(PathUtils.Many(expected));
        }
    }

    public class ListFilesModSelector
    {
        [Fact]
        public void PreservesQueryModSelectorOnResults()
        {
            // Arrange
            var virtualFilesystem = Seeded(Files);

            // Act
            var listedFiles = virtualFilesystem
                .ListFiles(PathUtils.At(string.Empty, ModSelectorOne))
                .ToArray();

            // Assert
            listedFiles.Should().OnlyContain(p => p.modSelector == ModSelectorOne);
        }
    }

    public class ListPaths
    {
        [Theory]
        [InlineData("", new[] { "dirAssetsOne", "dirAssetsTwo", "dirAssetsThree", "dirDeep" })]
        [InlineData("dirAssetsOne", new string[] { })]
        [InlineData(
            "dirAssetsThree",
            new[] { "dirAssetsThree/dirNestedOne", "dirAssetsThree/dirNestedTwo" }
        )]
        [InlineData(
            "dirAssetsThree/",
            new[] { "dirAssetsThree/dirNestedOne", "dirAssetsThree/dirNestedTwo" }
        )]
        [InlineData("dirDeep", new[] { "dirDeep/dirInnerOne" })]
        [InlineData("dirMissing", new string[] { })]
        public void ReturnsDirectSubdirectoriesOnly(string query, string[] expected)
        {
            // Arrange
            var virtualFilesystem = Seeded(Files);

            // Act
            var listedSubdirectories = virtualFilesystem.ListPaths(PathUtils.At(query));

            // Assert
            listedSubdirectories.Should().BeEquivalentTo(PathUtils.Many(expected));
        }

        [Fact]
        public void EmptyFilesystemReturnsEmpty()
        {
            // Arrange & Act
            var listedSubdirectories = new VirtualFilesystem().ListPaths(
                PathUtils.At(string.Empty)
            );

            // Assert
            listedSubdirectories.Should().BeEmpty();
        }

        [Fact]
        public void PreservesQueryModSelectorOnResults()
        {
            // Arrange
            var virtualFilesystem = Seeded(Files);

            // Act
            var listedSubdirectories = virtualFilesystem.ListPaths(
                PathUtils.At(string.Empty, ModSelectorOne)
            );

            // Assert
            listedSubdirectories.Should().OnlyContain(p => p.modSelector == ModSelectorOne);
        }
    }

    private static string ReadAll(Stream? stream)
    {
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static VirtualFilesystem Seeded(string[] paths)
    {
        var virtualFilesystem = new VirtualFilesystem();
        foreach (var path in paths)
            virtualFilesystem.AddFile(path, path);
        return virtualFilesystem;
    }
}
