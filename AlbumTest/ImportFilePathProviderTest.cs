using AlbumLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace AlbumTest {
	[TestClass]
	public class ImportFilePathProviderTest {
		[TestMethod]
		[DataRow(1, new string[] { "img001.jpg", "img002.jpg" }, new string[] { "img001.jpg", "img002.jpg" },
			new string[] { }, new bool[] { }, new string[] { }, new string?[] { }, new string?[] { }, new string[] { "file", "file" })]
		[DataRow(2, new string[] { "images/01/", "images\\02\\" }, new string[] { },
			new string[] { "images\\01\\", "images\\02\\" }, new bool[] { false, false }, new string[] { }, new string?[] { }, new string?[] { }, new string[] { "dir", "dir" })]
		[DataRow(3, new string[] { "images/01/...", "images\\02\\..." }, new string[] { },
			new string[] { "images\\01\\", "images\\02\\" }, new bool[] { true, true }, new string[] { }, new string?[] { }, new string?[] { }, new string[] { "dir", "dir" })]
		[DataRow(4, new string[] { "img001.jpg...", "...test/img002.jpg", "test2\\img003.jpg", "...", "test2/img004.jpg" },
			new string[] { }, new string[] { }, new bool[] { }, new string[] { "", "test", "test2" }, new string?[] { "img001.jpg", null, "test2\\img003.jpg" },
			new string?[] { null, "test\\img002.jpg", "test2\\img004.jpg" }, new string[] { "range", "range", "range" })]
		public void ImportFilePathProvider_Process(int id, string[] specs,
			string[] singleFiles, string[] directories, bool[] dirRecursive, string[] rangeDirs, string?[] rangeStarts, string?[] rangeEnds, string[] types) {
			Logger.LogMessage($"Testing {id}: {string.Join(' ', specs)}");
			Assert.AreEqual(rangeDirs.Length, rangeStarts.Length);
			Assert.AreEqual(rangeStarts.Length, rangeEnds.Length);

			var res = ImportFilePathProvider.Process(specs, new HashSet<string> { ".jpg" }, new ErrorTestFailHandler());
			Assert.IsNotNull(res);
			var providers = res.GetFilePathProviders();
			Assert.AreEqual(types.Length, providers.Count);
			int singleFileI = 0, directoriesI = 0, rangeI = 0;
			for (var i = 0; i < types.Length; i++) {
				switch (types[i]) {
				case "file":
					Assert.IsTrue(providers[i] is SingleFilePathProvider);
					Assert.IsTrue(singleFileI < singleFiles.Length);
					Assert.AreEqual(singleFiles[singleFileI++].Replace('\\', Path.DirectorySeparatorChar),
						((SingleFilePathProvider)providers[i]).FilePath);
					break;
				case "dir":
					Assert.IsTrue(providers[i] is DirectoryFilePathProvider);
					Assert.IsTrue(directoriesI < directories.Length);
					Assert.IsTrue(directoriesI < dirRecursive.Length);
					Assert.AreEqual(dirRecursive[directoriesI],
						((DirectoryFilePathProvider)providers[i]).Recursive);
					Assert.AreEqual(directories[directoriesI++].Replace('\\', Path.DirectorySeparatorChar),
						((DirectoryFilePathProvider)providers[i]).DirectoryPath);
					break;
				case "range":
					Assert.IsTrue(providers[i] is RangeFilePathProvider);
					Assert.IsTrue(rangeI < rangeDirs.Length);
					Assert.AreEqual(rangeDirs[rangeI].Replace('\\', Path.DirectorySeparatorChar),
						((RangeFilePathProvider)providers[i]).DirectoryPath);
					Assert.AreEqual(rangeStarts[rangeI]?.Replace('\\', Path.DirectorySeparatorChar),
						((RangeFilePathProvider)providers[i]).StartPath);
					Assert.AreEqual(rangeEnds[rangeI++]?.Replace('\\', Path.DirectorySeparatorChar),
						((RangeFilePathProvider)providers[i]).EndPath);
					break;
				default:
					Assert.Fail($"Invalid FilePathProvider type: {types[i]}");
					break;
				}
			}
		}

		[TestMethod]
		[DataRow(1, new string[] { @"test\", "...", "test2" }, "Invalid file range specification")]
		[DataRow(2, new string[] { "test...", "...", "test2" }, "Invalid file range specification")]
		[DataRow(3, new string[] { "...test", "..." , "test2" }, "Invalid file range specification")]
		[DataRow(4, new string[] { "test1", "...", "test2", "...", "test3" }, "Invalid file range specification")]
		[DataRow(5, new string[] { "test", "..." }, "Invalid file range specification")]
		[DataRow(6, new string[] { "test", "...", "test2/" }, "Invalid file range specification")]
		[DataRow(7, new string[] { "test", "...", "test2..." }, "Invalid file range specification")]
		[DataRow(8, new string[] { "test", "...", "...test2" }, "Invalid file range specification")]
		[DataRow(9, new string[] { "test", "...", "..." }, "Invalid file range specification")]
		[DataRow(10, new string[] { "test/file1", "...", "test2/file2" }, "File range directories do not match")]
		public void ImportFilePathProvider_Process_Errors(int id, string[] specs, string expectErrorPrefix) {
			Logger.LogMessage($"Testing {id}: {string.Join(' ', specs)}");

			var err = new ErrorListHandler();

			var res = ImportFilePathProvider.Process(specs, new HashSet<string> { ".jpg" }, err);
			Assert.IsNotNull(res);
			Assert.IsTrue(err.IsError);

			var errors = err.GetUnprocessed().ToList();
			Assert.IsTrue(errors[0].StartsWith(expectErrorPrefix));
		}

		protected static string[] DummyFiles { get; } = new string[] {
			@"C:\images1\img001.jpg", @"C:\images1\img002.jpg", @"C:\images1\img003.jpg", @"C:\images1\img004.jpg", @"C:\images1\screenshot.png",
			@"C:\images2\01\img001.jpg", @"C:\images2\01\img002.jpg", @"C:\images2\02\img001.jpg", @"C:\images2\02\img002.jpg",
		};

		[TestMethod]
		[DataRow(1, new string[] { "img001.jpg", "img002.jpg" }, @"C:\images1", new string[] { @"C:\images1\img001.jpg", @"C:\images1\img002.jpg" })]
		[DataRow(2, new string[] { "01\\" }, @"C:\images2", new string[] { @"C:\images2\01\img001.jpg", @"C:\images2\01\img002.jpg" })]
		[DataRow(3, new string[] { "img003.jpg..." }, @"C:\images1", new string[] { @"C:\images1\img003.jpg", @"C:\images1\img004.jpg" })]
		[DataRow(4, new string[] { "...img002.jpg" }, @"C:\images1", new string[] { @"C:\images1\img001.jpg", @"C:\images1\img002.jpg" })]
		[DataRow(5, new string[] { "img002.jpg", "...", "img003.jpg" }, @"C:\images1", new string[] { @"C:\images1\img002.jpg", @"C:\images1\img003.jpg" })]
		[DataRow(6, new string[] { "images2\\..." }, @"C:\", new string[] { @"C:\images2\01\img001.jpg", @"C:\images2\01\img002.jpg", @"C:\images2\02\img001.jpg", @"C:\images2\02\img002.jpg", })]
		public void ImportFilePathProvider_GetFilePaths(int id, string[] specs, string dir, string[] expected) {
			Logger.LogMessage($"Testing {id}: {string.Join(' ', specs)}");

			var res = ImportFilePathProvider.Process(specs, new HashSet<string> { ".jpg" }, new ErrorTestFailHandler());
			Assert.IsNotNull(res);

			var sys = new TestFileSystemProvider(dir, DummyFiles);
			var files = res.GetFilePaths(sys, new ErrorTestFailHandler()).ToList();
			Assert.AreEqual(expected.Length, files.Count);
			for (var i = 0; i < expected.Length; i++) {
				Assert.AreEqual(expected[i].Replace('\\', Path.DirectorySeparatorChar), files[i]);	
			}
		}
	}
}
