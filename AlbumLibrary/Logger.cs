namespace AlbumLibrary {
	public interface ILogger {
		void Write(string message);
		void WriteLine(string message = "");

		string GetSuitablePath(string path, IFileSystemProvider fileSystem);
	}

	public class NoLogger : ILogger {
		public string GetSuitablePath(string path, IFileSystemProvider fileSystem) {
			return path;
		}

		public void Write(string message) { }

		public void WriteLine(string message = "") { }
	}
}
