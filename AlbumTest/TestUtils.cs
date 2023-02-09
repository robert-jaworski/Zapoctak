using AlbumLibrary;
using FileInfo = AlbumLibrary.FileInfo;

namespace AlbumTest {
	internal class TestFileSystemProvider : IFileSystemProvider {
		protected List<FileInfo> Files { get; }
		protected string Directory { get; }
		protected string AlbumDirectory { get; }
		protected bool EmulateCopy { get; set; }

		public TestFileSystemProvider(string dir, string albumDir, IEnumerable<FileInfo> files, bool emulateCopy = false) {
			Directory = dir;
			Files = files.ToList();
			EmulateCopy = emulateCopy;
			AlbumDirectory = albumDir;
		}

		public TestFileSystemProvider(string dir, IEnumerable<string> files, bool emulateCopy = false) {
			Directory = dir;
			Files = new List<FileInfo>(from f in files select new FileInfo(GetFullPath(f), null, DateTime.Now, DateTime.Now, null, null,
				GetRelativePath(Directory, GetFullPath(f))));
			EmulateCopy = emulateCopy;
			AlbumDirectory = dir;
		}

		public bool DirectoryExists(string fullPath) {
			return Files.Any(f => f.OriginalFilePath.StartsWith(Path.TrimEndingDirectorySeparator(fullPath) + Path.DirectorySeparatorChar));
		}

		protected IEnumerable<string> GetFilesWithPrefix(ref string fullPath) {
			fullPath = Path.TrimEndingDirectorySeparator(fullPath) + Path.DirectorySeparatorChar;
			var p = fullPath;
			return from f in Files
				   where f.OriginalFilePath.StartsWith(p)
				   select f.OriginalFilePath;
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
			return Files.Any(f => f.OriginalFilePath == fullPath);
		}

		public string GetFullPath(string relPath) {
			return Path.GetFullPath(Path.Combine(Directory, relPath)).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		}

		public IEnumerable<FileInfo> GetFileInfos(string fullPath) {
			return from f in Files
				   where f.OriginalFilePath == fullPath
				   select f;
		}

		public DateTime GetFileCreation(string fullPath) {
			return GetFileInfos(fullPath).First().FileCreation;
		}

		public DateTime GetFileModification(string fullPath) {
			return GetFileInfos(fullPath).First().FileModification;
		}

		public Stream OpenFile(string fullPath) {
			throw new NotImplementedException();
		}

		public IReadOnlyList<MetadataExtractor.Directory> GetFileInfo(string fullPath) {
			throw new NotImplementedException();
		}

		public string GetAlbumDirectory() {
			return AlbumDirectory;
		}

		public string GetFullPathAlbum(string pathInAlbum) {
			return Path.GetFullPath(Path.Combine(AlbumDirectory, pathInAlbum)).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		}

		public string GetRelativePath(string relativeTo, string path) {
			return Path.GetRelativePath(relativeTo, path);
		}

		public void CreateDirectory(string path) {
			if (!EmulateCopy)
				throw new NotImplementedException();
		}

		public void CopyFile(string srcPath, string destPath, bool overwrite = false) {
			if (!EmulateCopy)
				throw new NotImplementedException();
		}

		public void CopyFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false) {
			if (!EmulateCopy)
				throw new NotImplementedException();
		}

		public void MoveFile(string srcPath, string destPath, bool overwrite = false) {
			if (!EmulateCopy)
				throw new NotImplementedException();
		}

		public void MoveFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false) {
			if (!EmulateCopy)
				throw new NotImplementedException();
		}

		public void SetFileCreation(string fullPath, DateTime creationDate) {
			if (!EmulateCopy)
				throw new NotImplementedException();
		}

		public void SetFileModification(string fullPath, DateTime modificationDate) {
			if (!EmulateCopy)
				throw new NotImplementedException();
		}

		public void DeleteFile(string fullPath) {
			if (!EmulateCopy)
				throw new NotImplementedException();
		}

		public void NewTransaction(string? info = "") { }

		public void EndTransaction() { }

		public FileAttributes GetFileAttributes(string fullPath) {
			throw new NotImplementedException();
		}

		public void SetFileAttributes(string fullPath, FileAttributes attributes) {
			throw new NotImplementedException();
		}

		public StreamReader ReadText(string fullPath) {
			throw new NotImplementedException();
		}

		public TextWriter WriteText(string fullPath, bool append = false) {
			throw new NotImplementedException();
		}

		public FileSystemTransaction? Undo(ILogger logger) {
			throw new NotImplementedException();
		}

		public FileSystemTransaction? Redo(ILogger logger) {
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

	internal class TestFileInfoProvider : IFileInfoProvider {
		public FileInfo GetInfo(string fullPath, IFileSystemProvider fileSystem) {
			if (fileSystem is not TestFileSystemProvider)
				throw new NotImplementedException();
			return ((TestFileSystemProvider)fileSystem).GetFileInfos(fullPath).First();
		}
	}
}
