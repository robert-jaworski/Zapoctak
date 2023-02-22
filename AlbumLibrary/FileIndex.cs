namespace AlbumLibrary {
	public class FileIndex {
		public FileIndexDirectory RootDirectory { get; }
		public string RootDirectoryPath { get; }

		public FileIndex(FileIndexDirectory directory, string rootDirectoryPath) {
			RootDirectory = directory;
			RootDirectoryPath = rootDirectoryPath;
		}

		public FileInfo? GetFileInfo(string path) => RootDirectory.GetFileInfo(path);

		public bool SetFileInfo(string path, FileInfo? info, bool overwrite = false) => RootDirectory.SetFileInfo(path, info, overwrite);

		public bool DirectoryExists(string path) => RootDirectory.GetDirectory(path) is not null;

		public IEnumerable<string> EnumerateDirectories(string path) {
			return from d in RootDirectory.EnumerateDirectories(path)
				   select Path.Join(RootDirectoryPath, d);
		}

		public IEnumerable<string> EnumerateFiles(string path) {
			return from f in RootDirectory.EnumerateFiles(path)
				   select Path.Join(RootDirectoryPath, f);
		}

		public IEnumerable<string> AllFiles() {
			return from f in RootDirectory.AllFiles()
				   select Path.Join(RootDirectoryPath, f);
		}

		public FileIndex(string rootDirectoryPath) : this(new(), rootDirectoryPath) { }

		public void WriteToStream(TextWriter writer) {
			writer.WriteLine(RootDirectoryPath);
			RootDirectory.WriteToStream(".", writer);
			writer.WriteLine();
		}

		public static FileIndex ReadFromStream(IFileSystemProvider fileSystem, StreamReader stream) {
			var index = new FileIndex(stream.ReadLine() ?? ".");
			var path = stream.ReadLine();
			while (!string.IsNullOrEmpty(path)) {
				var p = fileSystem.GetFullPathAlbum(path);
				var info = FileInfo.ReadFromStream(p, path, stream);
				if (info is null)
					break;
				index.SetFileInfo(path, info, false);
				path = stream.ReadLine();
			}
			//foreach (var d in index.EnumerateDirectories()) {
				
			//}
			return index;
		}
	}

	public class FileIndexDirectory {
		public Dictionary<string, FileIndexDirectory> Directories { get; }
		public Dictionary<string, FileInfo> Files { get; }

		public FileIndexDirectory(Dictionary<string, FileIndexDirectory> directories, Dictionary<string, FileInfo> files) {
			Directories = directories;
			Files = files;
		}

		public FileIndexDirectory() : this(new(), new()) { }

		protected static string GetPath(string path) => path.TrimStart('.', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
			.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

		public FileInfo? GetFileInfo(string path) {
			return GetFile(GetPath(path));
		}

		protected FileInfo? GetFile(string path) {
			if (path.Contains(Path.DirectorySeparatorChar)) {
				var d = path.IndexOf(Path.DirectorySeparatorChar);
				if (Directories.ContainsKey(path[..d]))
					return Directories[path[..d]].GetFile(path[(d + 1)..]);
				return null;
			} else {
				if (Files.ContainsKey(path))
					return Files[path];
				return null;
			}
		}

		public bool SetFileInfo(string path, FileInfo? info, bool overwrite = false) {
			var p = GetPath(path);
			if (info is null)
				return RemoveFile(p);
			if (!overwrite) {
				if (GetFile(p) is not null)
					return false;
			}
			SetFile(p, info);
			return true;
		}

		protected void SetFile(string path, FileInfo info) {
			if (path.Contains(Path.DirectorySeparatorChar)) {
				var d = path.IndexOf(Path.DirectorySeparatorChar);
				CreateDirectory(path[..d]);
				Directories[path[..d]].SetFile(path[(d + 1)..], info);
			} else {
				Files[path] = info;
			}
		}

		protected bool RemoveFile(string path) {
			if (path.Contains(Path.DirectorySeparatorChar)) {
				var d = path.IndexOf(Path.DirectorySeparatorChar);
				if (Directories.ContainsKey(path[..d])) {
					return Directories[path[..d]].RemoveFile(path[(d + 1)..]);
				}
				return false;
			} else {
				Files.Remove(path);
				return true;
			}
		}

		protected void CreateDirectory(string name) {
			if (!Directories.ContainsKey(name))
				Directories.Add(name, new FileIndexDirectory());
		}

		public FileIndexDirectory? GetDirectory(string path) {
			return RecurseDirectory(GetPath(path));
		}

		protected FileIndexDirectory? RecurseDirectory(string path) {
			if (path.Contains(Path.DirectorySeparatorChar)) {
				var d = path.IndexOf(Path.DirectorySeparatorChar);
				if (Directories.ContainsKey(path[..d]))
					return Directories[path[..d]].RecurseDirectory(path[(d + 1)..]);
				return null;
			} else {
				if (Directories.ContainsKey(path))
					return Directories[path];
				return null;
			}
		}

		public IEnumerable<string> EnumerateDirectories() {
			return Directories.Keys;
		}

		public IEnumerable<string> EnumerateDirectories(string path) {
			// Console.Error.WriteLine("Enumerating {0}", path);
			if (path == ".")
				return EnumerateDirectories();
			var dir = GetDirectory(path);
			if (dir is null)
				return Array.Empty<string>();
			return from d in dir.EnumerateDirectories()
				   select Path.Join(path, d);
		}

		public IEnumerable<string> EnumerateFiles() {
			return Files.Keys;
		}

		public IEnumerable<string> EnumerateFiles(string path) {
			var dir = GetDirectory(path);
			if (dir is null)
				return Array.Empty<string>();
			return from f in dir.EnumerateFiles()
				   select Path.Join(path, f);
		}

		public IEnumerable<string> AllFiles() {
			foreach (var (f, _) in Files) {
				yield return f;
			}
			foreach (var (p, d) in Directories) {
				foreach (var f in d.AllFiles()) {
					yield return p + Path.DirectorySeparatorChar + f;
				}
			}
		}

		public void WriteToStream(string path, TextWriter writer) {
			foreach (var (f, i) in Files) {
				writer.WriteLine("{0}{2}{1}", path, f, Path.DirectorySeparatorChar);
				writer.WriteLine(i.ToSavableString());
			}
			foreach (var (p, d) in Directories) {
				d.WriteToStream(path + Path.DirectorySeparatorChar + p, writer);
			}
		}
	}

	public class IndexingFileSystemProvider : IFileSystemProvider {
		public IFileSystemProvider FileSystem { get; }
		public IFileInfoProvider FileInfoProvider { get; }

		public FileIndex Index { get; }

		public string IndexFilePath { get; }

		public const string DefaultIndexFilePath = ".index";

		public IndexingFileSystemProvider(IFileSystemProvider fileSystem, IFileInfoProvider fileInfoProvider, string indexFilePath, bool dontReadFile = false) {
			FileSystem = fileSystem;
			IndexFilePath = GetFullPath(indexFilePath);
			FileInfoProvider = fileInfoProvider;

			if (dontReadFile || !FileExists(IndexFilePath)) {
				Index = new FileIndex(GetAlbumDirectory());
				WriteIndexFile();
			} else {
				Index = ReadIndexFile();
			}
		}

		public IndexingFileSystemProvider(IFileSystemProvider fileSystem, IFileInfoProvider fileInfoProvider, string metaDirectory,
			string indexFile = DefaultIndexFilePath, bool dontReadFile = false) :
			this(fileSystem, fileInfoProvider, Path.Combine(metaDirectory, indexFile), dontReadFile) { }

		public IndexingFileSystemProvider(IFileSystemProvider fileSystem, IFileInfoProvider fileInfoProvider, bool dontReadFile = false) :
			this(fileSystem, fileInfoProvider, metaDirectory: fileSystem.GetAlbumDirectory(), dontReadFile: dontReadFile) { }

		public FileIndex ReadIndexFile() {
			var file = ReadText(IndexFilePath);
			var res = FileIndex.ReadFromStream(this, file);
			file.Close();
			return res;
		}

		public void WriteIndexFile() {
			var file = WriteText(IndexFilePath);
			Index.WriteToStream(file);
			file.Close();
		}

		public void UpdateFile(string fullPath) {
			var relPath = GetRelativePath(GetAlbumDirectory(), fullPath);
			if (!FileExists(fullPath)) {
				Index.SetFileInfo(relPath, null);
			} else {
				var info = FileInfoProvider.GetInfo(fullPath, FileSystem);
				Index.SetFileInfo(relPath, info, true);
			}
		}

		public void CopyFile(string srcPath, string destPath, bool overwrite = false) {
			FileSystem.CopyFile(srcPath, destPath, overwrite);
			UpdateFile(destPath);
		}

		public void CopyFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false) {
			FileSystem.CopyFileCreatingDirectories(srcPath, destPath, overwrite);
			UpdateFile(destPath);
		}

		public void CreateDirectory(string path) {
			FileSystem.CreateDirectory(path);
		}

		public void DeleteFile(string fullPath) {
			FileSystem.DeleteFile(fullPath);
			UpdateFile(fullPath);
		}

		public bool DirectoryExists(string fullPath) {
			return FileSystem.DirectoryExists(fullPath);
		}

		public void EndTransaction() {
			FileSystem.EndTransaction();
			WriteIndexFile();
		}

		public IEnumerable<string> EnumerateDirectories(string fullPath) {
			var p = GetRelativePath(GetAlbumDirectory(), fullPath);
			if (Index.DirectoryExists(p))
				return Index.EnumerateDirectories(p);
			return FileSystem.EnumerateDirectories(fullPath);
		}

		public IEnumerable<string> EnumerateFiles(string fullPath) {
			var p = GetRelativePath(GetAlbumDirectory(), fullPath);
			if (Index.DirectoryExists(p))
				return Index.EnumerateFiles(p);
			return FileSystem.EnumerateFiles(fullPath);
		}

		public bool FileExists(string fullPath) {
			return FileSystem.FileExists(fullPath);
		}

		public string GetAlbumDirectory() {
			return FileSystem.GetAlbumDirectory();
		}

		public FileAttributes GetFileAttributes(string fullPath) {
			return FileSystem.GetFileAttributes(fullPath);
		}

		public DateTime GetFileCreation(string fullPath) {
			return Index.GetFileInfo(fullPath)?.TrueFileCreation ?? FileSystem.GetFileCreation(fullPath);
		}

		public IReadOnlyList<MetadataExtractor.Directory> GetFileInfo(string fullPath) {
			return FileSystem.GetFileInfo(fullPath);
		}

		public DateTime GetFileModification(string fullPath) {
			return Index.GetFileInfo(fullPath)?.TrueFileModification ?? FileSystem.GetFileModification(fullPath);
		}

		public string GetFullPath(string relPath) {
			return FileSystem.GetFullPath(relPath);
		}

		public string GetFullPathAlbum(string pathInAlbum) {
			return FileSystem.GetFullPathAlbum(pathInAlbum);
		}

		public string GetRelativePath(string relativeTo, string path) {
			return FileSystem.GetRelativePath(relativeTo, path);
		}

		public void MoveFile(string srcPath, string destPath, bool overwrite = false) {
			FileSystem.MoveFile(srcPath, destPath, overwrite);
			UpdateFile(srcPath);
			UpdateFile(destPath);
		}

		public void MoveFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false) {
			FileSystem.MoveFileCreatingDirectories(srcPath, destPath, overwrite);
			UpdateFile(srcPath);
			UpdateFile(destPath);
		}

		public void NewTransaction(string? info = "") {
			FileSystem.NewTransaction(info);
		}

		public Stream OpenFile(string fullPath) {
			return FileSystem.OpenFile(fullPath);
		}

		public StreamReader ReadText(string fullPath) {
			return FileSystem.ReadText(fullPath);
		}

		public IEnumerable<FileSystemTransaction> ReadUndoFile(bool redo = false) {
			return FileSystem.ReadUndoFile(redo);
		}

		public FileSystemTransaction? Redo(ILogger logger) {
			return FileSystem.Undo(logger);
		}

		public void SetFileAttributes(string fullPath, FileAttributes attributes) {
			FileSystem.SetFileAttributes(fullPath, attributes);
		}

		public void SetFileCreation(string fullPath, DateTime creationDate) {
			FileSystem.SetFileCreation(fullPath, creationDate);
			UpdateFile(fullPath);
		}

		public void SetFileModification(string fullPath, DateTime modificationDate) {
			FileSystem.SetFileModification(fullPath, modificationDate);
			UpdateFile(fullPath);
		}

		public FileSystemTransaction? Undo(ILogger logger) {
			return FileSystem.Undo(logger);
		}

		public TextWriter WriteText(string fullPath, bool append = false) {
			return FileSystem.WriteText(fullPath, append);
		}
	}
}
