namespace AlbumLibrary {
	public interface IFileImporter {
		public IEnumerable<ImportItem> ImportItems(IEnumerable<ImportItem> importItems, IErrorHandler errorHandler, ILogger logger);
	}

	public class FileImporter : IFileImporter {
		protected IFileSystemProvider FileSystem { get; set; }
		protected IDuplicateResolver DuplicateResolver { get; set; }

		public FileImporter(IFileSystemProvider fileSystem, IDuplicateResolver duplicateResolver) {
			FileSystem = fileSystem;
			DuplicateResolver = duplicateResolver;
		}

		public IEnumerable<ImportItem> ImportItems(IEnumerable<ImportItem> importItems, IErrorHandler errorHandler, ILogger logger) {
			var imported = new List<ImportItem>();
			foreach (var item in importItems) {
				if (!item.Cancelled) {
					var importedItem = item;
					logger.Write($"{logger.GetSuitablePath(item.SourcePath, FileSystem)}\t-> {logger.GetSuitablePath(item.DestinationPath, FileSystem)}");
					if (FileSystem.FileExists(item.DestinationPath)) {
						importedItem = DuplicateResolver.ResolveDuplicate(importedItem, FileSystem);
						if (importedItem.DestinationPath != item.DestinationPath)
							logger.Write($"\t=> {logger.GetSuitablePath(importedItem.DestinationPath, FileSystem)}");
						if (importedItem.Cancelled) {
							if (importedItem.IsDuplicate)
								logger.WriteLine("\t:  Cancelled (duplicate)");
							else
								logger.WriteLine("\t:  Cancelled");
							continue;
						}
					}

					try {
						FileSystem.CopyFileCreatingDirectories(importedItem.SourcePath, importedItem.DestinationPath);
						FileSystem.SetFileCreation(importedItem.DestinationPath, importedItem.Info.SuitableDateTime);
						logger.WriteLine("\t:  Success");
						imported.Add(importedItem);
					} catch (Exception e) {
						logger.WriteLine("\t:  Error!");
						logger.WriteLine(e.Message);
					}
				}
			}
			return imported;
		}
	}
}
