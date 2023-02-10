namespace AlbumLibrary {
	/// <summary>
	/// An interface for handling errors (log them to the console, save them into a list, fail the test).
	/// </summary>
	public interface IErrorHandler {
		/// <summary>
		/// Handle a new error.
		/// </summary>
		/// <param name="message"></param>
		public void Error(string message);

		/// <summary>
		/// Returns the yet unprocessed errors.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetUnprocessed();

		/// <summary>
		/// Returns all errors which were handled by this <see cref="IErrorHandler"/>
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetAll();

		/// <summary>
		/// Determines whether there is an unprocessed error.
		/// </summary>
		public bool IsError { get; }

		/// <summary>
		/// Determines whether any error has occurred.
		/// </summary>
		public bool WasError { get; }
	}

	/// <summary>
	/// An <see cref="IErrorHandler"/> which saves the errors into a list.
	/// </summary>
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
			Errors = new();
			return res;
		}

		public IEnumerable<string> GetAll() {
			return AllErrors;
		}
	}

	/// <summary>
	/// An exception used by <see cref="ErrorAbortHandler"/>
	/// </summary>
	public class AbortException : Exception {
		public AbortException(string msg) : base(msg) { }
	}

	/// <summary>
	/// Throws <see cref="AbortException"/> when an error occurs.
	/// </summary>
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

	/// <summary>
	/// Logs the errors to the console as they occur.
	/// </summary>
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
