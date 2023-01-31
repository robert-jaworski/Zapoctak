namespace AlbumLibrary {
	public interface IFileSystemProvider {
		string GetFullPath(string relPath);
		string GetRelativePath(string relativeTo, string path);

		string GetAlbumDirectory();
		string GetFullPathAlbum(string pathInAlbum);

		bool FileExists(string fullPath);
		bool DirectoryExists(string fullPath);
		IEnumerable<string> EnumerateFiles(string fullPath);
		IEnumerable<string> EnumerateDirectories(string fullPath);

		DateTime FileCreation(string fullPath);
		DateTime FileModification(string fullPath);

		Stream OpenFile(string fullPath);
		public IReadOnlyList<MetadataExtractor.Directory> GetFileInfo(string fullPath);

		public void CopyFile(string srcPath, string destPath, bool overwrite = false);
		public void CopyFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false);
	}

	public class NormalFileSystemProvider : IFileSystemProvider {
		protected string AlbumDirectory { get; }
		
		public NormalFileSystemProvider(string albumDirectory) {
			AlbumDirectory = albumDirectory;
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
			return Path.GetFullPath(relPath);
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

		public string GetAlbumDirectory() {
			return AlbumDirectory;
		}

		public string GetFullPathAlbum(string pathInAlbum) {
			return GetFullPath(Path.Combine(AlbumDirectory, pathInAlbum));
		}

		public void CopyFile(string srcPath, string destPath, bool overwrite = false) {
			File.Copy(srcPath, destPath, overwrite);
		}

		public string GetRelativePath(string relativeTo, string path) {
			return Path.GetRelativePath(relativeTo, path);
		}

		public void CopyFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false) {
			Directory.CreateDirectory(Path.GetDirectoryName(destPath));
			CopyFile(srcPath, destPath, overwrite);
		}
	}
}
