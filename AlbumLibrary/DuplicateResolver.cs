using System.Security.Cryptography;

namespace AlbumLibrary {
	public interface IDuplicateResolver {
		public ImportItem ResolveDuplicate(ImportItem item, IFileSystemProvider fileSystem);

		public IEnumerable<ImportItem> GetAlternatives(ImportItem item, IFileSystemProvider fileSystem);
	}

	public class SkipDuplicateResolver : IDuplicateResolver {
		public IEnumerable<ImportItem> GetAlternatives(ImportItem item, IFileSystemProvider fileSystem) {
			yield break;
		}

		public ImportItem ResolveDuplicate(ImportItem item, IFileSystemProvider fileSystem) {
			return item.Cancel();
		}
	}

	public class SuffixDuplicateResolver : IDuplicateResolver {
		protected string Suffixes { get; }
		protected string FallbackSuffix { get; }

		public SuffixDuplicateResolver(string suffixes = "abcdefghijklmnopqrstuvwxyz", string fallbackSuffix = "z") {
			Suffixes = suffixes;
			FallbackSuffix = fallbackSuffix;
		}

		public ImportItem ResolveDuplicate(ImportItem item, IFileSystemProvider fileSystem) {
			foreach (var letter in Suffixes) {
				var newPath = AddSuffix(item.DestinationPath, letter.ToString());
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

	public class HashDuplicateResolver : IDuplicateResolver {
		public IDuplicateResolver FallbackResolver { get; }

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

			return item.Cancel();
		}

		public IEnumerable<ImportItem> GetAlternatives(ImportItem item, IFileSystemProvider fileSystem) {
			yield break;
		}
	}
}
