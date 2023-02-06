using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace AlbumLibrary {
	public interface IFileInfoProvider {
		public FileInfo GetInfo(string fullPath, IFileSystemProvider fileSystem);
	}

	public class FileInfo {
		public string OriginalFileName { get; }
		public string OriginalFileExtension { get; }
		public string OriginalFilePath { get; }
		public string? OriginalFileRelativePath { get; }

		public DateTime? TrueEXIFDateTime { get; }
		public DateTime TrueFileCreation { get; }
		public DateTime TrueFileModification { get; }

		public DateTime? EXIFDateTime => TrueEXIFDateTime?.Add(TimeShift);
		public DateTime FileCreation => TrueFileCreation.Add(TimeShift);
		public DateTime FileModification => TrueFileModification.Add(TimeShift);

		public TimeSpan TimeShift { get; set; } = TimeSpan.Zero;

		public DateTime TrueEXIFOrCreationDateTime => TrueEXIFDateTime ?? TrueFileCreation;
		public DateTime SuitableDateTime => EXIFDateTime ?? FileCreation;

		public string? Manufacturer { get; }
		public string? Model { get; }

		public string? DeviceName { get; }

		public FileInfo(string path, DateTime? exifDateTime, DateTime fileCreation, DateTime fileModification, string? manufacturer, string? model, string? originalFileRelativePath) {
			OriginalFileName = Path.GetFileNameWithoutExtension(path);
			OriginalFileExtension = Path.GetExtension(path);
			OriginalFilePath = path;
			TrueEXIFDateTime = exifDateTime;
			TrueFileCreation = fileCreation;
			TrueFileModification = fileModification;
			Manufacturer = manufacturer;
			Model = model;
			DeviceName = manufacturer is null && model is null ? null : (manufacturer is null ? "" : manufacturer + " ") + (model ?? "");
			OriginalFileRelativePath = originalFileRelativePath;
		}

		public FileInfo(string path, DateTime fileCreation, DateTime? exifDateTime = null, string? manufacturer = null, string? model = null,
			string? originalFileRelativePath = null) :
			this(path, exifDateTime, fileCreation, fileCreation, manufacturer, model, originalFileRelativePath) { }

		public FileInfo(FileInfo fileInfo, string? originalFileRelativePath) {
			OriginalFileName = fileInfo.OriginalFileName;
			OriginalFileExtension = fileInfo.OriginalFileExtension;
			OriginalFilePath = fileInfo.OriginalFilePath;
			TrueEXIFDateTime = fileInfo.TrueEXIFDateTime;
			TrueFileCreation = fileInfo.TrueFileCreation;
			TrueFileModification = fileInfo.TrueFileModification;
			Manufacturer = fileInfo.Manufacturer;
			Model = fileInfo.Model;
			DeviceName = fileInfo.DeviceName;
			OriginalFileRelativePath = originalFileRelativePath;
		}
	}

	public class EXIFFileInfoProvider : IFileInfoProvider {
		public FileInfo GetInfo(string fullPath, IFileSystemProvider fileSystem) {
			var fileCreation = fileSystem.GetFileCreation(fullPath);
			var fileModification = fileSystem.GetFileModification(fullPath);

			// This code uses the following library: https://drewnoakes.com/code/exif/

			var directories = fileSystem.GetFileInfo(fullPath);
			var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
			var ifd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

			DateTime? exifDT = null;
			try {
				exifDT = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
			} catch (MetadataException) { }
			if (exifDT is null) {
				try {
					exifDT = ifd0Directory?.GetDateTime(ExifDirectoryBase.TagDateTime);
				} catch (MetadataException) { }
			}

			var make = ifd0Directory?.GetDescription(ExifDirectoryBase.TagMake);
			var model = ifd0Directory?.GetDescription(ExifDirectoryBase.TagModel);

			return new FileInfo(fullPath, exifDT, fileCreation, fileModification, make, model, null);
		}
	}

	public class NoEXIFDateFileInfoProvider : IFileInfoProvider {
		public FileInfo GetInfo(string fullPath, IFileSystemProvider fileSystem) {
			var fileCreation = fileSystem.GetFileCreation(fullPath);
			var fileModification = fileSystem.GetFileModification(fullPath);

			// This code uses the following library: https://drewnoakes.com/code/exif/

			var directories = fileSystem.GetFileInfo(fullPath);
			var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
			var ifd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

			DateTime? exifDT = null;
			try {
				exifDT = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
			} catch (MetadataException) { }
			if (exifDT is null) {
				try {
					exifDT = ifd0Directory?.GetDateTime(ExifDirectoryBase.TagDateTime);
				} catch (MetadataException) { }
			}

			var make = ifd0Directory?.GetDescription(ExifDirectoryBase.TagMake);
			var model = ifd0Directory?.GetDescription(ExifDirectoryBase.TagModel);

			return new FileInfo(fullPath, null, fileCreation, fileModification, make, model, null);
		}
	}

	public class RelativePathFileInfoProvider : IFileInfoProvider {
		protected IFileInfoProvider FileInfoProvider { get; }
		protected string? RelativeTo { get; }

		public RelativePathFileInfoProvider(IFileInfoProvider fileInfoProvider, string? relativeTo) {
			FileInfoProvider = fileInfoProvider;
			RelativeTo = relativeTo;
		}

		public FileInfo GetInfo(string fullPath, IFileSystemProvider fileSystem) {
			var info = FileInfoProvider.GetInfo(fullPath, fileSystem);
			return new FileInfo(info, fileSystem.GetRelativePath(RelativeTo ?? fileSystem.GetAlbumDirectory(), fullPath));
		}
	}
}
