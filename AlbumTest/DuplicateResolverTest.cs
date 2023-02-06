using AlbumLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace AlbumTest {
	[TestClass]
	public class DuplicateResolverTest {
		[TestMethod]
		[DataRow(1, @"C:\testDir\220201-144437.jpg", @"C:\testDir\220201-144437d.jpg", new string[] {
			@"C:\testDir\220201-144437.jpg", @"C:\testDir\220201-144437a.jpg", @"C:\testDir\220201-144437b.jpg", @"C:\testDir\220201-144437c.jpg"
		})]
		[DataRow(2, @"C:\3.jpg", @"C:\3zb.jpg", new string[] {
			@"C:\3.jpg", @"C:\3a.jpg", @"C:\3b.jpg", @"C:\3c.jpg", @"C:\3d.jpg", @"C:\3e.jpg", @"C:\3f.jpg", @"C:\3g.jpg", @"C:\3h.jpg",
			@"C:\3i.jpg", @"C:\3j.jpg", @"C:\3k.jpg", @"C:\3l.jpg", @"C:\3m.jpg", @"C:\3n.jpg", @"C:\3o.jpg", @"C:\3p.jpg", @"C:\3q.jpg",
			@"C:\3r.jpg", @"C:\3s.jpg", @"C:\3t.jpg", @"C:\3u.jpg", @"C:\3v.jpg", @"C:\3w.jpg", @"C:\3x.jpg", @"C:\3y.jpg", @"C:\3z.jpg",
			@"C:\3za.jpg"
		})]
		public void RenameDuplicateResolver_ResolveDuplicate(int id, string path, string expected, string[] files) {
			Logger.LogMessage($"Testing {id}: {path} -> {expected}");

			var item = new ImportItem(new AlbumLibrary.FileInfo(@"C:\test.jpg", null, DateTime.Now, DateTime.Now, null, null, null), path);
			var duplicateResolver = new SuffixDuplicateResolver();

			Assert.AreEqual(expected, duplicateResolver.ResolveDuplicate(item, new TestFileSystemProvider(@"C:\testDir", files)).DestinationPath);
		}

		[TestMethod]
		[DataRow(1, @"C:\testDir\220201-144437.jpg", @"C:\testDir\220201-144437d.jpg", new string[] {
			@"C:\testDir\220201-144437a.jpg", @"C:\testDir\220201-144437b.jpg", @"C:\testDir\220201-144437c.jpg"
		})]
		[DataRow(2, @"C:\3.jpg", @"C:\3zb.jpg", new string[] {
			@"C:\3a.jpg", @"C:\3b.jpg", @"C:\3c.jpg", @"C:\3d.jpg", @"C:\3e.jpg", @"C:\3f.jpg", @"C:\3g.jpg", @"C:\3h.jpg",
			@"C:\3i.jpg", @"C:\3j.jpg", @"C:\3k.jpg", @"C:\3l.jpg", @"C:\3m.jpg", @"C:\3n.jpg", @"C:\3o.jpg", @"C:\3p.jpg", @"C:\3q.jpg",
			@"C:\3r.jpg", @"C:\3s.jpg", @"C:\3t.jpg", @"C:\3u.jpg", @"C:\3v.jpg", @"C:\3w.jpg", @"C:\3x.jpg", @"C:\3y.jpg", @"C:\3z.jpg",
			@"C:\3za.jpg"
		})]
		public void RenameDuplicateResolver_GetAlternatives(int id, string path, string expected, string[] alternatives) {
			Logger.LogMessage($"Testing {id}: {path}");

			var item = new ImportItem(new AlbumLibrary.FileInfo(@"C:\test.jpg", null, DateTime.Now, DateTime.Now, null, null, null), path);
			var duplicateResolver = new SuffixDuplicateResolver();

			var provided = duplicateResolver.GetAlternatives(item, new TestFileSystemProvider(@"C:\testDir",
				alternatives.Concat(new string[] { path }))).Take(alternatives.Length + 1);

			Assert.IsTrue(alternatives.Concat(new string[] { expected }).Zip(provided).All(x => x.First == x.Second.DestinationPath));
		}
	}
}
