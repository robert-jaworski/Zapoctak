namespace AlbumLibrary {
	public interface IImportFilePathProvider {
		IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler);
	}

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

				if (Path.EndsInDirectorySeparator(file)) {
					fileProviders.Add(new DirectoryFilePathProvider(file));
				} else if (file.EndsWith("...")) {
					var prefix = file[..^3];
					if (Path.EndsInDirectorySeparator(prefix)) {
						fileProviders.Add(new DirectoryFilePathProvider(prefix, true));
					} else {
						var dir = Path.GetDirectoryName(prefix);
						fileProviders.Add(new RangeFilePathProvider(string.IsNullOrEmpty(dir) ? "" : dir, Path.GetFileName(prefix), null));
					}
				} else if (file.StartsWith("...")) {
					var dir = Path.GetDirectoryName(file[3..]);
					fileProviders.Add(new RangeFilePathProvider(string.IsNullOrEmpty(dir) ? "" : dir, null, Path.GetFileName(file[3..])));
				} else if (next == "...") {
					i++;
					var nnext = i + 1 < fileSpecs.Count ? fileSpecs[i + 1] : null;
					if (string.IsNullOrEmpty(nnext) || Path.EndsInDirectorySeparator(nnext) || nnext.StartsWith("...") || nnext.EndsWith("...")) {
						errorHandler.Error($"Invalid file range specification: {prev ?? ""} {file} {next} {nnext ?? ""}");
						continue;
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

					fileProviders.Add(new RangeFilePathProvider(dir1, Path.GetFileName(file), Path.GetFileName(nnext)));
				} else {
					fileProviders.Add(new SingleFilePathProvider(file));
				}
			}

			return new ImportFilePathProvider(fileProviders, allowedExtensions);
		}

		public IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler) {
			return from p in FileProviders
				   select p.GetFilePaths(fileSystem, errorHandler, AllowedExtensions) into files
				   from f in files
				   select fileSystem.GetFullPath(f);
		}

		public List<IFilePathProvider> GetFilePathProviders() {
			return new List<IFilePathProvider>(FileProviders);
		}
	}

	public interface IFilePathProvider {
		IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler, HashSet<string> allowedExtensions);
	}

	public class SingleFilePathProvider : IFilePathProvider {
		public string FilePath { get; }

		public SingleFilePathProvider(string path) {
			FilePath = path;
		}

		public IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler, HashSet<string> allowedExtensions) {
			var p = fileSystem.GetFullPath(FilePath);
			var ext = Path.GetExtension(p);
			if (allowedExtensions.Contains(ext)) {
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

	public class DirectoryFilePathProvider : IFilePathProvider {
		public string DirectoryPath { get; }
		public bool Recursive { get; }

		public DirectoryFilePathProvider(string path, bool recursive = false) {
			DirectoryPath = path;
			Recursive = recursive;
		}

		public IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler, HashSet<string> allowedExtensions) {
			var paths = new Stack<string>();
			paths.Push(fileSystem.GetFullPath(DirectoryPath));
			while (paths.Count > 0) {
				var p = paths.Pop();
				if (fileSystem.DirectoryExists(p)) {
					int files = 0, dirs = 0;
					foreach (var file in fileSystem.EnumerateFiles(p)) {
						if (allowedExtensions.Contains(Path.GetExtension(file))) {
							files++;
							yield return file;
						}
					}
					if (Recursive) {
						foreach (var dir in fileSystem.EnumerateDirectories(p).Reverse()) {
							dirs++;
							paths.Push(dir);
						}
					}
					else if (files == 0 && dirs == 0) {
						errorHandler.Error($"No suitable files found in directory: {p}");
					}
				} else {
					errorHandler.Error($"Directory does not exist: {p}");
				}
			}
		}
	}

	public class RangeFilePathProvider : IFilePathProvider {
		public string DirectoryPath { get; }
		public string? StartPath { get; }
		public string? EndPath { get; }

		public RangeFilePathProvider(string path, string? startFile, string? endFile) {
			DirectoryPath = path;
			StartPath = startFile is null ? null : Path.Combine(DirectoryPath, startFile);
			EndPath = endFile is null ? null : Path.Combine(DirectoryPath, endFile);
		}

		public IEnumerable<string> GetFilePaths(IFileSystemProvider fileSystem, IErrorHandler errorHandler, HashSet<string> allowedExtensions) {
			var p = fileSystem.GetFullPath(DirectoryPath);
			var start = StartPath is null ? null : fileSystem.GetFullPath(StartPath);
			var end = EndPath is null ? null : fileSystem.GetFullPath(EndPath);
			if (fileSystem.DirectoryExists(p)) {
				int files = 0;
				foreach (var file in fileSystem.EnumerateFiles(p)) {
					if (start is not null && file.CompareTo(start) < 0)
						continue;
					if (end is not null && file.CompareTo(end) > 0)
						continue;
					if (allowedExtensions.Contains(Path.GetExtension(file))) {
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
