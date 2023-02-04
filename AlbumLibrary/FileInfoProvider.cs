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

		public DateTime? EXIFDateTime { get; }
		public DateTime FileCreation { get; }
		public DateTime FileModification { get; }
		public DateTime? OverwriteDateTime { get; set; }

		public DateTime EXIFOrCreationDateTime => EXIFDateTime ?? FileCreation;
		public DateTime SuitableDateTime => OverwriteDateTime ?? EXIFOrCreationDateTime;

		public string? Manufacturer { get; }
		public string? Model { get; }

		public string? DeviceName { get; }

		public FileInfo(string path, DateTime? exifDateTime, DateTime fileCreation, DateTime fileModification, string? manufacturer, string? model) {
			OriginalFileName = Path.GetFileNameWithoutExtension(path);
			OriginalFileExtension = Path.GetExtension(path);
			OriginalFilePath = path;
			EXIFDateTime = exifDateTime;
			FileCreation = fileCreation;
			FileModification = fileModification;
			Manufacturer = manufacturer;
			Model = model;
			DeviceName = manufacturer is null && model is null ? null : (manufacturer is null ? "" : manufacturer + " ") + (model ?? "");
		}

		public FileInfo(string path, DateTime fileCreation, DateTime? exifDateTime = null, string? manufacturer = null, string? model = null) :
			this(path, exifDateTime, fileCreation, fileCreation, manufacturer, model) { }
	}

	public class NormalFileInfoProvider : IFileInfoProvider {
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
			} catch (MetadataException) {}
			if (exifDT is null) {
				try {
					exifDT = ifd0Directory?.GetDateTime(ExifDirectoryBase.TagDateTime);
				} catch (MetadataException) { }
			}

			var make = ifd0Directory?.GetDescription(ExifDirectoryBase.TagMake);
			var model = ifd0Directory?.GetDescription(ExifDirectoryBase.TagModel);

			return new FileInfo(fullPath, exifDT, fileCreation, fileModification, make, model);
		}
	}
}
