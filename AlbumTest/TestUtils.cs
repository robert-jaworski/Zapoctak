using AlbumLibrary;

namespace AlbumTest {
	internal class TestFileSystemProvider : IFileSystemProvider {
		protected List<string> Files { get; }
		protected string Directory { get; }

		public TestFileSystemProvider(string dir, IEnumerable<string> files) {
			Directory = dir;
			Files = new List<string>(from f in files select GetFullPath(f));
		}

		public bool DirectoryExists(string fullPath) {
			return Files.Any(f => f.StartsWith(Path.TrimEndingDirectorySeparator(fullPath) + Path.DirectorySeparatorChar));
		}

		public IEnumerable<string> EnumerateFiles(string fullPath) {
			return from f in Files
				   where f.StartsWith(Path.TrimEndingDirectorySeparator(fullPath) + Path.DirectorySeparatorChar)
				   select f;
		}

		public bool FileExists(string fullPath) {
			return Files.Any(f => f == fullPath);
		}

		public string GetFullPath(string relPath) {
			return Path.GetFullPath(Path.Combine(Directory, relPath));
		}
	}

	internal class ErrorTestFailHandler : IErrorHandler {
		public bool IsError => false;

		public void Error(string message) {
			Assert.Fail($"Unexpected error: {message}");
		}

		public IEnumerable<string> GetUnprocessed() {
			return Array.Empty<string>();
		}
	}
}
