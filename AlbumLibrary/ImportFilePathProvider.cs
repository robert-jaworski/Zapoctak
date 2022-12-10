using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbumLibrary {
	public interface IImportFilePathProvider {
		IEnumerable<string> GetFilePaths(string directoryPath);

		IEnumerable<string> GetErrors();
	}
	public class ImportFilePathProvider : IImportFilePathProvider {
		protected List<IFilePathProvider> FileProviders { get; }

		protected List<string> Errors { get; }

		protected HashSet<string> AllowedExtensions { get; }

		public ImportFilePathProvider(List<IFilePathProvider> fileProviders, HashSet<string> allowedExtensions) {
			FileProviders = fileProviders;
			Errors = new List<string>();
			AllowedExtensions = allowedExtensions;
		}

		public static ImportFilePathProvider Process(List<string> fileSpecs, HashSet<string> allowedExtensions) {
			var fileProviders = new List<IFilePathProvider>();
			var errors = new List<string>();

			for (int i = 0; i < fileSpecs.Count; i++) {
				var file = fileSpecs[i];
				var prev = i - 1 >= 0 ? fileSpecs[i - 1] : null;
				var next = i + 1 < fileSpecs.Count ? fileSpecs[i + 1] : null;

				if (string.IsNullOrEmpty(file))
					throw new ArgumentException("FileSpecs contains a null or empty string");
				if (file == "...") {
					errors.Add($"Invalid file range specification: {prev ?? ""} {file} {next ?? ""}");
					continue;
				}

				if (Path.EndsInDirectorySeparator(file)) {
					fileProviders.Add(new DirectoryFilePathProvider(file));
				} else if (file.EndsWith("...")) {
					var dir = Path.GetDirectoryName(file[..^3]);
					fileProviders.Add(new RangeFilePathProvider(string.IsNullOrEmpty(dir) ? "" : dir, Path.GetFileName(file[..^3]), null));
				} else if (file.StartsWith("...")) {
					var dir = Path.GetDirectoryName(file[3..]);
					fileProviders.Add(new RangeFilePathProvider(string.IsNullOrEmpty(dir) ? "" : dir, null, Path.GetFileName(file[3..])));
				} else if (next == "...") {
					i++;
					var nnext = i + 1 < fileSpecs.Count ? fileSpecs[i + 1] : null;
					if (string.IsNullOrEmpty(nnext) || Path.EndsInDirectorySeparator(nnext) || nnext.StartsWith("...") || nnext.EndsWith("...")) {
						errors.Add($"Invalid file range specification: {prev ?? ""} {file} {next} {nnext ?? ""}");
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
						errors.Add($"File range directories do not match: {file} {next} {nnext}");
						continue;
					}

					fileProviders.Add(new RangeFilePathProvider(dir1, Path.GetFileName(file), Path.GetFileName(nnext)));
				} else {
					fileProviders.Add(new SingleFilePathProvider(file));
				}
			}

			var p = new ImportFilePathProvider(fileProviders, allowedExtensions);
			p.Errors.AddRange(errors);
			return p;
		}

		public IEnumerable<string> GetFilePaths(string directoryPath) {
			return from p in FileProviders
				   select p.GetFilePaths(directoryPath, Errors, AllowedExtensions) into files
				   from f in files
				   select Path.GetFullPath(f);
		}

		public IEnumerable<string> GetErrors() {
			return Errors;
		}
	}

	public interface IFilePathProvider {
		IEnumerable<string> GetFilePaths(string dirPath, List<string> errors, HashSet<string> allowedExtensions);
	}

	public class SingleFilePathProvider : IFilePathProvider {
		protected string FilePath { get; }

		public SingleFilePathProvider(string path) {
			FilePath = path;
		}

		public IEnumerable<string> GetFilePaths(string dirPath, List<string> errors, HashSet<string> allowedExtensions) {
			var p = Path.Combine(dirPath, FilePath);
			var ext = Path.GetExtension(p);
			if (allowedExtensions.Contains(ext)) {
				if (File.Exists(p)) {
					yield return p;
				} else {
					errors.Add($"File does not exist: {p}");
				}
			} else {
				errors.Add($"Disallowed extension '{ext}': {p}");
			}
		}
	}

	public class DirectoryFilePathProvider : IFilePathProvider {
		protected string DirectoryPath { get; }

		public DirectoryFilePathProvider(string path) {
			DirectoryPath = path;
		}

		public IEnumerable<string> GetFilePaths(string dirPath, List<string> errors, HashSet<string> allowedExtensions) {
			var p = Path.Combine(dirPath, DirectoryPath);
			if (Directory.Exists(p)) {
				int files = 0;
				foreach (var file in Directory.EnumerateFiles(p)) {
					if (allowedExtensions.Contains(Path.GetExtension(file))) {
						files++;
						yield return file;
					}
				}
				if (files == 0) {
					errors.Add($"No suitable files found in directory: {p}");
				}
			} else {
				errors.Add($"Directory does not exist: {p}");
			}
		}
	}

	public class RangeFilePathProvider : IFilePathProvider {
		protected string DirectoryPath { get; }
		protected string? StartPath { get; }
		protected string? EndPath { get; }

		public RangeFilePathProvider(string path, string? startFile, string? endFile) {
			DirectoryPath = path;
			StartPath = startFile is null ? null : Path.Combine(DirectoryPath, startFile);
			EndPath = endFile is null ? null : Path.Combine(DirectoryPath, endFile);
		}

		public IEnumerable<string> GetFilePaths(string dirPath, List<string> errors, HashSet<string> allowedExtensions) {
			var p = Path.Combine(dirPath, DirectoryPath);
			var start = StartPath is null ? null : Path.Combine(dirPath, StartPath);
			var end = EndPath is null ? null : Path.Combine(dirPath, EndPath);
			if (Directory.Exists(p)) {
				int files = 0;
				foreach (var file in Directory.EnumerateFiles(p)) {
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
					errors.Add($"No suitable files found in range: {start ?? ""} ... {end ?? ""}");
				}
			} else {
				errors.Add($"Directory does not exist: {p}");
			}
		}
	}
}
