namespace AlbumLibrary {
	public interface IFileSystemProvider {
		string GetFullPath(string relPath);

		bool FileExists(string fullPath);
		bool DirectoryExists(string fullPath);
		IEnumerable<string> EnumerateFiles(string fullPath);
	}

	public class DirectoryFileSystemProvider : IFileSystemProvider {
		protected string DirectoryName { get; }
		
		public DirectoryFileSystemProvider(string directoryName) {
			DirectoryName = directoryName;
		}

		public bool DirectoryExists(string fullPath) {
			return Directory.Exists(fullPath);
		}

		public IEnumerable<string> EnumerateFiles(string fullPath) {
			return Directory.EnumerateFiles(fullPath);
		}

		public bool FileExists(string fullPath) {
			return File.Exists(fullPath);
		}

		public string GetFullPath(string relPath) {
			return Path.GetFullPath(Path.Combine(DirectoryName, relPath));
		}
	}
}
