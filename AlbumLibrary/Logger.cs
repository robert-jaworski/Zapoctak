namespace AlbumLibrary {
	public interface ILogger {
		void Write(string message);
		void WriteLine(string message = "");

		string GetSuitablePath(string path, IFileSystemProvider fileSystem);
	}
}
