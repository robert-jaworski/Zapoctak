using AlbumLibrary.CLI;

namespace AlbumConsole {
	internal class CommandArguments {
		public string ExecutableDirectory { get; }
		public string AlbumDirectory { get; }
		public string Command { get; }
		public List<string> UnnamedArguments { get; }
		public Dictionary<string, IArgument> NamedArguments { get; }

		public static CLIDefinition UniversalParameters { get; } = new CLIDefinition(new List<ArgumentDefinition>{
			new ArgumentDefinition("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false)),
			new ArgumentDefinition("album-dir", 'd', CLIArgumentType.String, new StringArgument("."))
		});

		public static Dictionary<string, CLIDefinition> CommandParameters { get; } = new Dictionary<string, CLIDefinition>();

		public CommandArguments(string executableDirectory, string albumDirectory, string command,
				List<string> unnamed, Dictionary<string, IArgument> named) {
			ExecutableDirectory = executableDirectory;
			AlbumDirectory = albumDirectory;
			Command = command;
			UnnamedArguments = unnamed;
			NamedArguments = named;
		}

		public static CommandArguments ParseArguments(string[] args) {
			if (args.Length == 0)
				throw new ArgumentException("No arguments supplied");
			var exeDir = Path.GetDirectoryName(Path.GetFullPath(args[0]));
			if (exeDir == null)
				throw new ArgumentException("Unexpected error");
			var albumDir = Path.GetFullPath(".");
			var cmd = "help";
			Arguments parsedArgs;
			if (args.Length >= 2) {
				cmd = args[1];
				var parameters = UniversalParameters;
				if (CommandParameters.ContainsKey(cmd)) {
					parameters = CommandParameters[cmd];
				}

				parsedArgs = parameters.GetArguments(args[2..]);

				if (parsedArgs.NamedArgs.ContainsKey("album-dir")) {
					albumDir = Path.GetFullPath(((StringArgument)parsedArgs.NamedArgs["album-dir"]).Value);
				}
			} else {
				parsedArgs = UniversalParameters.GetArguments(Array.Empty<string>());
			}

			return new CommandArguments(exeDir, albumDir, cmd, parsedArgs.UnnamedArgs, parsedArgs.NamedArgs);
		}
	}
}
