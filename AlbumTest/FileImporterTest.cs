using AlbumLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using FileInfo = AlbumLibrary.FileInfo;

namespace AlbumTest {
	[TestClass]
	public class FileImporterTest {
		protected static FileInfo[] DummyFiles { get; } = new FileInfo[] {
			new FileInfo(@"C:\images1\img001.jpg", DateTime.Parse("4. 2. 2022 10:37:46")),
			new FileInfo(@"C:\images1\img002.jpg", DateTime.Parse("4. 2. 2022 10:37:56")),
			new FileInfo(@"C:\images1\img003.jpg", DateTime.Parse("4. 2. 2022 10:38:04")),
			new FileInfo(@"C:\images1\screenshot.png", DateTime.Parse("1. 2. 2022 22:02:21")),
			new FileInfo(@"C:\album\png\screenshot.png", DateTime.Parse("30. 1. 2022 18:36:25")),
		};

		[TestMethod]
		[DataRow(1, new string[] { "images1/" }, @"C:\album", new string[] {
			@"C:\images1\img001.jpg", @"C:\images1\img002.jpg", @"C:\images1\img003.jpg", @"C:\images1\screenshot.png"
		}, new string[] {
			@"C:\album\2022\02\220204-103746.jpg", @"C:\album\2022\02\220204-103756.jpg", @"C:\album\2022\02\220204-103804.jpg",
			@"C:\album\png\screenshota.png"
		})]
		public void FileImporter_ImportItems(int id, string[] specs, string albumDir, string[] expectedSources, string[] expectedDestinations) {
			Logger.LogMessage($"Testing {id}: {string.Join(' ', specs)}");

			var errHandler = new ErrorTestFailHandler();

			var importFilePathProvider = ImportFilePathProvider.Process(specs, new HashSet<string> { ".jpg", ".png" }, new ErrorTestFailHandler());

			var fileSystem = new TestFileSystemProvider(@"C:\", albumDir, DummyFiles, true);

			var fileNameProvider = new MultipleFileNameProvider(new TemplateFileNameProvider[] {
				new TemplateFileNameProvider(@"{YYYY}\{MM}\{YY}{MM}{DD}-{hh}{mm}{ss}{ext:.jpg}"),
				new TemplateFileNameProvider(@"png\{file:name}{ext:.png}"),
			});

			var importListProvider = new ImportListProvider(new TestFileInfoProvider(), fileNameProvider);

			var paths = importFilePathProvider.GetFilePaths(fileSystem, errHandler).ToList();

			var fileImporter = new FileCopyImporter(fileSystem, new SuffixDuplicateResolver(), "import");

			var importItems = importListProvider.GetImportItems(fileSystem, paths, errHandler);

			var imported = fileImporter.ImportItems(importItems, errHandler, new NoLogger());
			var expected = expectedSources.Zip(expectedDestinations);

			foreach (var item in imported) {
				Assert.IsTrue(!item.Cancelled && expected.Any(x => x.First == item.SourcePath && x.Second == item.DestinationPath));
			}
		}
	}
}
