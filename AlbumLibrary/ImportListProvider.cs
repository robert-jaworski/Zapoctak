namespace AlbumLibrary {
	public interface IImportListProvider {
		public IEnumerable<ImportItem> GetImportItems(IFileSystemProvider fileSystem, IEnumerable<string> importFilePaths, IErrorHandler errorHandler);

		public ImportList GetImportList(IFileSystemProvider fileSystem, IEnumerable<string> importFilePaths, IErrorHandler errorHandler) {
			return new ImportList(GetImportItems(fileSystem, importFilePaths, errorHandler));
		}

		public IEnumerable<ImportItem> GetImportItems(IFileSystemProvider fileSystem, IImportFilePathProvider importFilePathProvider, IErrorHandler errorHandler) {
			return GetImportItems(fileSystem, importFilePathProvider.GetFilePaths(fileSystem, errorHandler), errorHandler);
		}

		public ImportList GetImportList(IFileSystemProvider fileSystem, IImportFilePathProvider importFilePathProvider, IErrorHandler errorHandler) {
			return new ImportList(GetImportItems(fileSystem, importFilePathProvider, errorHandler));
		}
	}

	public class ImportListProvider : IImportListProvider {
		protected IFileInfoProvider FileInfoProvider { get; }
		protected IFileNameProvider FileNameProvider { get; }

		public ImportListProvider(IFileInfoProvider fileInfoProvider, IFileNameProvider fileNameProvider) {
			FileInfoProvider = fileInfoProvider;
			FileNameProvider = fileNameProvider;
		}

		public IEnumerable<ImportItem> GetImportItems(IFileSystemProvider fileSystem, IEnumerable<string> importFilePaths, IErrorHandler errorHandler) {
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
				}
				if (val is not null)
					yield return val;
			}
		}
	}

	public class ImportItem {
		public string SourcePath { get; }
		public string DestinationPath { get; protected set; }
		public bool Cancelled { get; protected set; }
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

		public ImportItem Cancel() {
			Cancelled = true;
			return this;
		}

		public ImportItem Copy() {
			return new ImportItem(Info, DestinationPath) {
				Cancelled = Cancelled
			};
		}
	}

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
