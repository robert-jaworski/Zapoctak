using AlbumLibrary;

namespace AlbumTest {
	[TestClass]
	public class FileSystemProviderTest {
		[TestMethod]
		public void UndoFileSystemProvider_TypeWidth() {
			Assert.AreEqual(UndoFileSystemProvider.TypeWidth, FileSystemTransaction.TransactionString.Length);
			Assert.AreEqual(UndoFileSystemProvider.TypeWidth, FileSystemTransaction.InfoString.Length);
			Assert.AreEqual(UndoFileSystemProvider.TypeWidth, FileSystemTransaction.NoInfoString.Length);
			Assert.AreEqual(UndoFileSystemProvider.TypeWidth, FileSystemAction.MoveString.Length);
			Assert.AreEqual(UndoFileSystemProvider.TypeWidth, FileSystemAction.CopyString.Length);
			Assert.AreEqual(UndoFileSystemProvider.TypeWidth, FileSystemAction.ToPathString.Length);
			Assert.AreEqual(UndoFileSystemProvider.TypeWidth, FileSystemAction.CreationString.Length);
			Assert.AreEqual(UndoFileSystemProvider.TypeWidth, FileSystemAction.ModificationString.Length);
			Assert.AreEqual(UndoFileSystemProvider.TypeWidth, FileSystemAction.OfPathString.Length);
		}
	}
}
