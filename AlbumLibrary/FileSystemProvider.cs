using System.IO;

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

		FileAttributes GetFileAttributes(string fullPath);
		void SetFileAttributes(string fullPath, FileAttributes attributes);

		DateTime GetFileCreation(string fullPath);
		DateTime GetFileModification(string fullPath);

		void SetFileCreation(string fullPath, DateTime creationDate);
		void SetFileModification(string fullPath, DateTime modificationDate);

		Stream OpenFile(string fullPath);
		public IReadOnlyList<MetadataExtractor.Directory> GetFileInfo(string fullPath);

		StreamReader ReadText(string fullPath);
		TextWriter WriteText(string fullPath, bool append = false);

		public void CreateDirectory(string path);

		public void CopyFile(string srcPath, string destPath, bool overwrite = false);
		public void CopyFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false);

		public void MoveFile(string srcPath, string destPath, bool overwrite = false);
		public void MoveFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false);

		public void DeleteFile(string fullPath);

		public void NewTransaction(string? info = "");
		public void EndTransaction();
		public FileSystemTransaction? Undo(ILogger logger);
		public FileSystemTransaction? Redo(ILogger logger);
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

		public DateTime GetFileCreation(string fullPath) {
			return File.GetCreationTime(fullPath);
		}

		public DateTime GetFileModification(string fullPath) {
			return File.GetLastWriteTime(fullPath);
		}

		public FileAttributes GetFileAttributes(string fullPath) {
			return File.GetAttributes(fullPath);
		}

		public void SetFileAttributes(string fullPath, FileAttributes attributes) {
			File.SetAttributes(fullPath, attributes);
		}

		public Stream OpenFile(string fullPath) {
			return File.OpenRead(fullPath);
		}

		public IReadOnlyList<MetadataExtractor.Directory> GetFileInfo(string fullPath) {
			return MetadataExtractor.ImageMetadataReader.ReadMetadata(fullPath);
		}

		public StreamReader ReadText(string fullPath) {
			return File.OpenText(fullPath);
		}

		public TextWriter WriteText(string fullPath, bool append = false) {
			return new StreamWriter(fullPath, append);
		}

		public string GetAlbumDirectory() {
			return AlbumDirectory;
		}

		public string GetFullPathAlbum(string pathInAlbum) {
			return GetFullPath(Path.Combine(AlbumDirectory, pathInAlbum));
		}

		public string GetRelativePath(string relativeTo, string path) {
			return Path.GetRelativePath(relativeTo, path);
		}

		public void CreateDirectory(string path) {
			Directory.CreateDirectory(path);
		}

		public void CopyFile(string srcPath, string destPath, bool overwrite = false) {
			File.Copy(srcPath, destPath, overwrite);
		}

		public void CopyFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false) {
			var dir = Path.GetDirectoryName(destPath);
			if (dir is not null)
				CreateDirectory(dir);
			CopyFile(srcPath, destPath, overwrite);
		}

		public void MoveFile(string srcPath, string destPath, bool overwrite = false) {
			File.Move(srcPath, destPath, overwrite);
		}

		public void MoveFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false) {
			var dir = Path.GetDirectoryName(destPath);
			if (dir is not null)
				CreateDirectory(dir);
			MoveFile(srcPath, destPath, overwrite);
		}

		public void SetFileCreation(string fullPath, DateTime creationDate) {
			File.SetCreationTime(fullPath, creationDate);
		}

		public void SetFileModification(string fullPath, DateTime modificationDate) {
			File.SetLastWriteTime(fullPath, modificationDate);
		}

		public void DeleteFile(string fullPath) {
			File.Delete(fullPath);
		}

		public void NewTransaction(string? info = "") { }

		public void EndTransaction() { }

		public FileSystemTransaction? Undo(ILogger logger) {
			throw new NotImplementedException();
		}

		public FileSystemTransaction? Redo(ILogger logger) {
			throw new NotImplementedException();
		}
	}

	public class UndoFileSystemProvider : IFileSystemProvider {
		protected IFileSystemProvider FileSystem { get; }

		protected string UndoFilePath { get; }
		protected string RedoFilePath { get; }
		protected string TrashDirectory { get; }

		protected Random RNG { get;  } = new();

		protected TextWriter? UndoFile { get; set; }

		public const int TypeWidth = 16;

		public UndoFileSystemProvider(IFileSystemProvider fileSystem, string undoFilePath, string redoFilePath, string trashDirectory) {
			FileSystem = fileSystem;
			UndoFilePath = GetFullPath(undoFilePath);
			RedoFilePath = GetFullPath(redoFilePath);
			TrashDirectory = GetFullPath(trashDirectory);

			if (!FileExists(UndoFilePath)) {
				OpenUndoFile();
				CloseUndoFile();
			}
			if (!FileExists(RedoFilePath)) {
				OpenUndoFile(true);
				CloseUndoFile();
			}
			if (!DirectoryExists(TrashDirectory)) {
				CreateDirectory(TrashDirectory);
			}
			SetFileAttributes(UndoFilePath, FileAttributes.Hidden | GetFileAttributes(UndoFilePath));
			SetFileAttributes(RedoFilePath, FileAttributes.Hidden | GetFileAttributes(RedoFilePath));
			SetFileAttributes(TrashDirectory, FileAttributes.Hidden | GetFileAttributes(TrashDirectory));
		}

		public UndoFileSystemProvider(IFileSystemProvider fileSystem, string metaDirectory) :
			this(fileSystem, Path.Combine(metaDirectory, ".undo"), Path.Combine(metaDirectory, ".redo"), Path.Combine(metaDirectory, ".trash")) { }

		public UndoFileSystemProvider(IFileSystemProvider fileSystem) : this(fileSystem, fileSystem.GetAlbumDirectory()) { }

		protected void OpenUndoFile(bool redo = false) {
			if (UndoFile is not null)
				throw new InvalidOperationException();
			UndoFile = WriteText(redo ? RedoFilePath : UndoFilePath, append: true);
		}

		protected void CloseUndoFile() {
			if (UndoFile is null)
				throw new InvalidOperationException();
			UndoFile.Dispose();
			UndoFile = null;
		}

		protected string GetTrashBinFile(string extension) {
			CreateDirectory(TrashDirectory);
			var name = RNG.Next().ToString("X8");
			var path = Path.Combine(TrashDirectory, name + extension);
			while (FileExists(path)) {
				name = RNG.Next().ToString("X8");
				path = Path.Combine(TrashDirectory, name + extension);
			}
			return path;
		}

		protected void Operation(string operation, string arg) {
			if (UndoFile is null)
				throw new InvalidOperationException();
			UndoFile.WriteLine($"{{0,-{TypeWidth}}}{{1}}", operation, arg);
		}

		protected void Operation(string operation, string formatString, params object[] args) {
			Operation(operation, string.Format(formatString, args: args));
		}

		protected void DoMove(string srcPath, string destPath) {
			if (UndoFile is null)
				throw new InvalidOperationException();
			FileSystem.MoveFile(srcPath, destPath, false);
			Operation("Move", srcPath);
			Operation("To", destPath);
		}

		protected void DoCopy(string srcPath, string destPath) {
			if (UndoFile is null)
				throw new InvalidOperationException();
			FileSystem.CopyFile(srcPath, destPath, false);
			Operation("Copy", srcPath);
			Operation("To", destPath);
		}

		public string GetUndoFilePath() {
			return GetFullPath(UndoFilePath);
		}

		public string GetRedoFilePath() {
			return GetFullPath(RedoFilePath);
		}

		public string GetTrashDirectoryPath() {
			return GetFullPath(TrashDirectory);
		}

		public bool DirectoryExists(string fullPath) {
			return FileSystem.DirectoryExists(fullPath);
		}

		public IEnumerable<string> EnumerateFiles(string fullPath) {
			return FileSystem.EnumerateFiles(fullPath);
		}

		public IEnumerable<string> EnumerateDirectories(string fullPath) {
			return FileSystem.EnumerateDirectories(fullPath);
		}

		public bool FileExists(string fullPath) {
			return FileSystem.FileExists(fullPath);
		}

		public string GetFullPath(string relPath) {
			return FileSystem.GetFullPath(relPath);
		}

		public DateTime GetFileCreation(string fullPath) {
			return FileSystem.GetFileCreation(fullPath);
		}

		public DateTime GetFileModification(string fullPath) {
			return FileSystem.GetFileModification(fullPath);
		}

		public FileAttributes GetFileAttributes(string fullPath) {
			return FileSystem.GetFileAttributes(fullPath);
		}

		public void SetFileAttributes(string fullPath, FileAttributes attributes) {
			FileSystem.SetFileAttributes(fullPath, attributes);
		}

		public Stream OpenFile(string fullPath) {
			return FileSystem.OpenFile(fullPath);
		}

		public IReadOnlyList<MetadataExtractor.Directory> GetFileInfo(string fullPath) {
			return FileSystem.GetFileInfo(fullPath);
		}

		public StreamReader ReadText(string fullPath) {
			return FileSystem.ReadText(fullPath);
		}

		public TextWriter WriteText(string fullPath, bool append = false) {
			return FileSystem.WriteText(fullPath, append);
		}

		public string GetAlbumDirectory() {
			return FileSystem.GetAlbumDirectory();
		}

		public string GetFullPathAlbum(string pathInAlbum) {
			return FileSystem.GetFullPathAlbum(pathInAlbum);
		}

		public string GetRelativePath(string relativeTo, string path) {
			return FileSystem.GetRelativePath(relativeTo, path);
		}

		public void CreateDirectory(string path) {
			FileSystem.CreateDirectory(path);
		}

		public void CopyFile(string srcPath, string destPath, bool overwrite = false) {
			if (UndoFile is null)
				throw new InvalidOperationException();
			if (FileExists(destPath)) {
				if (!overwrite)
					FileSystem.CopyFile(srcPath, destPath, overwrite);
				DoMove(destPath, GetTrashBinFile(Path.GetExtension(destPath)));
			}
			DoCopy(srcPath, destPath);
		}

		public void CopyFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false) {
			var dir = Path.GetDirectoryName(destPath);
			if (dir is not null)
				CreateDirectory(dir);
			CopyFile(srcPath, destPath, overwrite);
		}

		public void MoveFile(string srcPath, string destPath, bool overwrite = false) {
			if (UndoFile is null)
				throw new InvalidOperationException();
			if (FileExists(destPath)) {
				if (!overwrite)
					FileSystem.MoveFile(srcPath, destPath, overwrite);
				DoMove(destPath, GetTrashBinFile(Path.GetExtension(destPath)));
			}
			DoMove(srcPath, destPath);
		}

		public void MoveFileCreatingDirectories(string srcPath, string destPath, bool overwrite = false) {
			var dir = Path.GetDirectoryName(destPath);
			if (dir is not null)
				CreateDirectory(dir);
			MoveFile(srcPath, destPath, overwrite);
		}

		public void SetFileCreation(string fullPath, DateTime creationDate) {
			if (UndoFile is null)
				throw new InvalidOperationException();
			var prev = GetFileCreation(fullPath);
			FileSystem.SetFileCreation(fullPath, creationDate);
			Operation("Creation", "{0:o} {1:o}", prev, creationDate);
			Operation("Of", fullPath);
		}

		public void SetFileModification(string fullPath, DateTime modificationDate) {
			if (UndoFile is null)
				throw new InvalidOperationException();
			var prev = GetFileModification(fullPath);
			FileSystem.SetFileModification(fullPath, modificationDate);
			Operation("Modification", "{0:o} {1:o}", prev, modificationDate);
			Operation("Of", fullPath);
		}

		public void DeleteFile(string fullPath) {
			DoMove(fullPath, GetTrashBinFile(Path.GetExtension(fullPath)));
		}

		protected bool IsFileEmpty(string fullPath) {
			if (!FileExists(fullPath))
				return true;
			using var file = ReadText(fullPath);
			return file.EndOfStream;
		}

		public void NewTransaction(string? info = "") {
			if (UndoFile is not null)
				throw new InvalidOperationException();
			if (!IsFileEmpty(RedoFilePath))
				throw new InvalidOperationException("Redo file is not empty");
			OpenUndoFile();

			if (UndoFile is null)
				throw new InvalidOperationException();
			Operation("Transaction", "{0} {1:o}", info is null, DateTime.Now);
			if (info is not null)
				Operation("Info", info);
			else
				Operation("NoInfo", "");
		}

		public void EndTransaction() {
			if (UndoFile is null)
				throw new InvalidOperationException();
			UndoFile.WriteLine();
			CloseUndoFile();
		}

		public FileSystemTransaction? Undo(ILogger logger) {
			if (UndoFile is not null)
				throw new InvalidOperationException();

			var transactions = ReadUndoFile().ToList();
			if (transactions.Count == 0)
				return null;

			UndoFile = WriteText(RedoFilePath, append: true);
			Operation("Transaction", "{0} {1:o}", false, DateTime.Now);
			Operation("Info", "undo");
			transactions[^1].Undo(this, logger);
			UndoFile.WriteLine();
			UndoFile.Dispose();
			UndoFile = null;

			FileSystem.DeleteFile(UndoFilePath);
			var file = WriteText(UndoFilePath, append: false);
			foreach (var t in transactions.Take(transactions.Count - 1)) {
				t.WriteToStream(file);
			}

			file.Close();
			file.Dispose();
			SetFileAttributes(UndoFilePath, FileAttributes.Hidden | GetFileAttributes(UndoFilePath));

			return transactions[^1];
		}

		public FileSystemTransaction? Redo(ILogger logger) {
			if (UndoFile is not null)
				throw new InvalidOperationException();

			var transactions = ReadUndoFile(true).ToList();
			if (transactions.Count == 0)
				return null;

			UndoFile = WriteText(UndoFilePath, append: true);
			Operation("Transaction", "{0} {1:o}", false, DateTime.Now);
			Operation("Info", "redo");
			transactions[^1].Undo(this, logger);
			UndoFile.WriteLine();
			UndoFile.Dispose();
			UndoFile = null;

			FileSystem.DeleteFile(RedoFilePath);
			var file = WriteText(RedoFilePath, append: false);
			foreach (var t in transactions.Take(transactions.Count - 1)) {
				t.WriteToStream(file);
			}

			file.Close();
			file.Dispose();
			SetFileAttributes(RedoFilePath, FileAttributes.Hidden | GetFileAttributes(RedoFilePath));

			return transactions[^1];
		}

		public IEnumerable<FileSystemTransaction> ReadUndoFile(bool redo = false) {
			var path = redo ? RedoFilePath : UndoFilePath;
			if (!File.Exists(path))
				yield break;

			var file = ReadText(path);
			var prev = FileSystemTransaction.ReadFromStream(file);
			if (prev is null) {
				file.Close();
				file.Dispose();
				yield break;
			}

			while (!file.EndOfStream) {
				var curr = FileSystemTransaction.ReadFromStream(file);
				if (curr is null) {
					yield return prev;
					file.Close();
					file.Dispose();
					yield break;
				} else {
					if (curr.ShouldJoin)
						prev.Join(curr);
					else {
						yield return prev;
						prev = curr;
					}
				}
			}
			yield return prev;
			file.Close();
			file.Dispose();
			yield break;
		}
	}

	public class FileSystemTransaction {
		protected List<FileSystemAction> actions;

		public IReadOnlyList<FileSystemAction> Actions => actions;
		public DateTime Date { get; }
		public string Info { get; }
		public bool ShouldJoin { get; }

		public const string TransactionString	= "Transaction     ";
		public const string InfoString			= "Info            ";
		public const string NoInfoString		= "NoInfo          ";

		public FileSystemTransaction(List<FileSystemAction> actions, DateTime date, string info, bool shouldJoin) {
			this.actions = actions;
			Date = date;
			Info = info;
			ShouldJoin = shouldJoin;
		}

		public static FileSystemTransaction? ReadFromStream(StreamReader stream) {
			var firstLine = stream.ReadLine();
			if (firstLine is null)
				return null;
			if (firstLine[..UndoFileSystemProvider.TypeWidth] != TransactionString)
				return null;

			var args = firstLine[UndoFileSystemProvider.TypeWidth..].Split(' ');
			var shouldJoin = bool.Parse(args[0]);
			var date = DateTime.Parse(args[1]);

			string info;
			var secondLine = stream.ReadLine();
			if (secondLine is null)
				return null;
			if (secondLine[..UndoFileSystemProvider.TypeWidth] == NoInfoString)
				info = "";
			else if (secondLine[..UndoFileSystemProvider.TypeWidth] == InfoString)
				info = secondLine[UndoFileSystemProvider.TypeWidth..];
			else
				return null;
				
			var actions = new List<FileSystemAction>();
			var action = FileSystemAction.ReadFromStream(stream);
			while (action is not null) {
				actions.Add(action);
				action = FileSystemAction.ReadFromStream(stream);
			}

			return new FileSystemTransaction(actions, date, info, shouldJoin);
		}

		public void WriteToStream(TextWriter writer) {
			writer.WriteLine("{0}{1} {2:o}", TransactionString, ShouldJoin, Date);
			writer.WriteLine("{0}{1}", InfoString, Info);
			foreach (var action in actions) {
				action.WriteToStream(writer);
			}
			writer.WriteLine();
		}

		public void Join(FileSystemTransaction transaction) {
			actions.AddRange(transaction.Actions);
		}

		public void Undo(IFileSystemProvider fileSystem, ILogger logger) {
			foreach (var action in actions.Reverse<FileSystemAction>()) {
				action.Undo(fileSystem, logger);
			}
		}
	}

	public abstract class FileSystemAction {
		public const string MoveString			= "Move            ";
		public const string CopyString			= "Copy            ";
		public const string ToPathString		= "To              ";
		public const string CreationString		= "Creation        ";
		public const string ModificationString	= "Modification    ";
		public const string OfPathString		= "Of              ";

		public static FileSystemAction? ReadFromStream(StreamReader stream) {
			var line = stream.ReadLine();
			if (string.IsNullOrEmpty(line))
				return null;
			var type = line[..UndoFileSystemProvider.TypeWidth];
			var arg = line[UndoFileSystemProvider.TypeWidth..];
			switch (type) {
			case MoveString:
				var line2 = stream.ReadLine();
				if (line2 is null)
					return null;
				var type2 = line2[..UndoFileSystemProvider.TypeWidth];
				var arg2 = line2[UndoFileSystemProvider.TypeWidth..];
				if (type2 != ToPathString)
					return null;

				return new FileSystemMoveAction(arg, arg2);
			case CopyString:
				line2 = stream.ReadLine();
				if (line2 is null)
					return null;
				type2 = line2[..UndoFileSystemProvider.TypeWidth];
				arg2 = line2[UndoFileSystemProvider.TypeWidth..];
				if (type2 != ToPathString)
					return null;

				return new FileSystemCopyAction(arg, arg2);
			case CreationString:
				line2 = stream.ReadLine();
				if (line2 is null)
					return null;
				type2 = line2[..UndoFileSystemProvider.TypeWidth];
				arg2 = line2[UndoFileSystemProvider.TypeWidth..];
				if (type2 != OfPathString)
					return null;

				var dates = arg.Split(' ').Select(DateTime.Parse).ToList();
				return new FileSystemCreationAction(arg2, dates[0], dates[1]);
			case ModificationString:
				line2 = stream.ReadLine();
				if (line2 is null)
					return null;
				type2 = line2[..UndoFileSystemProvider.TypeWidth];
				arg2 = line2[UndoFileSystemProvider.TypeWidth..];
				if (type2 != OfPathString)
					return null;

				dates = arg.Split(' ').Select(DateTime.Parse).ToList();
				return new FileSystemModificationAction(arg2, dates[0], dates[1]);
			default:
				return null;
			}
		}

		public abstract void WriteToStream(TextWriter writer);

		public abstract void Undo(IFileSystemProvider fileSystem, ILogger logger);
	}

	public class FileSystemMoveAction : FileSystemAction {
		public string FromPath { get; }
		public string ToPath { get; }

		public FileSystemMoveAction(string fromPath, string toPath) {
			FromPath = fromPath;
			ToPath = toPath;
		}

		public override void WriteToStream(TextWriter writer) {
			writer.WriteLine("{0}{1}", MoveString, FromPath);
			writer.WriteLine("{0}{1}", ToPathString, ToPath);
		}

		public override void Undo(IFileSystemProvider fileSystem, ILogger logger) {
			logger.WriteLine($"Move\t{logger.GetSuitablePath(ToPath, fileSystem)}\t -> {logger.GetSuitablePath(FromPath, fileSystem)}");
			fileSystem.MoveFile(ToPath, FromPath);
		}
	}

	public class FileSystemCopyAction : FileSystemAction {
		public string FromPath { get; }
		public string ToPath { get; }

		public FileSystemCopyAction(string fromPath, string toPath) {
			FromPath = fromPath;
			ToPath = toPath;
		}

		public override void WriteToStream(TextWriter writer) {
			writer.WriteLine("{0}{1}", CopyString, FromPath);
			writer.WriteLine("{0}{1}", ToPathString, ToPath);
		}

		public override void Undo(IFileSystemProvider fileSystem, ILogger logger) {
			logger.WriteLine($"Delete\t{logger.GetSuitablePath(ToPath, fileSystem)}");
			fileSystem.DeleteFile(ToPath);
		}
	}

	public class FileSystemCreationAction : FileSystemAction {
		public string Path { get; }
		public DateTime FromDate { get; }
		public DateTime ToDate { get; }

		public FileSystemCreationAction(string path, DateTime fromDate, DateTime toDate) {
			Path = path;
			FromDate = fromDate;
			ToDate = toDate;
		}

		public override void WriteToStream(TextWriter writer) {
			writer.WriteLine("{0}{1:o} {2:o}", CreationString, FromDate, ToDate);
			writer.WriteLine("{0}{1}", OfPathString, Path);
		}

		public override void Undo(IFileSystemProvider fileSystem, ILogger logger) {
			fileSystem.SetFileCreation(Path, FromDate);
		}
	}

	public class FileSystemModificationAction : FileSystemAction {
		public string Path { get; }
		public DateTime FromDate { get; }
		public DateTime ToDate { get; }

		public FileSystemModificationAction(string path, DateTime fromDate, DateTime toDate) {
			Path = path;
			FromDate = fromDate;
			ToDate = toDate;
		}

		public override void WriteToStream(TextWriter writer) {
			writer.WriteLine("{0}{1:o} {2:o}", ModificationString, FromDate, ToDate);
			writer.WriteLine("{0}{1}", OfPathString, Path);
		}

		public override void Undo(IFileSystemProvider fileSystem, ILogger logger) {
			fileSystem.SetFileModification(Path, FromDate);
		}
	}
}
