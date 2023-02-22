using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace AlbumLibrary {
	/// <summary>
	/// An interface for getting the info of a file given its path and a <see cref="IFileSystemProvider"/>.
	/// This information may be incomplete, see <see cref="EXIFFileInfoProvider"/>,
	/// <see cref="NoEXIFDateFileInfoProvider"/> and <see cref="RelativePathFileInfoProvider"/>.
	/// </summary>
	public interface IFileInfoProvider {
		/// <summary>
		/// Gets the info for the specified files. It may not necessarily be all the information that is available about the file.
		/// </summary>
		/// <param name="fullPath">The full path to the file.</param>
		/// <param name="fileSystem"></param>
		/// <returns><see cref="FileInfo"/> containing information about the selected file</returns>
		public FileInfo GetInfo(string fullPath, IFileSystemProvider fileSystem);
	}

	/// <summary>
	/// A class which bundles the required information about a given file.
	/// </summary>
	public class FileInfo {
		/// <summary>
		/// Name of this file with the extension.
		/// </summary>
		public string OriginalFileName { get; }
		/// <summary>
		/// The extension of this file with the <c>.</c> (dot) symbol.
		/// </summary>
		public string OriginalFileExtension { get; }
		/// <summary>
		/// The full path to this file.
		/// </summary>
		public string OriginalFilePath { get; }
		/// <summary>
		/// The relative path to this file in the album, or <see langword="null"/> if the file is not in the album.
		/// </summary>
		public string? OriginalFileRelativePath { get; }

		/// <summary>
		/// The actual date of the file extracted from the EXIF of the file, or <see langword="null"/> if unavailable.
		/// </summary>
		public DateTime? TrueEXIFDateTime { get; }
		/// <summary>
		/// The actual file creation date of this file.
		/// </summary>
		public DateTime TrueFileCreation { get; }
		/// <summary>
		/// The actual file modification (or last write) date of this file.
		/// </summary>
		public DateTime TrueFileModification { get; }

		/// <summary>
		/// The date extracted from the EXIF modified using the current <see cref="TimeShift"/>.
		/// </summary>
		public DateTime? EXIFDateTime => TrueEXIFDateTime?.Add(TimeShift);
		/// <summary>
		/// The file creation date modified using the current <see cref="TimeShift"/>.
		/// </summary>
		public DateTime FileCreation => TrueFileCreation.Add(TimeShift);
		/// <summary>
		/// The file modification date modified using the current <see cref="TimeShift"/>.
		/// </summary>
		public DateTime FileModification => TrueFileModification.Add(TimeShift);

		/// <summary>
		/// The amount by which to shift the dates.
		/// </summary>
		public TimeSpan TimeShift { get; set; } = TimeSpan.Zero;

		/// <summary>
		/// Either <see cref="TrueEXIFDateTime"/> or <see cref="TrueFileCreation"/>, depending on whether the former is <see langword="null"/> or not.
		/// </summary>
		public DateTime TrueEXIFOrCreationDateTime => TrueEXIFDateTime ?? TrueFileCreation;
		/// <summary>
		/// Same as <see cref="TrueEXIFOrCreationDateTime"/> but uses <see cref="EXIFDateTime"/> and <see cref="FileCreation"/>.
		/// </summary>
		public DateTime SuitableDateTime => EXIFDateTime ?? FileCreation;

		/// <summary>
		/// The manufacturer of the device which took the photo, if applicable and available.
		/// </summary>
		public string? Manufacturer { get; }
		/// <summary>
		/// The model of the device which took the photo, if applicable and available.
		/// </summary>
		public string? Model { get; }

		/// <summary>
		/// <see cref="Manufacturer"/> + <c>" "</c> + <see cref="Model"/>
		/// </summary>
		public string? DeviceName { get; }

		public FileInfo(string fullPath, DateTime? exifDateTime, DateTime fileCreation, DateTime fileModification, string? manufacturer, string? model,
			string? originalFileRelativePath) {
			OriginalFileName = Path.GetFileNameWithoutExtension(fullPath);
			OriginalFileExtension = Path.GetExtension(fullPath);
			OriginalFilePath = fullPath;
			TrueEXIFDateTime = exifDateTime;
			TrueFileCreation = fileCreation;
			TrueFileModification = fileModification;
			Manufacturer = manufacturer;
			Model = model;
			DeviceName = manufacturer is null && model is null ? null : (manufacturer is null ? "" : manufacturer + " ") + (model ?? "");
			OriginalFileRelativePath = originalFileRelativePath;
		}

		public FileInfo(string fullPath, DateTime fileCreation, DateTime? exifDateTime = null, string? manufacturer = null, string? model = null,
			string? originalFileRelativePath = null) :
			this(fullPath, exifDateTime, fileCreation, fileCreation, manufacturer, model, originalFileRelativePath) { }

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

		public string ToSavableString() {
			return $"{TrueEXIFDateTime:o} {TrueFileCreation:o} {TrueFileModification:o}\n{Manufacturer}\n{Model}";
		}

		public static FileInfo? ReadFromStream(string fullPath, string relativePath, StreamReader stream) {
			try {
				var datesStr = stream.ReadLine()?.Split(' ').ToArray();
				if (datesStr is null)
					return null;

				DateTime? exif = string.IsNullOrEmpty(datesStr[0]) ? null : DateTime.Parse(datesStr[0]);
				var dates = datesStr[1..].Select(DateTime.Parse).ToList();
				var manufacturer = stream.ReadLine();
				var model = stream.ReadLine();
				return new FileInfo(fullPath, exif, dates[0], dates[1], manufacturer, model, relativePath);
			} catch {
				return null;
			}
		}
	}

	/// <summary>
	/// Reads the EXIF information of the file (if it has one).
	/// </summary>
	public class EXIFFileInfoProvider : IFileInfoProvider {
		public FileInfo GetInfo(string fullPath, IFileSystemProvider fileSystem) => GetFileInfo(fullPath, fileSystem);

		public static FileInfo GetFileInfo(string fullPath, IFileSystemProvider fileSystem) {
			if (fileSystem is IndexingFileSystemProvider f) {
				var info = f.Index.GetFileInfo(f.GetRelativePath(f.GetAlbumDirectory(), fullPath));
				if (info is not null)
					return info;
			}

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

	/// <summary>
	/// Reads the EXIF information of the file (if it has one) but does not use the date. Used for files already in the album.
	/// </summary>
	public class NoEXIFDateFileInfoProvider : IFileInfoProvider {
		public FileInfo GetInfo(string fullPath, IFileSystemProvider fileSystem) {
			var info = EXIFFileInfoProvider.GetFileInfo(fullPath, fileSystem);

			return new FileInfo(fullPath, info.TrueEXIFDateTime, info.TrueFileCreation, info.TrueFileModification, info.Manufacturer, info.Model,
				info.OriginalFileRelativePath);
		}
	}

	/// <summary>
	/// Uses a specified <see cref="IFileInfoProvider"/> but adds the relative path to the file in the album.
	/// </summary>
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
