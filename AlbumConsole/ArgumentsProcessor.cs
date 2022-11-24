using AlbumLibrary.CLI;

namespace AlbumConsole {
	public class CommandArguments {
		public string ExecutableDirectory { get; }
		public string AlbumDirectory { get; }
		public string Command { get; }
		public Dictionary<string, IArgument> NamedArguments { get; }

		public static CLIDefinition UniversalParameters { get; } = new CLIDefinition(new List<ArgumentDefinition> {
			new ArgumentDefinition("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
			new ArgumentDefinition("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false)
		});

		public static Dictionary<string, CLIDefinition> CommandParameters { get; } = new Dictionary<string, CLIDefinition> {
			{ "debug", new CLIDefinition(new List<ArgumentDefinition> {
				new ArgumentDefinition("test-implicit", ' ', CLIArgumentType.String, null, true),
				new ArgumentDefinition("test-number", 'n', CLIArgumentType.Number, new NumberArgument(0), false),
				new ArgumentDefinition("test-files", 'f', CLIArgumentType.Files, new FilesArgument(), false),
			}, new List<CLIDefinition> { UniversalParameters }) }
		};

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
			var cmd = "help";
			Dictionary<string, IArgument> parsedArgs;
			if (args.Length >= 2) {
				cmd = args[1];
				var parameters = UniversalParameters;
				if (CommandParameters.ContainsKey(cmd)) {
					parameters = CommandParameters[cmd];
				}

				parsedArgs = parameters.GetArguments(args[2..]);

				if (parsedArgs.ContainsKey("album-dir")) {
					albumDir = Path.GetFullPath(((StringArgument)parsedArgs["album-dir"]).Value);
				}
			} else {
				parsedArgs = UniversalParameters.GetArguments(Array.Empty<string>());
			}

			return new CommandArguments(exeDir, albumDir, cmd, parsedArgs);
		}
	}
}
