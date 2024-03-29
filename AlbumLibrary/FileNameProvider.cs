﻿using System.Globalization;
using System.Text.RegularExpressions;

namespace AlbumLibrary {
	/// <summary>
	/// Provides destination names for the given files using their <see cref="FileInfo"/>.
	/// </summary>
	public interface IFileNameProvider {
		/// <summary>
		/// Provides destination name for the given file using its <see cref="FileInfo"/>.
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="addExtension"></param>
		/// <returns></returns>
		public string GetFileName(FileInfo fileInfo, bool addExtension = true);
	}

	/// <summary>
	/// An exception used to signify that the given file cannot be processed.
	/// For example the <see cref="IFileNameProvider"/> requires the EXIF information about the device, but the file has no EXIF.
	/// </summary>
	public class CancelFileCopyException : Exception { }

	/// <summary>
	/// An <see cref="IFileNameProvider"/> which assigns names using a simple placeholder schema.
	/// </summary>
	public class TemplateFileNameProvider : IFileNameProvider {
		public string Template { get; }

		protected static Regex TemplateElement { get; } = new(@"\{([^{}:#]*)(?::([^{}:#]*))?(?:#([^{}:#]*))?\}");

		public delegate string TemplateReplacer(FileInfo fileInfo, string? options, int? width, ref bool addExtension);

		protected static Dictionary<string, TemplateReplacer> TemplateReplacers { get; } = new Dictionary<string, TemplateReplacer> {
			{ "year", Year },
			{ "Y", Year },
			{ "YY", Year2 },
			{ "YYYY", Year4 },
			{ "month", Month },
			{ "M", Month },
			{ "MM", Month2 },
			{ "day", Day },
			{ "D", Day },
			{ "DD", Day2 },
			{ "hour", Hour },
			{ "h", Hour },
			{ "hh", Hour2 },
			{ "minute", Minute },
			{ "m", Minute },
			{ "mm", Minute2 },
			{ "second", Second },
			{ "s", Second },
			{ "ss", Second2 },
			{ "device", Device },
			{ "noextension", NoExtension },
			{ "noext", NoExtension },
			{ "extension", Extension },
			{ "ext", Extension },
			{ ".", Extension },
			{ "file", File },
			{ "unicode", Unicode },
			{ "u", Unicode },
			{ "x", Unicode },
		};

		public TemplateFileNameProvider(string template) {
			Template = template;
			// Test template:
			var fileInfo = new FileInfo("test/portal.jpg", DateTime.Now, DateTime.Now, DateTime.Now, "Aperture Science, Inc.", "Aperture Science Handheld Portal Device", "portal.jpg");
			bool addExtension = true;
			foreach (var m in TemplateElement.Matches(Template).ToList()) {
				try {
					var elem = m.Groups[1].Value;
					if (elem is not null && TemplateReplacers.ContainsKey(elem)) {
						int? width = null;
						if (m.Groups[3].Success) {
							if (int.TryParse(m.Groups[3].Value, out int w))
								width = w;
							else
								throw new InvalidFileNameTemplateException($"Expected a number after '#': {m.Value}");
						}
						_ = TemplateReplacers[elem](fileInfo, m.Groups[2].Success ? m.Groups[2].Value : null, width, ref addExtension);
					} else
						throw new InvalidFileNameTemplateException($"Unknown template element: {m.Value}");
				} catch (CancelFileCopyException) { }
			}
		}

		/// <summary>
		/// Combines multiple templates into a <see cref="MultipleFileNameProvider"/>.
		/// </summary>
		/// <param name="templates"></param>
		/// <returns></returns>
		public static MultipleFileNameProvider MultipleTemplates(IEnumerable<string> templates) {
			return new MultipleFileNameProvider(from t in templates select new TemplateFileNameProvider(t));
		}

		public string GetFileName(FileInfo fileInfo, bool addExtension = true) {
			var name = TemplateElement.Replace(Template, m => TemplateReplacers[m.Groups[1].Value](fileInfo,
					m.Groups[2].Success ? m.Groups[2].Value : null,
					m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : null, ref addExtension)
				.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
			if (addExtension)
				return name + fileInfo.OriginalFileExtension;
			return name;
		}

		protected static DateTime GetDateTime(FileInfo fileInfo, string? options) {
			return options switch {
				null => fileInfo.SuitableDateTime,
				"exif" => fileInfo.EXIFDateTime ?? throw new CancelFileCopyException(),
				"create" => fileInfo.FileCreation,
				"modify" => fileInfo.FileModification,
				_ => throw new InvalidFileNameTemplateException($"Unknown date time option: {options}"),
			};
		}

		protected static string SetWidth(int x, int? width) {
			if (width is not null)
				return x.ToString().PadLeft((int)width, '0');
			return x.ToString();
		}

		protected static string SetWidth(string s, int? width) {
			if (width is not null)
				return s.ToString().PadLeft((int)width, ' ');
			return s.ToString();
		}

		protected static string Year(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Year, width);
		}

