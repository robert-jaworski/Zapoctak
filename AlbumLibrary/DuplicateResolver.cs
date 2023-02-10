using System.Security.Cryptography;

namespace AlbumLibrary {
	/// <summary>
	/// An interface for determining what to do when an file with the designated name already exists.
	/// </summary>
	public interface IDuplicateResolver {
		/// <summary>
		/// Whether this resolver checks if the original file and the new file share the same contents.
		/// </summary>
		public bool ChecksFileContents { get; }

		/// <summary>
		/// Determines what to do with a name conflict.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="fileSystem"></param>
		/// <returns></returns>
		public ImportItem ResolveDuplicate(ImportItem item, IFileSystemProvider fileSystem);

		/// <summary>
		/// Returns available name alternatives.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="fileSystem"></param>
		/// <returns></returns>
		public IEnumerable<ImportItem> GetAlternatives(ImportItem item, IFileSystemProvider fileSystem);
	}

	/// <summary>
	/// When a name collision occurs, skip the file that caused it.
	/// </summary>
	public class SkipDuplicateResolver : IDuplicateResolver {
		public bool ChecksFileContents => false;

		public IEnumerable<ImportItem> GetAlternatives(ImportItem item, IFileSystemProvider fileSystem) {
			yield break;
		}

		public ImportItem ResolveDuplicate(ImportItem item, IFileSystemProvider fileSystem) {
			return item.Cancel();
		}
	}

	/// <summary>
	/// Overwrites the file with the same name.
	/// Has options for checking the modification date and keeping the newer file and for checking the file contents.
	/// </summary>
	public class OverwriteDuplicateResolver : IDuplicateResolver {
		public bool CheckModificationDate { get; }
		public bool HashFiles { get; }

		public bool ChecksFileContents => HashFiles;

		public OverwriteDuplicateResolver(bool checkModificationDate, bool hashFiles) {
			CheckModificationDate = checkModificationDate;
			HashFiles = hashFiles;
		}

		public IEnumerable<ImportItem> GetAlternatives(ImportItem item, IFileSystemProvider fileSystem) {
			yield break;
		}

		public ImportItem ResolveDuplicate(ImportItem item, IFileSystemProvider fileSystem) {
			if (CheckModificationDate) {
				if (item.Info.FileModification > fileSystem.GetFileModification(item.DestinationPath))
					return item.MakeOverwrite();
			}
			if (HashFiles) {
				using var md5 = MD5.Create();

				byte[] srcHash, destHash;
				using (var file = fileSystem.OpenFile(item.SourcePath)) {
					srcHash = md5.ComputeHash(file);
				}
				using (var file = fileSystem.OpenFile(item.DestinationPath)) {
					destHash = md5.ComputeHash(file);
				}

				if (srcHash.Zip(destHash).All(x => x.First == x.Second))
					return item.MarkDuplicate();
				return item.MakeOverwrite();
			}
			if (!CheckModificationDate)
				return item.MakeOverwrite();
			return item.Cancel();
		}
	}

	/// <summary>
	/// Adds a suffix to the name until a suitable name is found.
	/// </summary>
	public class SuffixDuplicateResolver : IDuplicateResolver {
		protected IEnumerable<string> Suffixes { get; }
		protected string FallbackSuffix { get; }

		public bool ChecksFileContents => false;

		public SuffixDuplicateResolver(IEnumerable<string> suffixes, string fallbackSuffix) {
			Suffixes = suffixes;
			FallbackSuffix = fallbackSuffix;
		}

		public SuffixDuplicateResolver(string suffixes = "abcdefghijklmnopqrstuvwxyz", string fallbackSuffix = "z") :
			this(suffixes.ToCharArray().Select(x => x.ToString()), fallbackSuffix) { }

		public static IEnumerable<string> GetNumberSuffixes(string separator = "-") {
			for (var i = 1; ; i++) {
				yield return $"{separator}{i}";
			}
		}

		public ImportItem ResolveDuplicate(ImportItem item, IFileSystemProvider fileSystem) {
			foreach (var suffix in Suffixes) {
				var newPath = AddSuffix(item.DestinationPath, suffix);
				if (!fileSystem.FileExists(newPath)) {
					return item.ChangeDestination(newPath);
				}
			}
			return ResolveDuplicate(item.ChangeDestination(AddSuffix(item.DestinationPath, FallbackSuffix)), fileSystem);
		}

		public static string AddSuffix(string path, string suffix) {
			return path[..^Path.GetExtension(path).Length] + suffix + Path.GetExtension(path);
		}

		public IEnumerable<ImportItem> GetAlternatives(ImportItem item, IFileSystemProvider fileSystem) {
			foreach (var letter in Suffixes) {
				yield return item.Copy().ChangeDestination(AddSuffix(item.DestinationPath, letter.ToString()));
			}
			foreach (var i in GetAlternatives(item.ChangeDestination(AddSuffix(item.DestinationPath, FallbackSuffix)), fileSystem)) {
				yield return i;
			}
		}
	}

	/// <summary>
	/// Hashes the contents of the files and compares them. If they are the same the file is skipped.
	/// Otherwise a fallback resolver is used to try different names until a match or an unused name is found.
	/// </summary>
	public class HashDuplicateResolver : IDuplicateResolver {
		public IDuplicateResolver FallbackResolver { get; }

		public bool ChecksFileContents => true;

		public HashDuplicateResolver(IDuplicateResolver fallbackResolver) {
			FallbackResolver = fallbackResolver;
		}

		public ImportItem ResolveDuplicate(ImportItem item, IFileSystemProvider fileSystem) {
			using var md5 = MD5.Create();

			byte[] srcHash, destHash;
			using (var file = fileSystem.OpenFile(item.SourcePath)) {
				srcHash = md5.ComputeHash(file);
			}
			using (var file = fileSystem.OpenFile(item.DestinationPath)) {
				destHash = md5.ComputeHash(file);
			}

			if (srcHash.Zip(destHash).All(x => x.First == x.Second))
				return item.MarkDuplicate();

			foreach (var i in FallbackResolver.GetAlternatives(item, fileSystem)) {
				if (!fileSystem.FileExists(i.DestinationPath))
					return i;

				using (var file = fileSystem.OpenFile(i.DestinationPath)) {
					destHash = md5.ComputeHash(file);
				}

				if (srcHash.Zip(destHash).All(x => x.First == x.Second))
					return i.MarkDuplicate();
			}

			return FallbackResolver.ResolveDuplicate(item, fileSystem);
		}

		public IEnumerable<ImportItem> GetAlternatives(ImportItem item, IFileSystemProvider fileSystem) {
			yield break;
		}
	}
}
