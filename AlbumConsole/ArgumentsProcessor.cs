using AlbumLibrary.CLI;

namespace AlbumConsole {
	/// <summary>
	/// Class which stores the information about all the command line arguments supplied
	/// </summary>
	public class CommandArguments {
		public string ExecutableDirectory { get; }
		public string AlbumDirectory { get; }
		public string Command { get; }
		public Dictionary<string, IArgument> NamedArguments { get; }

		public CommandArguments(string executableDirectory, string albumDirectory, string command, Dictionary<string, IArgument> named) {
			ExecutableDirectory = executableDirectory;
			AlbumDirectory = albumDirectory;
			Command = command;
			NamedArguments = named;
		}

		public static CommandArguments ParseArguments(string[] args) {
			if (args.Length == 0)
				throw new ArgumentException("No arguments supplied");
			var exeDir = Path.GetDirectoryName(Path.GetFullPath(args[0]));
			if (exeDir == null)
				throw new ArgumentException("Unexpected error");
			var albumDir = Path.GetFullPath(".");

			Dictionary<string, IArgument> parsedArgs;
			var cmd = args.Length >= 2 ? args[1] : "help";
			var parameters = AlbumConsole.Command.GetCLIDefinition(cmd);

			try {
				parsedArgs = args.Length >= 2 ? parameters.GetArguments(args[2..]) : parameters.GetArguments(Array.Empty<string>());
			} catch (CLIArgumentException e) {
				if (e.ProcessedArgs.ContainsKey("help") && (e.ProcessedArgs["help"] as FlagArgument)?.IsSet == true)
					parsedArgs = new Dictionary<string, IArgument> {
						{ "help", new FlagArgument(true) },
						{ "verbose", e.ProcessedArgs.ContainsKey("verbose") ?
							(e.ProcessedArgs["verbose"] as FlagArgument) ?? new FlagArgument(false) :
							new FlagArgument(false) },
						{ "album-dir", new StringArgument(".") },
					};
				else
					throw e;
			}

			if (parsedArgs.ContainsKey("album-dir")) {
				albumDir = Path.GetFullPath(((StringArgument)parsedArgs["album-dir"]).Value);
			}

			return new CommandArguments(exeDir, albumDir, cmd, parsedArgs);
		}

		/// <summary>
		/// Get the specified argument as the specified type.
		/// </summary>
		/// <typeparam name="T">The expected type</typeparam>
		/// <param name="name">The name of the argument</param>
		/// <returns></returns>
		/// <exception cref="InternalException"></exception>
		public T GetArgument<T>(string name) where T : IArgument {
			if (!NamedArguments.ContainsKey(name))
				throw new InternalException($"There is no argument named {name}");
			if (NamedArguments[name] is not T)
				throw new InternalException($"Argument {name} has the wrong type");
			return (T)NamedArguments[name];
		}

		public bool HasArgument<T>(string name) where T : IArgument {
			if (!NamedArguments.ContainsKey(name))
				return false;
			if (NamedArguments[name] is not T)
				return false;
			return true;
		}
	}
}
