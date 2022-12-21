namespace AlbumLibrary {
	public interface IErrorHandler {
		public void Error(string message);

		public IEnumerable<string> GetUnprocessed();

		public bool IsError { get; }
	}

	public class ErrorListHandler : IErrorHandler {
		protected List<string> Errors { get; set; } = new();

		public bool IsError => Errors.Count > 0;

		public void Error(string message) {
			Errors.Add(message);
		}

		public IEnumerable<string> GetUnprocessed() {
			var res = Errors;
			Errors = new List<string>();
			return res;
		}
	}

	public class AbortException : Exception {
		public AbortException(string msg) : base(msg) { }
	}

	public class ErrorAbortHandler : IErrorHandler {
		public bool IsError => false;

		public void Error(string message) {
			throw new AbortException(message);
		}

		public IEnumerable<string> GetUnprocessed() {
			return Array.Empty<string>();
		}
	}
}
