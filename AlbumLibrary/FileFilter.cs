using Sharpen;

namespace AlbumLibrary {
	public interface IFileFilter {
		FileInfo? Filter(FileInfo file);
	}

	public class TimeShiftFilter : IFileFilter {
		public TimeSpan Shift { get; }

		public TimeShiftFilter(TimeSpan shift) {
			Shift = shift;
		}

		public FileInfo? Filter(FileInfo file) {
			file.TimeShift = Shift;
			return file;
		}
	}

	public class BeforeDateFilter : IFileFilter {
		public DateTime Date { get; }

		public BeforeDateFilter(DateTime dateTime) {
			Date = dateTime;
		}

		public FileInfo? Filter(FileInfo file) {
			return file.TrueEXIFOrCreationDateTime <= Date ? file : null;
		}
	}

	public class AfterDateFilter : IFileFilter {
		public DateTime Date { get; }

		public AfterDateFilter(DateTime dateTime) {
			Date = dateTime;
		}

		public FileInfo? Filter(FileInfo file) {
			return file.TrueEXIFOrCreationDateTime >= Date ? file : null;
		}
	}

	public class TemplateFilter : IFileFilter {
		public IFileNameProvider FileNameProvider { get; }

		public TemplateFilter(string template) {
			FileNameProvider = TemplateFileNameProvider.MultipleTemplates(template.Split(','));
		}

		public FileInfo? Filter(FileInfo file) {
			try {
				var result = FileNameProvider.GetFileName(file, false).Split('=');
				return result.Skip(1).All(x => x == result[0]) ? file : null;
			} catch (CancelFileCopyException) {
				return null;
			}
		}
	}

	public class FirstFilter : IFileFilter {
		protected List<IFileFilter> FileFilters { get; }

		public FirstFilter(IEnumerable<IFileFilter> fileFilters) {
			FileFilters = fileFilters.ToList();
		}

		public FileInfo? Filter(FileInfo file) {
			return FileFilters.Select(x => x.Filter(file)).Where(x => x is not null).FirstOrDefault();
		}
	}

	public class FilteredImportListProvider : IImportListProvider {
		protected List<IFileFilter> FileFilters { get; set; }
		protected IFileInfoProvider FileInfoProvider { get; set; }
		protected IFileNameProvider FileNameProvider { get; set; }

		public FilteredImportListProvider(IEnumerable<IFileFilter> fileFilters, IFileInfoProvider fileInfoProvider, IFileNameProvider fileNameProvider) {
			FileFilters = fileFilters.ToList();
			FileInfoProvider = fileInfoProvider;
			FileNameProvider = fileNameProvider;
		}

		public IEnumerable<ImportItem> GetImportItems(IFileSystemProvider fileSystem, IEnumerable<string> importFilePaths, IErrorHandler errorHandler) {
			foreach (var file in importFilePaths) {
				var info = FileInfoProvider.GetInfo(file, fileSystem);
				var ignore = false;
				foreach (var filter in FileFilters) {
					var newInfo = filter.Filter(info);
					if (newInfo is not null)
						info = newInfo;
					else {
						ignore = true;
						break;
					}
				}
				if (ignore)
					continue;

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
}
