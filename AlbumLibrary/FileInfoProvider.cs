using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbumLibrary {
	public interface IFileInfoProvider {
		public FileInfo GetInfo(string fullPath, IFileSystemProvider fileSystem);
	}

	public class FileInfo {
		public DateTime? EXIFDateTime { get; }
		public DateTime FileCreation { get; }
		public DateTime FileModification { get; }

		public DateTime SuitableDateTime { get; }

		public string? Manufacturer { get; }
		public string? Model { get; }

		public string DeviceName { get; }

		public FileInfo(DateTime? exifDateTime, DateTime fileCreation, DateTime fileModification, string? manufacturer, string? model) {
			EXIFDateTime = exifDateTime;
			FileCreation = fileCreation;
			FileModification = fileModification;
			SuitableDateTime = EXIFDateTime ?? fileCreation;
			Manufacturer = manufacturer;
			Model = model;
			DeviceName = (manufacturer is null ? "" : manufacturer + " ") + (model ?? "");
		}
	}

	public class NormalFileInfoProvider : IFileInfoProvider {
		public FileInfo GetInfo(string fullPath, IFileSystemProvider fileSystem) {
			var fileCreation = fileSystem.FileCreation(fullPath);
			var fileModification = fileSystem.FileModification(fullPath);

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

			return new FileInfo(exifDT, fileCreation, fileModification, make, model);
		}
	}
}
