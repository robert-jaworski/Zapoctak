namespace AlbumLibrary {
	/// <summary>
	/// An interface for converting a list of file paths to a list of <see cref="ImportItem"/>.
	/// It can decide which files to process, assign them destinations and modify their info.
	/// </summary>
	public interface IImportListProvider {
		/// <summary>
		/// Convert the list of file paths to a list of <see cref="ImportItem"/>
		/// </summary>
		/// <param name="fileSystem"></param>
		/// <param name="importFilePaths">the file paths</param>
		/// <param name="errorHandler"></param>
		/// <returns>The list of <see cref="ImportItem"/> to process</returns>
		public IEnumerable<ImportItem> GetImportItems(IFileSystemProvider fileSystem, IEnumerable<string> importFilePaths, IErrorHandler errorHandler, ILogger logger);

		public ImportList GetImportList(IFileSystemProvider fileSystem, IEnumerable<string> importFilePaths, IErrorHandler errorHandler, ILogger logger) {
			return new ImportList(GetImportItems(fileSystem, importFilePaths, errorHandler, logger));
		}

		public IEnumerable<ImportItem> GetImportItems(IFileSystemProvider fileSystem, IImportFilePathProvider importFilePathProvider, IErrorHandler errorHandler, ILogger logger) {
			return GetImportItems(fileSystem, importFilePathProvider.GetFilePaths(fileSystem, errorHandler, logger), errorHandler, logger);
		}

		public ImportList GetImportList(IFileSystemProvider fileSystem, IImportFilePathProvider importFilePathProvider, IErrorHandler errorHandler, ILogger logger) {
			return new ImportList(GetImportItems(fileSystem, importFilePathProvider, errorHandler, logger));
		}
	}

	/// <summary>
	/// A simple <see cref="IImportListProvider"/> which simply goes through the list of file paths and
	/// assigns a name to each using a <see cref="IFileNameProvider"/>
	/// </summary>
	public class ImportListProvider : IImportListProvider {
		protected IFileInfoProvider FileInfoProvider { get; }
		protected IFileNameProvider FileNameProvider { get; }

		public ImportListProvider(IFileInfoProvider fileInfoProvider, IFileNameProvider fileNameProvider) {
			FileInfoProvider = fileInfoProvider;
			FileNameProvider = fileNameProvider;
		}

		public IEnumerable<ImportItem> GetImportItems(IFileSystemProvider fileSystem, IEnumerable<string> importFilePaths, IErrorHandler errorHandler, ILogger logger) {
			foreach (var file in importFilePaths) {
				var info = FileInfoProvider.GetInfo(file, fileSystem);
				ImportItem? val = null;
				try {
					var name = FileNameProvider.GetFileName(info);
					val = new ImportItem(info, fileSystem.GetFullPathAlbum(name));
				} catch (CancelFileCopyException) {
					val = new ImportItem(info, null);
				} catch (InvalidFileNameTemplateException e) {
					errorHandler.Error(e.Message);
					// Console.Error.WriteLine(e);
				}
				if (val is not null)
					yield return val;
			}
		}
	}

	/// <summary>
	/// A class which bundles all information needed to import a file into the album.
	/// However, it can be used for other things beside importing.
	/// </summary>
	public class ImportItem {
		/// <summary>
		/// The full path to the file to import.
		/// </summary>
		public string SourcePath { get; }
		/// <summary>
		/// The full path of the destination (where the file should be copied, moved etc.)
		/// </summary>
		public string DestinationPath { get; protected set; }
		/// <summary>
		/// Whether the importing of this file has been cancelled.
		/// </summary>
		public bool Cancelled { get; protected set; }
		/// <summary>
		/// Whether the importing of this file has been cancelled because it already exists.
		/// </summary>
		public bool IsDuplicate { get; protected set; }
		/// <summary>
		/// Whether the destination should be overwritten.
		/// </summary>
		public bool Overwrite { get; protected set; }
		/// <summary>
		/// The information about the original file.
		/// </summary>
		public FileInfo Info { get; }

		public ImportItem(FileInfo info, string? destinationPath) {
			SourcePath = info.OriginalFilePath;
			DestinationPath = destinationPath ?? "";
			Cancelled = destinationPath is null;
			Info = info;
		}

		public ImportItem ChangeDestination(string destinationPath) {
			DestinationPath = destinationPath;
			return this;
		}

		/// <summary>
		/// Cancels the importing of this file.
		/// </summary>
		/// <returns>Itself.</returns>
		public ImportItem Cancel() {
			Cancelled = true;
			return this;
		}

		/// <summary>
		/// Cancels the importing of this file because it already exists.
		/// </summary>
		/// <returns>Itself.</returns>
		public ImportItem MarkDuplicate() {
			IsDuplicate = true;
			return Cancel();
		}

		/// <summary>
		/// Sets <see cref="Overwrite"/> to <see langword="true"/>.
		/// </summary>
		/// <returns>Itself.</returns>
		public ImportItem MakeOverwrite() {
			Overwrite = true;
			return this;
		}

		/// <summary>
		/// Copies all information.
		/// </summary>
		/// <returns></returns>
		public ImportItem Copy() {
			return new ImportItem(Info, DestinationPath) {
				Cancelled = Cancelled,
				IsDuplicate = IsDuplicate,
				Overwrite = Overwrite
			};
		}
	}

	/// <summary>
	/// A list of items to import.
	/// </summary>
	public class ImportList {
		protected List<ImportItem> allItems;
		protected List<ImportItem> importItems;
		protected List<ImportItem> cancelledItems;

		public IReadOnlyList<ImportItem> AllItems => allItems;
		public IReadOnlyList<ImportItem> ImportItems => importItems;
		public IReadOnlyList<ImportItem> CancelledItems => cancelledItems;

		public ImportList(IEnumerable<ImportItem> items) {
			allItems = items.ToList();
			importItems = allItems.Where(i => !i.Cancelled).ToList();
			cancelledItems = allItems.Where(i => i.Cancelled).ToList();
		}
	}
}
