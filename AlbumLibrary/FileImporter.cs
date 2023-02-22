namespace AlbumLibrary {
	/// <summary>
	/// An interface for importing (or otherwise processing) given files.
	/// </summary>
	public interface IFileImporter {
		/// <summary>
		/// Processes given <see cref="ImportItem"/>
		/// </summary>
		/// <param name="importItems"></param>
		/// <param name="errorHandler"></param>
		/// <param name="logger"></param>
		/// <returns>A list of successfully</returns>
		public IEnumerable<ImportItem> ImportItems(IEnumerable<ImportItem> importItems, IErrorHandler errorHandler, ILogger logger);
	}

	/// <summary>
	/// An <see cref="IFileImporter"/> which copies the files to the destination.
	/// </summary>
	public class FileCopyImporter : IFileImporter {
		protected IFileSystemProvider FileSystem { get; }
		protected IDuplicateResolver DuplicateResolver { get; }
		protected string? Operation { get; }

		public FileCopyImporter(IFileSystemProvider fileSystem, IDuplicateResolver duplicateResolver, string? operation) {
			FileSystem = fileSystem;
			DuplicateResolver = duplicateResolver;
			Operation = operation;
		}

		public IEnumerable<ImportItem> ImportItems(IEnumerable<ImportItem> importItems, IErrorHandler errorHandler, ILogger logger) {
			FileSystem.NewTransaction(Operation);
			var imported = new List<ImportItem>();
			foreach (var item in importItems) {
				if (!item.Cancelled) {
					var importedItem = item;
					logger.Write($"Copy\t{logger.GetSuitablePath(item.SourcePath, FileSystem)}\t-> {logger.GetSuitablePath(item.DestinationPath, FileSystem)}");
					if (FileSystem.FileExists(item.DestinationPath)) {
						var prevDest = item.DestinationPath;
						importedItem = DuplicateResolver.ResolveDuplicate(importedItem, FileSystem);
						if (importedItem.DestinationPath != prevDest)
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
						FileSystem.CopyFileCreatingDirectories(importedItem.SourcePath, importedItem.DestinationPath, importedItem.Overwrite);
						FileSystem.SetFileCreation(importedItem.DestinationPath, importedItem.Info.SuitableDateTime);
						FileSystem.SetFileModification(importedItem.DestinationPath, DateTime.Now);

						if (importedItem.Overwrite)
							logger.WriteLine("\t:  Success (overwritten)");
						else
							logger.WriteLine("\t:  Success");
						imported.Add(importedItem);
					} catch (Exception e) {
						logger.WriteLine("\t:  Error!");
						errorHandler.Error(e.Message);
						// Console.Error.WriteLine(e);
					}
				}
			}
			FileSystem.EndTransaction();
			return imported;
		}
	}

	/// <summary>
	/// An <see cref="IFileImporter"/> which moves the files to the destination.
	/// </summary>
	public class FileMoveImporter : IFileImporter {
		protected IFileSystemProvider FileSystem { get; }
		protected IDuplicateResolver DuplicateResolver { get; }
		protected string? Operation { get; }

		public FileMoveImporter(IFileSystemProvider fileSystem, IDuplicateResolver duplicateResolver, string? operation) {
			FileSystem = fileSystem;
			DuplicateResolver = duplicateResolver;
			Operation = operation;
		}

		public IEnumerable<ImportItem> ImportItems(IEnumerable<ImportItem> importItems, IErrorHandler errorHandler, ILogger logger) {
			FileSystem.NewTransaction(Operation);
			var imported = new List<ImportItem>();
			foreach (var item in importItems) {
				if (!item.Cancelled) {
					var importedItem = item;
					logger.Write($"Move\t{logger.GetSuitablePath(item.SourcePath, FileSystem)}\t-> {logger.GetSuitablePath(item.DestinationPath, FileSystem)}");
					if (FileSystem.FileExists(item.DestinationPath)) {
						var prevDest = item.DestinationPath;
						importedItem = DuplicateResolver.ResolveDuplicate(importedItem, FileSystem);
						if (importedItem.DestinationPath != prevDest)
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
						FileSystem.MoveFileCreatingDirectories(importedItem.SourcePath, importedItem.DestinationPath, importedItem.Overwrite);
						FileSystem.SetFileCreation(importedItem.DestinationPath, importedItem.Info.SuitableDateTime);
						FileSystem.SetFileModification(importedItem.DestinationPath, DateTime.Now);

						if (importedItem.Overwrite)
							logger.WriteLine("\t:  Success (moved, overwritten)");
						else
							logger.WriteLine("\t:  Success (moved)");
						imported.Add(importedItem);
					} catch (Exception e) {
						logger.WriteLine("\t:  Error!");
						errorHandler.Error(e.Message);
						// Console.Error.WriteLine(e);
					}
				}
			}
			FileSystem.EndTransaction();
			return imported;
		}
	}

	/// <summary>
	/// An <see cref="IFileImporter"/> which checks if the destination file exists and is the same as the source
	/// (if the given <see cref="IDuplicateResolver"/> supports checking file contents).
	/// If it is not, then the file is deleted. Has an option to simply delete every source file without checking anything.
	/// </summary>
	public class FileDeleteImporter : IFileImporter {
		protected IFileSystemProvider FileSystem { get; set; }
		protected IDuplicateResolver DuplicateResolver { get; set; }
		public bool DeleteEverything { get; }
		protected string? Operation { get; }

		public FileDeleteImporter(IFileSystemProvider fileSystem, IDuplicateResolver duplicateResolver, string? operation, bool deleteEverything = false) {
			FileSystem = fileSystem;
			DuplicateResolver = duplicateResolver;
			DeleteEverything = deleteEverything;
			Operation = operation;
		}

		public IEnumerable<ImportItem> ImportItems(IEnumerable<ImportItem> importItems, IErrorHandler errorHandler, ILogger logger) {
			FileSystem.NewTransaction(Operation);
			IEnumerable<ImportItem> imported;
			if (DeleteEverything) {
				var importing = new List<ImportItem>();
				foreach (var item in importItems) {
					if (!item.Cancelled) {
						logger.Write($"Delete\t{logger.GetSuitablePath(item.SourcePath, FileSystem)}");
						try {
							FileSystem.DeleteFile(item.SourcePath);

							logger.WriteLine("\t:  Deleted");
							importing.Add(item);
						} catch (Exception e) {
							logger.WriteLine("\t:  Error!");
							errorHandler.Error(e.Message);
							// Console.Error.WriteLine(e);
						}
					}
				}
				imported = importing;
			} else {
				if (DuplicateResolver.ChecksFileContents)
					imported = ImportItemsForceDuplicates(importItems, errorHandler, logger);
				else
					imported = ImportItemsNoDuplicates(importItems, errorHandler, logger);
			}
			FileSystem.EndTransaction();
			return imported;
		}

		protected IEnumerable<ImportItem> ImportItemsForceDuplicates(IEnumerable<ImportItem> importItems, IErrorHandler errorHandler, ILogger logger) {
			var imported = new List<ImportItem>();
			foreach (var item in importItems) {
				if (!item.Cancelled) {
					var importedItem = item;
					logger.Write($"Delete\t{logger.GetSuitablePath(item.SourcePath, FileSystem)}\t-> {logger.GetSuitablePath(item.DestinationPath, FileSystem)}");
					var different = false;
					if (FileSystem.FileExists(item.DestinationPath)) {
						var prevDest = item.DestinationPath;
						importedItem = DuplicateResolver.ResolveDuplicate(importedItem, FileSystem);
						if (importedItem.DestinationPath != prevDest)
							logger.Write($"\t=> {logger.GetSuitablePath(importedItem.DestinationPath, FileSystem)}");
						if (importedItem.Cancelled) {
							if (importedItem.IsDuplicate) {
								logger.WriteLine("\t:  Exists (is the same)");
								continue;
							} else {
								different = true;
							}
						} else if (importedItem.Overwrite) {
							logger.WriteLine("\t:  Incorrect behaviour (overwrite)");
							continue;
						} else {
							logger.WriteLine("\t:  Incorrect behaviour (rename)");
							continue;
						}
					}

					try {
						FileSystem.DeleteFile(importedItem.SourcePath);

						if (different)
							logger.WriteLine("\t:  Deleted (different)");
						else
							logger.WriteLine("\t:  Deleted");
						imported.Add(importedItem);
					} catch (Exception e) {
						logger.WriteLine("\t:  Error!");
						errorHandler.Error(e.Message);
						// Console.Error.WriteLine(e);
					}
				}
			}
			return imported;
		}

		protected IEnumerable<ImportItem> ImportItemsNoDuplicates(IEnumerable<ImportItem> importItems, IErrorHandler errorHandler, ILogger logger) {
			var imported = new List<ImportItem>();
			foreach (var item in importItems) {
				if (!item.Cancelled) {
					var importedItem = item;
					logger.Write($"Delete\t{logger.GetSuitablePath(item.SourcePath, FileSystem)}\t-> {logger.GetSuitablePath(item.DestinationPath, FileSystem)}");
					if (FileSystem.FileExists(item.DestinationPath)) {
						var prevDest = item.DestinationPath;
						importedItem = DuplicateResolver.ResolveDuplicate(importedItem, FileSystem);
						if (importedItem.DestinationPath != prevDest)
							logger.Write($"\t=> {logger.GetSuitablePath(importedItem.DestinationPath, FileSystem)}");

						if (importedItem.Cancelled) {
							if (importedItem.IsDuplicate)
								logger.WriteLine("\t:  Exists (is the same)");
							else
								logger.WriteLine("\t:  Exists (may not be the same)");
						} else if (importedItem.Overwrite) {
							logger.WriteLine("\t:  Incorrect behaviour (overwrite)");
						} else {
							logger.WriteLine("\t:  Incorrect behaviour (rename)");
						}
						continue;
					}

					try {
						FileSystem.DeleteFile(importedItem.SourcePath);

						logger.WriteLine("\t:  Deleted");
						imported.Add(importedItem);
					} catch (Exception e) {
						logger.WriteLine("\t:  Error!");
						errorHandler.Error(e.Message);
						// Console.Error.WriteLine(e);
					}
				}
			}
			return imported;
		}
	}
}
