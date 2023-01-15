namespace AlbumLibrary {
	public interface IImportListProvider {
		public IEnumerable<ImportItem> GetImportItems(IFileSystemProvider fileSystem, IEnumerable<string> importFilePaths,
			IFileInfoProvider fileInfoProvider, IFileNameProvider fileNameProvider, IErrorHandler errorHandler);

		public ImportList GetImportList(IFileSystemProvider fileSystem, IEnumerable<string> importFilePaths,
			IFileInfoProvider fileInfoProvider, IFileNameProvider fileNameProvider, IErrorHandler errorHandler) {
			return new ImportList(GetImportItems(fileSystem, importFilePaths, fileInfoProvider, fileNameProvider, errorHandler));
		}

		public IEnumerable<ImportItem> GetImportItems(IFileSystemProvider fileSystem, IImportFilePathProvider importFilePathProvider,
			IFileInfoProvider fileInfoProvider, IFileNameProvider fileNameProvider, IErrorHandler errorHandler) {
			return GetImportItems(fileSystem, importFilePathProvider.GetFilePaths(fileSystem, errorHandler), fileInfoProvider,
				fileNameProvider, errorHandler);
		}

		public ImportList GetImportList(IFileSystemProvider fileSystem, IImportFilePathProvider importFilePathProvider,
			IFileInfoProvider fileInfoProvider, IFileNameProvider fileNameProvider, IErrorHandler errorHandler) {
			return new ImportList(GetImportItems(fileSystem, importFilePathProvider, fileInfoProvider, fileNameProvider, errorHandler));
		}
	}

	public class ImportListProvider : IImportListProvider {
		public IEnumerable<ImportItem> GetImportItems(IFileSystemProvider fileSystem, IEnumerable<string> importFilePaths,
			IFileInfoProvider fileInfoProvider, IFileNameProvider fileNameProvider, IErrorHandler errorHandler) {
			foreach (var file in importFilePaths) {
				var info = fileInfoProvider.GetInfo(file, fileSystem);
				ImportItem? val = null;
				try {
					var name = fileNameProvider.GetFileName(info);
					val = new ImportItem(info.OriginalFilePath, fileSystem.GetFullPathAlbum(name));
				} catch (CancelFileCopyException) {
					val = new ImportItem(info.OriginalFilePath, null);
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
		public string DestinationPath { get; }
		public bool Cancelled { get; }

		public ImportItem(string sourcePath, string? destinationPath) {
			SourcePath = sourcePath;
			DestinationPath = destinationPath ?? "";
			Cancelled = destinationPath is null;
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
