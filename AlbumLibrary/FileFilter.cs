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
			file.OverwriteDateTime = (file.EXIFDateTime ?? file.FileCreation).Add(Shift);
			return file;
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
