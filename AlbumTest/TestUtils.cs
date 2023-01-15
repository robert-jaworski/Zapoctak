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

		protected IEnumerable<string> GetFilesWithPrefix(ref string fullPath) {
			fullPath = Path.TrimEndingDirectorySeparator(fullPath) + Path.DirectorySeparatorChar;
			var p = fullPath;
			return from f in Files
				   where f.StartsWith(p)
				   select f;
		}

		public IEnumerable<string> EnumerateFiles(string fullPath) {
			return from f in GetFilesWithPrefix(ref fullPath)
				   where !f[fullPath.Length..].Contains(Path.DirectorySeparatorChar)
				   select f;
		}

		public IEnumerable<string> EnumerateDirectories(string fullPath) {
			var result = from f in GetFilesWithPrefix(ref fullPath)
				   where f[fullPath.Length..].Contains(Path.DirectorySeparatorChar)
				   let x = f[fullPath.Length..].Split(Path.DirectorySeparatorChar)[0]
				   select fullPath + x;
			return result.Distinct();
		}

		public bool FileExists(string fullPath) {
			return Files.Any(f => f == fullPath);
		}

		public string GetFullPath(string relPath) {
			return Path.GetFullPath(Path.Combine(Directory, relPath)).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		}

		public DateTime FileCreation(string fullPath) {
			throw new NotImplementedException();
		}

		public DateTime FileModification(string fullPath) {
			throw new NotImplementedException();
		}

		public Stream OpenFile(string fullPath) {
			throw new NotImplementedException();
		}

		public IReadOnlyList<MetadataExtractor.Directory> GetFileInfo(string fullPath) {
			throw new NotImplementedException();
		}

		public string GetAlbumDirectory() {
			throw new NotImplementedException();
		}

		public string GetFullPathAlbum(string pathInAlbum) {
			throw new NotImplementedException();
		}
	}

	internal class ErrorTestFailHandler : IErrorHandler {
		public bool IsError => false;
		public bool WasError => false;

		public void Error(string message) {
			Assert.Fail($"Unexpected error: {message}");
		}

		public IEnumerable<string> GetUnprocessed() {
			return Array.Empty<string>();
		}

		public IEnumerable<string> GetAll() {
			return Array.Empty<string>();
		}
	}
}
