namespace AlbumLibrary {
	/// <summary>
	/// An interface for providing the paths of files to process.
	/// </summary>
	public interface IImportFilePathProvider {
		/// <summary>
		/// Provides the paths of available files to process given <see cref="IFileSystemProvider"/>.
		/// </summary>
		/// <param name="fileSystem"></param>
		/// <param name="errorHandler"></param>
		/// <returns></returns>
		IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler, ILogger logger);
	}

	/// <summary>
	/// Basic implementation of <see cref="IImportFilePathProvider"/>, uses a list of <see cref="IFilePathProvider"/> and
	/// a <see cref="HashSet{string}"/> of allowed extension to get the file paths.
	/// </summary>
	public class ImportFilePathProvider : IImportFilePathProvider {
		protected List<IFilePathProvider> FileProviders { get; }

		protected HashSet<string> AllowedExtensions { get; }

		public ImportFilePathProvider(List<IFilePathProvider> fileProviders, HashSet<string> allowedExtensions) {
			FileProviders = fileProviders;
			AllowedExtensions = allowedExtensions;
		}

		public static ImportFilePathProvider Process(IList<string> fileSpecs, HashSet<string> allowedExtensions, IErrorHandler errorHandler) {
			var fileProviders = new List<IFilePathProvider>();

			for (int i = 0; i < fileSpecs.Count; i++) {
				var file = fileSpecs[i].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
				var prev = i - 1 >= 0 ? fileSpecs[i - 1] : null;
				var next = i + 1 < fileSpecs.Count ? fileSpecs[i + 1] : null;

				if (string.IsNullOrEmpty(file))
					throw new ArgumentException("FileSpecs contains a null or empty string");
				if (file == "...") {
					errorHandler.Error($"Invalid file range specification: {prev ?? ""} {file} {next ?? ""}");
					continue;
				}

				if (file.StartsWith("@")) {
					if (file.StartsWith("@last")) {
						var countStr = file[5..];
						int count = 1;
						if (string.IsNullOrEmpty(countStr) || int.TryParse(countStr, out count)) {
							if (count > 0) {
								fileProviders.Add(new LastTransactionFilePathProvider(count));
							} else {
								errorHandler.Error($"Expected a positive number in @last specification: {countStr}");
							}
						} else {
							errorHandler.Error($"Expected a number in @last specification: {countStr}");
						}
					} else {
						errorHandler.Error($"Unknown pseudo file specification: {file}");
					}
					continue;
				}

				var allDrives = OperatingSystem.IsWindows() && file.StartsWith(Path.VolumeSeparatorChar);
				if (allDrives) {
					file = "C" + file;
				}

				if (Path.EndsInDirectorySeparator(file)) {
					fileProviders.Add(new DirectoryFilePathProvider(file, false, allDrives));
				} else if (file.EndsWith("...")) {
					var prefix = file[..^3];
					if (Path.EndsInDirectorySeparator(prefix)) {
						fileProviders.Add(new DirectoryFilePathProvider(prefix, true, allDrives));
					} else {
						var dir = Path.GetDirectoryName(prefix);
						fileProviders.Add(new RangeFilePathProvider(string.IsNullOrEmpty(dir) ? "" : dir, Path.GetFileName(prefix), null, allDrives));
					}
				} else if (file.StartsWith("...")) {
					var dir = Path.GetDirectoryName(file[3..]);
					fileProviders.Add(new RangeFilePathProvider(string.IsNullOrEmpty(dir) ? "" : dir, null, Path.GetFileName(file[3..]), allDrives));
				} else if (next == "...") {
					i++;
					var nnext = i + 1 < fileSpecs.Count ? fileSpecs[i + 1] : null;
					if (string.IsNullOrEmpty(nnext) || Path.EndsInDirectorySeparator(nnext) || nnext.StartsWith("...") || nnext.EndsWith("...") ||
						allDrives != nnext.StartsWith(Path.VolumeSeparatorChar)) {
						errorHandler.Error($"Invalid file range specification: {prev ?? ""} {file} {next} {nnext ?? ""}");
						continue;
					}
					if (allDrives) {
						nnext = "C" + nnext;
					}
					i++;

					var dir1 = Path.GetDirectoryName(file);
					var dir2 = Path.GetDirectoryName(nnext);
					if (string.IsNullOrEmpty(dir1))
						dir1 = ".";
					if (string.IsNullOrEmpty(dir2))
						dir2 = ".";
					if (dir1 != dir2) {
						errorHandler.Error($"File range directories do not match: {file} {next} {nnext}");
						continue;
					}

					fileProviders.Add(new RangeFilePathProvider(dir1, Path.GetFileName(file), Path.GetFileName(nnext), allDrives));
				} else {
					fileProviders.Add(new SingleFilePathProvider(file, allDrives));
				}
			}

			return new ImportFilePathProvider(fileProviders, allowedExtensions);
		}

		public IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler, ILogger logger) {
			return from p in FileProviders
				   select p.GetFilePaths(fileSystem, errorHandler, AllowedExtensions, logger) into files
				   from f in files
				   select fileSystem.GetFullPath(f);
		}

		public List<IFilePathProvider> GetFilePathProviders() {
			return new List<IFilePathProvider>(FileProviders);
		}

		public static IEnumerable<string> GoThroughAllDrives(string path, IFileSystemProvider fileSystem) {
			var fullPath = fileSystem.GetFullPath(path);
			foreach (var drive in "ABCDEFGHIJKLMNOPQRSTUVWXYZ") {
				yield return fileSystem.GetFullPath(drive + fullPath[1..]);
			}
		}
	}

	/// <summary>
	/// A simpler version of <see cref="IImportFilePathProvider"/> which accepts an additional parameter determining the acceptable file extensions.
	/// </summary>
	public interface IFilePathProvider {
		IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler, HashSet<string> allowedExtensions, ILogger logger);
	}

	/// <summary>
	/// An <see cref="IFilePathProvider"/> which returns a specified file.
	/// </summary>
	public class SingleFilePathProvider : IFilePathProvider {
		public string FilePath { get; }
		public bool AllDrives { get; }

		public SingleFilePathProvider(string path, bool allDrives) {
			FilePath = path;
			AllDrives = allDrives;
		}

		public IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler, HashSet<string> allowedExtensions, ILogger logger) {
			var fullPath = fileSystem.GetFullPath(FilePath);
			if (AllDrives) {
				var ext = Path.GetExtension(fullPath);
				if (!allowedExtensions.Contains(ext.ToLower())) {
					errorHandler.Error($"Disallowed extension '{ext}': {fullPath}");
					yield break;
				}

				var count = 0;
				foreach (var p in ImportFilePathProvider.GoThroughAllDrives(fullPath, fileSystem)) {
					ext = Path.GetExtension(p);
					if (allowedExtensions.Contains(ext.ToLower())) {
						if (fileSystem.FileExists(p)) {
							count++;
							yield return p;
						}
					}
				}
				if (count == 0) {
					errorHandler.Error($"No files found in any of the attached drives: {fullPath}");
				}
			} else {
				var p = fullPath;
				var ext = Path.GetExtension(p);
				if (allowedExtensions.Contains(ext.ToLower())) {
					if (fileSystem.FileExists(p)) {
						yield return p;
					} else {
						errorHandler.Error($"File does not exist: {p}");
					}
				} else {
					errorHandler.Error($"Disallowed extension '{ext}': {p}");
				}
			}
		}
	}

	/// <summary>
	/// An <see cref="IFilePathProvider"/> which returns all files in a specified directory, potentially recursively.
	/// </summary>
	public class DirectoryFilePathProvider : IFilePathProvider {
		public string DirectoryPath { get; }
		public bool Recursive { get; }
		public bool AllDrives { get; }

		public DirectoryFilePathProvider(string path, bool recursive, bool allDrives) {
			DirectoryPath = path;
			Recursive = recursive;
			AllDrives = allDrives;
		}

		public IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler, HashSet<string> allowedExtensions, ILogger logger) {
			var paths = new Stack<string>();
			if (AllDrives) {
				foreach (var p in ImportFilePathProvider.GoThroughAllDrives(DirectoryPath, fileSystem).Reverse()) {
					paths.Push(p);
				}
			} else {
				paths.Push(fileSystem.GetFullPath(DirectoryPath));
			}
			var total = 0;
			while (paths.Count > 0) {
				var p = paths.Pop();
				if (fileSystem.DirectoryExists(p)) {
					logger.WriteLine($"Entering directory: {logger.GetSuitablePath(p, fileSystem)}");
					int files = 0, dirs = 0;
					foreach (var file in fileSystem.EnumerateFiles(p)) {
						if (Path.GetFileName(file).StartsWith('.'))
							continue;
						if (allowedExtensions.Contains(Path.GetExtension(file).ToLower())) {
							files++;
							total++;
							yield return file;
						}
					}
					if (Recursive) {
						foreach (var dir in fileSystem.EnumerateDirectories(p).Reverse()) {
							if (Path.GetFileName(dir).StartsWith('.'))
								continue;
							dirs++;
							paths.Push(dir);
						}
					}
					else if (!AllDrives && files == 0 && dirs == 0) {
						errorHandler.Error($"No suitable files found in directory: {p}");
					}
				} else if (!AllDrives) {
					errorHandler.Error($"Directory does not exist: {p}");
				}
			}
			if (AllDrives && total == 0) {
				errorHandler.Error($"No files found in any of the attached drives: {fileSystem.GetFullPath(DirectoryPath)}");
			}
		}
	}

	/// <summary>
	/// An <see cref="IFilePathProvider"/> which returns all files in a single directory whose names sort between two specified files.
	/// </summary>
	public class RangeFilePathProvider : IFilePathProvider {
		public string DirectoryPath { get; }
		public string? StartPath { get; }
		public string? EndPath { get; }
		public bool AllDrives { get; }

		public RangeFilePathProvider(string path, string? startFile, string? endFile, bool allDrives) {
			DirectoryPath = path;
			StartPath = startFile is null ? null : Path.Combine(DirectoryPath, startFile);
			EndPath = endFile is null ? null : Path.Combine(DirectoryPath, endFile);
			AllDrives = allDrives;
		}

		public IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler, HashSet<string> allowedExtensions, ILogger logger) {
			if (AllDrives) {
				var files = 0;
				foreach (var path in ImportFilePathProvider.GoThroughAllDrives(DirectoryPath, fileSystem)) {
					var p = fileSystem.GetFullPath(path);
					var start = StartPath is null ? null : fileSystem.GetFullPath(StartPath);
					var end = EndPath is null ? null : fileSystem.GetFullPath(EndPath);
					if (fileSystem.DirectoryExists(p)) {
						foreach (var file in fileSystem.EnumerateFiles(p)) {
							if (Path.GetFileName(file).StartsWith('.'))
								continue;
							if (start is not null && file.CompareTo(start) < 0)
								continue;
							if (end is not null && file.CompareTo(end) > 0)
								continue;
							if (allowedExtensions.Contains(Path.GetExtension(file).ToLower())) {
								files++;
								yield return file;
							}
						}
					}
				}
				if (files == 0) {
					errorHandler.Error($"No suitable files found in range in any of the drives: " +
						$"{(StartPath is null ? "" : fileSystem.GetFullPath(StartPath))} ... " +
						$"{(EndPath is null ? "" : fileSystem.GetFullPath(EndPath))}");
				}
			} else {
				var p = fileSystem.GetFullPath(DirectoryPath);
				var start = StartPath is null ? null : fileSystem.GetFullPath(StartPath);
				var end = EndPath is null ? null : fileSystem.GetFullPath(EndPath);
				if (fileSystem.DirectoryExists(p)) {
					int files = 0;
					foreach (var file in fileSystem.EnumerateFiles(p)) {
						if (Path.GetFileName(file).StartsWith('.'))
							continue;
						if (start is not null && file.CompareTo(start) < 0)
							continue;
						if (end is not null && file.CompareTo(end) > 0)
							continue;
						if (allowedExtensions.Contains(Path.GetExtension(file).ToLower())) {
							files++;
							yield return file;
						}
					}
					if (files == 0) {
						errorHandler.Error($"No suitable files found in range: {start ?? ""} ... {end ?? ""}");
					}
				} else {
					errorHandler.Error($"Directory does not exist: {p}");
				}
			}
		}
	}

	public class LastTransactionFilePathProvider : IFilePathProvider {
		public int LastCount { get; }

		public LastTransactionFilePathProvider(int lastCount) {
			LastCount = lastCount;
		}

		public IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler, HashSet<string> allowedExtensions, ILogger logger) {
			var transactions = fileSystem.ReadUndoFile().ToList();
			var files = 0;
			var included = new HashSet<string>();
			if (transactions.Count < LastCount) {
				errorHandler.Error($"Not enough transactions in history! Expected at least {LastCount} but got {transactions.Count}.");
				foreach (var t in transactions) {
					foreach (var a in t.Actions) {
						var fullPath = fileSystem.GetFullPath(a.AffectedPath);
						if (allowedExtensions.Contains(Path.GetExtension(fullPath).ToLower()) && !included.Contains(fullPath)) {
							files++;
							yield return fullPath;
							included.Add(fullPath);
						}
					}
				}
			} else {
				foreach (var t in transactions.Skip(transactions.Count - LastCount)) {
					foreach (var a in t.Actions) {
						var fullPath = fileSystem.GetFullPath(a.AffectedPath);
						if (allowedExtensions.Contains(Path.GetExtension(fullPath).ToLower()) && !included.Contains(fullPath)) {
							files++;
							yield return fullPath;
							included.Add(fullPath);
						}
					}
				}
			}
			if (files == 0)
				errorHandler.Error("No suitable files in history");
		}
	}
}
