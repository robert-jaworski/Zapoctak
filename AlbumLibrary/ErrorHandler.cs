namespace AlbumLibrary {
	public interface IErrorHandler {
		public void Error(string message);

		public IEnumerable<string> GetUnprocessed();
		public IEnumerable<string> GetAll();

		public bool IsError { get; }
		public bool WasError { get; }
	}

	public class ErrorListHandler : IErrorHandler {
		protected List<string> Errors { get; set; } = new();
		protected List<string> AllErrors { get; set; } = new();

		public bool IsError => Errors.Count > 0;
		public bool WasError => AllErrors.Count > 0;

		public void Error(string message) {
			Errors.Add(message);
			AllErrors.Add(message);
		}

		public IEnumerable<string> GetUnprocessed() {
			var res = Errors;
			Errors = new List<string>();
			return res;
		}

		public IEnumerable<string> GetAll() {
			return AllErrors;
		}
	}

	public class AbortException : Exception {
		public AbortException(string msg) : base(msg) { }
	}

	public class ErrorAbortHandler : IErrorHandler {
		public bool IsError => false;
		public bool WasError => false;

		public void Error(string message) {
			throw new AbortException(message);
		}

		public IEnumerable<string> GetUnprocessed() {
			return Array.Empty<string>();
		}

		public IEnumerable<string> GetAll() {
			return Array.Empty<string>();
		}
	}

	public class ErrorLogHandler : IErrorHandler {
		protected List<string> AllErrors { get; set; } = new();

		public bool IsError => false;
		public bool WasError => AllErrors.Count > 0;

		public void Error(string message) {
			AllErrors.Add(message);
			Console.Error.WriteLine(message);
		}

		public IEnumerable<string> GetUnprocessed() {
			return Array.Empty<string>();
		}

		public IEnumerable<string> GetAll() {
			return AllErrors;
		}
	}
}
