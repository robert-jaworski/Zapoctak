namespace AlbumLibrary {
	public interface IFileSystemProvider {
		string GetFullPath(string relPath);

		bool FileExists(string fullPath);
		bool DirectoryExists(string fullPath);
		IEnumerable<string> EnumerateFiles(string fullPath);
		IEnumerable<string> EnumerateDirectories(string fullPath);

		DateTime FileCreation(string fullPath);
		DateTime FileModification(string fullPath);

		Stream OpenFile(string fullPath);
		public IReadOnlyList<MetadataExtractor.Directory> GetFileInfo(string fullPath);
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

		public IEnumerable<string> EnumerateDirectories(string fullPath) {
			return Directory.EnumerateDirectories(fullPath);
		}

		public bool FileExists(string fullPath) {
			return File.Exists(fullPath);
		}

		public string GetFullPath(string relPath) {
			return Path.GetFullPath(Path.Combine(DirectoryName, relPath));
		}

		public DateTime FileCreation(string fullPath) {
			return File.GetCreationTime(fullPath);
		}

		public DateTime FileModification(string fullPath) {
			return File.GetLastWriteTime(fullPath);
		}

		public Stream OpenFile(string fullPath) {
			return File.OpenRead(fullPath);
		}

		public IReadOnlyList<MetadataExtractor.Directory> GetFileInfo(string fullPath) {
			return MetadataExtractor.ImageMetadataReader.ReadMetadata(fullPath);
		}
	}
}
