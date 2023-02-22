namespace AlbumLibrary {
	/// <summary>
	/// An interface which decides whether a file should be processed given its <see cref="FileInfo"/>.
	/// It can also modify the info, see <see cref="TimeShiftFilter"/>.
	/// </summary>
	public interface IFileFilter {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		/// <returns><see langword="null"/> if the file should not be processed,
		/// otherwise the (potentially modified) <see cref="FileInfo"/> of the file.</returns>
		FileInfo? Filter(FileInfo file);
	}

	/// <summary>
	/// An <see cref="IFileFilter"/> which shifts the EXIF, creation and modification times of the given file by a specified amount.
	/// </summary>
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

	/// <summary>
	/// An <see cref="IFileFilter"/> which allows only files which are older than a specified point in time.
	/// </summary>
	public class BeforeDateFilter : IFileFilter {
		public DateTime Date { get; }

		public BeforeDateFilter(DateTime dateTime) {
			Date = dateTime;
		}

		public FileInfo? Filter(FileInfo file) {
			return file.TrueEXIFOrCreationDateTime <= Date ? file : null;
		}
	}

	/// <summary>
	/// An <see cref="IFileFilter"/> which allows only files which are younger than a specified point in time.
	/// </summary>
	public class AfterDateFilter : IFileFilter {
		public DateTime Date { get; }

		public AfterDateFilter(DateTime dateTime) {
			Date = dateTime;
		}

		public FileInfo? Filter(FileInfo file) {
			return file.TrueEXIFOrCreationDateTime >= Date ? file : null;
		}
	}

	/// <summary>
	/// An <see cref="IFileFilter"/> which runs the <see cref="TemplateFileNameProvider"/> with the given template and <see cref="FileInfo"/>.
	/// If the resulting name is in the format <c>xxx=yyy</c> or <c>aaa=bbb=ccc</c> etc., then the file will be processed if the equations are true.
	/// </summary>
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

	/// <summary>
	/// An <see cref="IFileFilter"/> which acts like a logical or. Returns the first non-<see langword="null"/> result
	/// from the specified list of <see cref="IFileFilter"/>
	/// </summary>
	public class FirstFilter : IFileFilter {
		protected List<IFileFilter> FileFilters { get; }

		public FirstFilter(IEnumerable<IFileFilter> fileFilters) {
			FileFilters = fileFilters.ToList();
		}

		public FileInfo? Filter(FileInfo file) {
			return FileFilters.Select(x => x.Filter(file)).Where(x => x is not null).FirstOrDefault();
		}
	}

	/// <summary>
	/// An <see cref="IImportListProvider"/> which filters the files using a given list of <see cref="IFileFilter"/>.
	/// </summary>
	public class FilteredImportListProvider : IImportListProvider {
		protected List<IFileFilter> FileFilters { get; set; }
		protected IFileInfoProvider FileInfoProvider { get; set; }
		protected IFileNameProvider FileNameProvider { get; set; }

		public FilteredImportListProvider(IEnumerable<IFileFilter> fileFilters, IFileInfoProvider fileInfoProvider, IFileNameProvider fileNameProvider) {
			FileFilters = fileFilters.ToList();
			FileInfoProvider = fileInfoProvider;
			FileNameProvider = fileNameProvider;
		}

		public IEnumerable<ImportItem> GetImportItems(IFileSystemProvider fileSystem, IEnumerable<string> importFilePaths, IErrorHandler errorHandler, ILogger logger) {
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
					// Console.Error.WriteLine(e);
				}
				if (val is not null)
					yield return val;
			}
		}
	}
}