		protected static string Year2(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Year % 100, width ?? 2);
		}

		protected static string Year4(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Year, width ?? 4);
		}

		protected static string Month(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Month, width);
		}

		protected static string Month2(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Month, width ?? 2);
		}

		protected static string Day(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Day, width);
		}

		protected static string Day2(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Day, width ?? 2);
		}

		protected static string Hour(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Hour, width);
		}

		protected static string Hour2(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Hour, width ?? 2);
		}

		protected static string Minute(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Minute, width);
		}

		protected static string Minute2(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Minute, width ?? 2);
		}

		protected static string Second(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Second, width);
		}

		protected static string Second2(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return SetWidth(GetDateTime(fileInfo, options).Second, width ?? 2);
		}

		protected static string Device(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return options switch {
				null or "name" => SetWidth(fileInfo.DeviceName ?? throw new CancelFileCopyException(), width),
				"make" or "manufacturer" => SetWidth(fileInfo.Manufacturer ?? throw new CancelFileCopyException(), width),
				"model" => SetWidth(fileInfo.Model ?? throw new CancelFileCopyException(), width),
				_ => throw new InvalidFileNameTemplateException($"Unknown device option: {options}"),
			};
		}

		protected static string NoExtension(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			addExtension = false;
			if (options is null)
				return "";
			if (width is not null)
				throw new InvalidFileNameTemplateException($"Cannot specify width of the extension");

			var ext = options;
			if (!ext.StartsWith("."))
				ext = "." + ext;

			if (ext != fileInfo.OriginalFileExtension)
				throw new CancelFileCopyException();
			return "";
		}

		protected static string Extension(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			addExtension = false;
			if (options is null)
				return SetWidth(fileInfo.OriginalFileExtension[1..], width);
			if (width is not null)
				throw new InvalidFileNameTemplateException($"Cannot specify width of the extension");

			var ext = options;
			if (!ext.StartsWith("."))
				ext = "." + ext;

			if (ext != fileInfo.OriginalFileExtension)
				throw new CancelFileCopyException();
			return options;
		}

		protected static string File(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			return options switch {
				null or "name" => SetWidth(fileInfo.OriginalFileName, width),
				"extension" or "ext" => SetWidth(fileInfo.OriginalFileExtension[1..], width),
				"relativePath" or "relPath" or "rel" => SetWidth(fileInfo.OriginalFileRelativePath ?? throw new CancelFileCopyException(), width),
				_ => throw new InvalidFileNameTemplateException($"Unknown file options: {options}")
			};
		}

		protected static string Unicode(FileInfo fileInfo, string? options, int? width, ref bool addExtension) {
			if (width is not null)
				throw new InvalidFileNameTemplateException($"Cannot specify width of a unicode character");
			if (options is null)
				throw new InvalidFileNameTemplateException($"Unicode character code must be specified");
			if (!int.TryParse(options, NumberStyles.HexNumber, null, out int code))
				throw new InvalidFileNameTemplateException($"Unicode character code must be a hexadecimal number");
			return char.ConvertFromUtf32(code);
		}
	}

	/// <summary>
	/// The supplied file name template for <see cref="TemplateFileNameProvider"/> is invalid.
	/// </summary>
	public class InvalidFileNameTemplateException : Exception {
		public InvalidFileNameTemplateException(string msg) : base(msg) { }
	}

	/// <summary>
	/// An <see cref="IFileNameProvider"/> which uses multiple different providers and uses the first successful one.
	/// </summary>
	public class MultipleFileNameProvider : IFileNameProvider {
		protected List<IFileNameProvider> FileNameProviders { get; }

		public MultipleFileNameProvider(IEnumerable<IFileNameProvider> fileNameProviders) {
			FileNameProviders = fileNameProviders.ToList();
		}

		public string GetFileName(FileInfo fileInfo, bool addExtension = true) {
			foreach (var provider in FileNameProviders) {
				try {
					return provider.GetFileName(fileInfo, addExtension);
				} catch (CancelFileCopyException) { }
			}
			throw new CancelFileCopyException();
		}
	}
}
