namespace AlbumConsole {
	/// <summary>
	/// Class which stores the information about all the command line arguments supplied
	/// </summary>
	public class CommandArguments {
		public string ExecutableDirectory { get; }
		public string AlbumDirectory { get; }
		public string Profile { get; }
		public string Command { get; }
		public Dictionary<string, IArgument> NamedArguments { get; }

		public CommandArguments(string executableDirectory, string albumDirectory, string command, Dictionary<string, IArgument> named, string profile) {
			ExecutableDirectory = executableDirectory;
			AlbumDirectory = albumDirectory;
			Command = command;
			NamedArguments = named;
			Profile = profile;
		}

		/// <summary>
		/// Parses given command-line arguments. Expects the first argument to be the name of the executable.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static CommandArguments ParseArguments(string[] args, bool allowIncomplete = false) {
			if (args.Length == 0)
				throw new ArgumentException("No arguments supplied");
			var exeDir = Path.GetDirectoryName(Path.GetFullPath(args[0])) ?? throw new ArgumentException("Unexpected error");
			var albumDir = Path.GetFullPath(".");
			var profile = "default";

			Dictionary<string, IArgument> parsedArgs;
			var cmd = args.Length >= 2 ? args[1] : "help";
			var parameters = AlbumConsole.Command.GetCLIDefinition(cmd);

			if (parameters is null) {
				parsedArgs = new Dictionary<string, IArgument> {
						{ "help", new FlagArgument(true) },
						{ "verbose", new FlagArgument(false) },
						{ "album-dir", new StringArgument(".") },
						{ "profile", new StringArgument("default") },
					};
			} else {
				try {
					parsedArgs = args.Length >= 2 ? parameters.GetArguments(args[2..]) : parameters.GetArguments(Array.Empty<string>());
				} catch (CLIArgumentException e) {
					if (allowIncomplete) {
						parsedArgs = e.ProcessedArgs;
					} else if (e.ProcessedArgs.ContainsKey("help") && e.ProcessedArgs["help"] is FlagArgument { IsSet: true }) {
						parsedArgs = new Dictionary<string, IArgument> {
							{ "help", new FlagArgument(true) },
							{ "verbose", e.ProcessedArgs.ContainsKey("verbose") ?
								(e.ProcessedArgs["verbose"] as FlagArgument) ?? new FlagArgument(false) :
								new FlagArgument(false) },
							{ "album-dir", e.ProcessedArgs.ContainsKey("album-dir") ?
								(e.ProcessedArgs["album-dir"] as StringArgument) ?? new StringArgument(".") :
								new StringArgument(".") },
							{ "profile", e.ProcessedArgs.ContainsKey("profile") ?
								(e.ProcessedArgs["profile"] as StringArgument) ?? new StringArgument(".") :
								new StringArgument(".") },
						};
					} else
						throw;
				}
			}

			if (parsedArgs.ContainsKey("album-dir")) {
				albumDir = Path.GetFullPath(((StringArgument)parsedArgs["album-dir"]).Value);
			}
			if (parsedArgs.ContainsKey("profile")) {
				profile = ((StringArgument)parsedArgs["profile"]).Value;
			}

			return new CommandArguments(exeDir, albumDir, cmd, parsedArgs, profile);
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

		/// <summary>
		/// Determines whether the specified argument exists. That is if the used command specified a default for this argument of if it was set.
		/// </summary>
		/// <typeparam name="T">The expected type</typeparam>
		/// <param name="name">The name of the argument</param>
		/// <returns></returns>
		public bool HasArgument<T>(string name) where T : IArgument {
			if (!NamedArguments.ContainsKey(name))
				return false;
			if (NamedArguments[name] is not T)
				return false;
			return true;
		}

		/// <summary>
		/// Determines whether the specified flag argument is set.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool IsSet(string name) {
			return HasArgument<FlagArgument>(name) && GetArgument<FlagArgument>(name).IsSet;
		}

		/// <summary>
		/// Determines whether the specified flag argument is not set but could be.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool IsNotSet(string name) {
			return HasArgument<FlagArgument>(name) && !GetArgument<FlagArgument>(name).IsSet;
		}
	}
}
