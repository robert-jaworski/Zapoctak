namespace AlbumLibrary {
	/// <summary>
	/// An interface for logging the process of importig (or exporting etc.) of the files.
	/// </summary>
	public interface ILogger {
		void Write(string message);
		void WriteLine(string message = "");

		/// <summary>
		/// Returns either the full path or some relative path, depending on configuration.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="fileSystem"></param>
		/// <returns></returns>
		string GetSuitablePath(string path, IFileSystemProvider fileSystem);
	}

	/// <summary>
	/// Ignores all logged information.
	/// </summary>
	public class NoLogger : ILogger {
		public string GetSuitablePath(string path, IFileSystemProvider fileSystem) {
			return path;
		}

		public void Write(string message) { }

		public void WriteLine(string message = "") { }
	}
}
