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
			Assert.AreEqual(exeDir, parsed.ExecutableDirectory);
			Assert.AreEqual(Path.GetFullPath(albumDir), parsed.AlbumDirectory);
			Assert.AreEqual(cmd, parsed.Command);
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
		[DataRow(3, new string[] { @"C:\Album\album.exe", "debug", "-d", @"E:\Foto", "hello", "-f", "img001.jpg", "img002.jpg", "-n", "123" },
			new string[] { "verbose" }, new bool[] { false },
			new string[] { "test-number" }, new int[] { 123 },
			new string[] { "album-dir", "test-implicit" }, new string[] { @"E:\Foto", "hello" },
			new string[] { "test-files" }, new string[] { "img001.jpg", "img002.jpg" })]
		[DataRow(4, new string[] { @"C:\Album\album.exe", "debug", "-d", @"E:\Foto", "hello", "-nf", "123", "img001.jpg", "img002.jpg" },
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
				Assert.AreEqual(new FlagArgument(v), parsed.NamedArguments[k]);
			}

			foreach (var (k, v) in expectedNumber.Zip(expectedNumberValues)) {
				Assert.IsTrue(parsed.NamedArguments.ContainsKey(k));
				Assert.AreEqual(new NumberArgument(v), parsed.NamedArguments[k]);
			}

			foreach (var (k, v) in expectedString.Zip(expectedStringValues)) {
				Assert.IsTrue(parsed.NamedArguments.ContainsKey(k));
				Assert.AreEqual(new StringArgument(v), parsed.NamedArguments[k]);
			}

			foreach (var (k, v) in expectedFiles.Zip(expectedFilesValues)) {
				Assert.IsTrue(parsed.NamedArguments.ContainsKey(k));
				Assert.AreEqual(new FilesArgument(v.ToList()), parsed.NamedArguments[k]);
			}
		}

		[TestMethod]
		[DataRow(0, new string[] { @"C:\Album\album.exe", "help", "-v", "-v" }, "Duplicate argument: --verbose", null)]
		[DataRow(1, new string[] { @"C:\Album\album.exe", "help", "--verbose", "--verbose" }, "Duplicate argument: --verbose", null)]
		[DataRow(2, new string[] { @"C:\Album\album.exe", "help", "123" }, "Unexpected implicit argument: 123", null)]
		[DataRow(3, new string[] { @"C:\Album\album.exe", "debug" }, "Missing required parameter: test-implicit", null)]
		[DataRow(4, new string[] { @"C:\Album\album.exe", "debug", "test", "-n" }, "Argument test-number requires a number but nothing was given", null)]
		[DataRow(5, new string[] { @"C:\Album\album.exe", "debug", "test", "-n", "abc" }, "Argument test-number requires a number but 'abc' was given", null)]
		[DataRow(6, new string[] { @"C:\Album\album.exe", "debug", "--test-implicit" }, "Argument test-implicit requires a string but nothing was given", null)]
		[DataRow(7, new string[] { @"C:\Album\album.exe", "debug", "--foo" }, null, "Unknown argument: --foo")]
		[DataRow(8, new string[] { @"C:\Album\album.exe", "debug", "-vx" }, null, "Unknown argument: -x")]
		public void ArgumentsProcessor_ParseArguments_Exceptions(int id, string[] args, string argException, string notSupported) {
			Logger.LogMessage($"Testing {id}: {string.Join(' ', args)}");
			if (argException != null) {
				Assert.ThrowsException<ArgumentException>(() => CommandArguments.ParseArguments(args), argException);
				Logger.LogMessage($"ArgumentException: {argException}");
			} else if (notSupported != null) {
				Assert.ThrowsException<NotSupportedException>(() => CommandArguments.ParseArguments(args), notSupported);
				Logger.LogMessage($"NotSupportedException: {notSupported}");
			}
		}
	}
}