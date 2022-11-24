using AlbumConsole;
using AlbumLibrary.CLI;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace AlbumTest {
	[TestClass]
	public class ArgumentsProcessorTest {
		[TestMethod]
		[DataRow(1, new string[] { @"C:\Album\album.exe" }, @"C:\Album", ".", "help")]
		[DataRow(2, new string[] { @"C:\Album\album.exe", "help", "-v" }, @"C:\Album", ".", "help")]
		[DataRow(3, new string[] { @"C:\Album\album.exe", "debug", "hello", "-d", @"E:\Foto" }, @"C:\Album", @"E:\Foto", "debug")]
		public void ArgumentsProcessor_ParseArguments_Basic(int id, string[] args, string exeDir, string albumDir, string cmd) {
			Logger.LogMessage($"Testing {id}: {string.Join(' ', args)}");
			var parsed = CommandArguments.ParseArguments(args);
			Assert.IsNotNull(parsed);
			Assert.AreEqual(parsed.ExecutableDirectory, exeDir);
			Assert.AreEqual(parsed.AlbumDirectory, Path.GetFullPath(albumDir));
			Assert.AreEqual(parsed.Command, cmd);
		}

		[TestMethod]
		[DataRow(1, new string[] { @"C:\Album\album.exe" },
			new string[] { "verbose" }, new bool[] { false },
			new string[] { }, new int[] { },
			new string[] { }, new string[] { },
			new string[] { })]
		[DataRow(2, new string[] { @"C:\Album\album.exe", "help", "-v" },
			new string[] { "verbose" }, new bool[] { true },
			new string[] { }, new int[] { },
			new string[] { }, new string[] { },
			new string[] { })]
		[DataRow(3, new string[] { @"C:\Album\album.exe", "debug", "-d", @"E:\Foto", "hello", "-fn", "img001.jpg", "img002.jpg", "-", "123" },
			new string[] { "verbose" }, new bool[] { false },
			new string[] { "test-number" }, new int[] { 123 },
			new string[] { "album-dir", "test-implicit" }, new string[] { @"E:\Foto", "hello" },
			new string[] { "test-files" }, new string[] { "img001.jpg", "img002.jpg" })]
		public void ArgumentsProcessor_ParseArguments_Params(int id, string[] args, string[] expectedFlag, bool[] expectedFlagValues,
				string[] expectedNumber, int[] expectedNumberValues, string[] expectedString, string[] expectedStringValues,
				string[] expectedFiles, params string[][] expectedFilesValues) {
			Logger.LogMessage($"Testing {id}: {string.Join(' ', args)}");
			var parsed = CommandArguments.ParseArguments(args);
			Assert.IsNotNull(parsed);

			foreach (var (k, v) in expectedFlag.Zip(expectedFlagValues)) {
				Assert.IsTrue(parsed.NamedArguments.ContainsKey(k));
				Assert.AreEqual(parsed.NamedArguments[k], new FlagArgument(v));
			}

			foreach (var (k, v) in expectedNumber.Zip(expectedNumberValues)) {
				Assert.IsTrue(parsed.NamedArguments.ContainsKey(k));
				Assert.AreEqual(parsed.NamedArguments[k], new NumberArgument(v));
			}

			foreach (var (k, v) in expectedString.Zip(expectedStringValues)) {
				Assert.IsTrue(parsed.NamedArguments.ContainsKey(k));
				Assert.AreEqual(parsed.NamedArguments[k], new StringArgument(v));
			}

			foreach (var (k, v) in expectedFiles.Zip(expectedFilesValues)) {
				Assert.IsTrue(parsed.NamedArguments.ContainsKey(k));
				Assert.AreEqual(parsed.NamedArguments[k], new FilesArgument(v.ToList()));
			}
		}
	}
}