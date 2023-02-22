using AlbumLibrary;
using System.Text.RegularExpressions;

namespace AlbumConsole {
	public record ArgsDef(string name, char shortName, CLIArgumentType type, IArgument? defaultValue,
			bool isImplicit);

	/// <summary>
	/// Class which defines a command which can be run. This includes the arguments the command takes and the code that will be run.
	/// </summary>
	public class Command {
		public static Dictionary<string, Command> Commands { get; } = new Dictionary<string, Command> {
			{ "help", new Command("help", "Prints information about available commands", new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("command", 'c', CLIArgumentType.String, new StringArgument(""), true),
				new ArgsDef("help-config", 'g', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("all-commands", 'a', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("list-profiles", 'l', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Help) },
			{ "debug", new Command("debug", "For debugging arguments parsing", new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("test-implicit", ' ', CLIArgumentType.String, null, true),
				new ArgsDef("test-number", 'n', CLIArgumentType.Number, new NumberArgument(0), false),
				new ArgsDef("test-files", 'f', CLIArgumentType.Files, new FilesArgument(), false),
			}, args => {
				Console.WriteLine($"Exe dir    = {args.ExecutableDirectory}");
				Console.WriteLine($"Album dir  = {args.AlbumDirectory}");
				Console.WriteLine($"Command    = {args.Command}");
				Console.WriteLine($"Named args:\n    {string.Join("\n    ", from x in args.NamedArguments select $"{x.Key} = {x.Value}")}");
				return new CommandResult(true);
			}) },
			{ "interactive", new Command("interactive", "Allows you to run multiple commands after each other, " +
				"propagates the following parameters: verbose, album-dir", new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("strict", 's', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Interactive) },
			{ "metadata", new Command("metadata", "Show the metadata of specified images. Use --verbose to show all available metadata.",
				new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("files", 'f', CLIArgumentType.Files, null, true),
				new ArgsDef("extensions", 'x', CLIArgumentType.String, new StringArgument(@".jpg"), false),
				new ArgsDef("count", 'c', CLIArgumentType.Number, new NumberArgument(-1), false),
				new ArgsDef("long-names", 'l', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Metadata) },
			{ "import", new Command("import", "Imports specified files into the album.",
				new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("files", 'f', CLIArgumentType.Files, null, true),
				new ArgsDef("extensions", 'x', CLIArgumentType.String, new StringArgument(@".jpg"), false),
				new ArgsDef("template", 't', CLIArgumentType.String, new StringArgument(@"{YYYY}/{MM}/{YY}{MM}{DD}-{hh}{mm}{ss}"), false),
				new ArgsDef("time-shift", 's', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("after-date", 'a', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("before-date", 'b', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("filter", 'j', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("hash-duplicates", 'h', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("use-index", 'i', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("yes", 'y', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no", 'n', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("long-names", 'l', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no-exif-date", ' ', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no-suffix", ' ', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no-undo", ' ', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Import) },
			{ "export", new Command("export", "Exports specified files from the album to the specified directory. " +
				"By default targets all files in the album. This command does not support undo.",
				new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("export-to", 'e', CLIArgumentType.String, null, true),
				new ArgsDef("files", 'f', CLIArgumentType.Files, new FilesArgument(), true),
				new ArgsDef("extensions", 'x', CLIArgumentType.String, new StringArgument(@".jpg"), false),
				new ArgsDef("template", 't', CLIArgumentType.String, new StringArgument(@"{file:name}"), false),
				new ArgsDef("after-date", 'a', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("before-date", 'b', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("filter", 'j', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("use-index", 'i', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("yes", 'y', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no", 'n', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("long-names", 'l', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("use-exif-date", ' ', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Export) },
			{ "change", new Command("change", "Changes names and dates of specified files in the album. By default targets all files in the album. " +
				"Ignores EXIF and uses only the file creation date.",
				new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("files", 'f', CLIArgumentType.Files, new FilesArgument(), true),
				new ArgsDef("extensions", 'x', CLIArgumentType.String, new StringArgument(@".jpg"), false),
				new ArgsDef("template", 't', CLIArgumentType.String, new StringArgument(@"{YYYY}/{MM}/{YY}{MM}{DD}-{hh}{mm}{ss}"), false),
				new ArgsDef("time-shift", 's', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("after-date", 'a', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("before-date", 'b', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("filter", 'j', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("hash-duplicates", 'h', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("use-index", 'i', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("yes", 'y', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no", 'n', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("long-names", 'l', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("use-exif-date", ' ', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no-suffix", ' ', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no-undo", ' ', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Change) },
			{ "backup", new Command("backup", "Backs up the specified files in the album. By default targets all files in the album. " +
				"Ignores EXIF and uses only the file creation date. This command does not support undo.",
				new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("backup-to", 'e', CLIArgumentType.String, null, true),
				new ArgsDef("files", 'f', CLIArgumentType.Files, new FilesArgument(), true),
				new ArgsDef("extensions", 'x', CLIArgumentType.String, new StringArgument(@".jpg"), false),
				new ArgsDef("after-date", 'a', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("before-date", 'b', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("filter", 'j', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("use-index", 'i', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("yes", 'y', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no", 'n', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("long-names", 'l', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("use-exif-date", ' ', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("hash-duplicates", 'h', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Backup) },
			{ "delete", new Command("delete", "Deletes the specified files in the album. By default targets all files in the album. " +
				"Ignores EXIF and uses only the file creation date.",
				new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("files", 'f', CLIArgumentType.Files, new FilesArgument(), true),
				new ArgsDef("extensions", 'x', CLIArgumentType.String, new StringArgument(@".jpg"), false),
				new ArgsDef("after-date", 'a', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("before-date", 'b', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("filter", 'j', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("use-index", 'i', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("yes", 'y', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no", 'n', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("long-names", 'l', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("use-exif-date", ' ', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no-undo", ' ', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Delete) },
			{ "history", new Command("history", "Shows saved history.",
				new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("clear", 'c', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("clear-undo", 'u', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("clear-redo", 'r', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("yes", 'y', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no", 'n', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("long-names", 'l', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.History) },
			{ "undo", new Command("undo", "Undo last operation.",
				new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("use-index", 'i', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("long-names", 'l', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Undo) },
			{ "redo", new Command("redo", "Redo last undone operation.",
				new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("use-index", 'i', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("long-names", 'l', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Redo) },
			{ "index", new Command("index", "Loads information about specified files and saves it to the index file.",
				new List<ArgsDef> {
				new ArgsDef("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false),
				new ArgsDef("profile", 'p', CLIArgumentType.String, new StringArgument("default"), false),
				new ArgsDef("clear-index", 'c', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("files", 'f', CLIArgumentType.Files, new FilesArgument(), true),
				new ArgsDef("extensions", 'x', CLIArgumentType.String, new StringArgument(@".jpg"), false),
				new ArgsDef("after-date", 'a', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("before-date", 'b', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("filter", 'j', CLIArgumentType.String, new StringArgument(""), false),
				new ArgsDef("yes", 'y', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("no", 'n', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("long-names", 'l', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgsDef("use-exif-date", ' ', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.UpdateIndex) },
		};

		public string Name { get; }
		public string Description { get; }

		public delegate CommandResult CommandAction(CommandArguments args);
		public CommandAction Action { get; }

		public CLIDefinition Parameters { get; }

		protected Command(string name, string description, CLIDefinition cli, CommandAction action) {
			Name = name;
			Description = description;
			Parameters = cli;
			Action = action;
		}

		public Command(string name, string description, List<ArgumentDefinition> argsDef, CommandAction action) :
			this(name, description, new CLIDefinition($"{name} parameters", argsDef, new List<CLIDefinition> { }), action) { }

		public Command(string name, string description, List<ArgsDef> argsDef, CommandAction action) :
			this(name, description, argsDef.Select(x => new ArgumentDefinition(x.name, x.shortName, x.type,
				new ConfigDefaultValueProvider(name, x.name, x.type, x.defaultValue), x.isImplicit)).ToList(), action) { }

		public string GetFullDescription(int recurse = -1) {
			return $"{Name} {Parameters.GetFullDescription(recurse)}\n\t{Description}\n";
		}

		public static CommandResult RunCommand(CommandArguments args) {
			if (!Commands.ContainsKey(args.Command))
				return new CommandResult(false, $"Unknown command {args.Command}\nTry running the 'help' command");

			if (args.GetArgument<FlagArgument>("help").IsSet) {
				args.NamedArguments["command"] = new StringArgument(args.Command);
				return Commands["help"].Action(args);
			}

			return Commands[args.Command].Action(args);
		}

		public static CLIDefinition? GetCLIDefinition(string command) {
			if (!Commands.ContainsKey(command))
				return null;
			return Commands[command].Parameters;
		}
	}

	/// <summary>
	/// A class which describes a result of a command - whether it was successful and
	/// all the additional messages describing the result of the command.
	/// </summary>
	public class CommandResult {
		public bool Success { get; }
		public List<string> Messages { get; }

		public CommandResult(bool success, List<string> messages) {
			Success = success;
			Messages = messages;
		}

		public CommandResult(bool success, string message) : this(success, new List<string> { message }) { }

		public CommandResult(bool success) : this(success, new List<string>()) { }
	}

	public class InternalException : Exception {
		public InternalException(string msg) : base(msg) { }
	}

	/// <summary>
	/// An <see cref="ILogger"/> which logs all information to the console.
	/// </summary>
	public class ConsoleLogger : ILogger {
		public bool LongNames { get; }

		public ConsoleLogger(bool longNames) {
			LongNames = longNames;
		}

		public string GetSuitablePath(string path, IFileSystemProvider fileSystem) {
			return DefaultCommandsActions.GetSuitablePath(path, fileSystem, LongNames);
		}

		public void Write(string message) {
			Console.Write(message);
		}

		public void WriteLine(string message = "") {
			Console.WriteLine(message);
		}
	}

	/// <summary>
	/// A class containing the code from the default commands
	/// </summary>
	public static class DefaultCommandsActions {
		/// <summary>
		/// Determines whether asking the user for confirmation is necessary.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static bool RequiresConfirmation(CommandArguments args) {
			if (args.IsSet("yes")) {
				if (args.IsSet("no-undo") || args.IsSet("no"))
					return true;
				return false;
			}
			if (args.IsSet("no")) {
				return false;
			}
			return true;
		}

		/// <summary>
		/// If necessary, asks the user to confirm a specific action.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="msg"></param>
		/// <returns></returns>
		public static bool Confirm(CommandArguments args, string msg) {
			if (args.IsSet("no-undo")) {
				if (args.IsSet("yes") || args.IsSet("no")) {
					Console.WriteLine("Since --no-undo is set, confirmation is required and cannot be skipped.");
				}
				return ConfirmPrompt(msg);
			}
			if (args.IsSet("yes")) {
				if (args.IsSet("no"))
					return ConfirmPrompt(msg, true);
				return true;
			}
			if (args.IsSet("no")) {
				return false;
			}
			return ConfirmPrompt(msg);
		}

		/// <summary>
		/// Asks the user to confirm a specific action.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="mixed">Whether the user set both <c>--yes</c> and <c>--no</c> flags.</param>
		/// <returns></returns>
		public static bool ConfirmPrompt(string msg, bool mixed = false) {
			if (mixed)
				Console.WriteLine("You are sending mixed messages - both --yes and --no set!");

			Console.Write(msg);
			Console.Write(" (Y/N): ");
			var key = Console.ReadKey();
			Console.WriteLine();
			if (key.Key == ConsoleKey.Y)
				return true;
			if (key.Key == ConsoleKey.N)
				return false;

			while (true) {
				Console.Write("Please type Y or N: ");
				key = Console.ReadKey();
				Console.WriteLine();
				if (key.Key == ConsoleKey.Y)
					return true;
				if (key.Key == ConsoleKey.N)
					return false;
			}
		}

		/// <summary>
		/// Gets the allowed extensions given the command-line arguments.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static HashSet<string> GetExtensions(CommandArguments args) {
			return new HashSet<string>(args.GetArgument<StringArgument>("extensions").Value.Split(',').Select(x => x.StartsWith(".") ? x.ToLower() : "." + x));
		}

		/// <summary>
		/// Returns either the full version or the relative version of <paramref name="path"/>.
		/// If <paramref name="longNames"/> is set, returns the full version of <paramref name="path"/>,
		/// otherwise returns whichever one is shorter.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="fileSystem"></param>
		/// <param name="longNames"></param>
		/// <returns></returns>
		public static string GetSuitablePath(string path, IFileSystemProvider fileSystem, bool longNames) {
			var fullPath = fileSystem.GetFullPath(path);
			var relPath = fileSystem.GetRelativePath(".", fullPath);
			return !longNames && relPath.Length < fullPath.Length ? relPath : fullPath;
		}

		public static CommandResult Help(CommandArguments args) {
			var cmd = args.GetArgument<StringArgument>("command").Value;
			var verbose = args.GetArgument<FlagArgument>("verbose").IsSet;

			var didSomething = false;

			if (args.IsSet("help-config")) {
				var fileSystem = new NormalFileSystemProvider(args.AlbumDirectory);
				var path = fileSystem.GetFullPathAlbum("config");
				using var file = fileSystem.WriteText(path, true);
				file.WriteLine();
				file.WriteLine("; This is a comment.");
				file.WriteLine("[default] ; This defines a profile named 'default', which is always active");
				file.WriteLine("; We can set the verbose flag to be always set and set the default extensions:");
				file.WriteLine("--verbose = true");
				file.WriteLine("--extensions = .jpg,.png");
				file.WriteLine("; We can also specify the default to apply to only certain commands:");
				file.WriteLine("import--files = ./...");
				file.WriteLine("\n[last] ; Let's define another profile");
				file.WriteLine("--files = @last");
				file.WriteLine("--long-names = True");
				file.WriteLine("\n[myphotos] ; And another");
				file.WriteLine("--template = 'My Photos/{YYYY}{MM}{DD}-{hh}{mm}{ss}'");

				Console.WriteLine("Dummy config saved to: {0}", path);
				didSomething = true;
			}
			if (args.IsSet("all-commands")) {
				Console.WriteLine("Here is a list of available commands:\n");
				foreach (var kv in Command.Commands) {
					if (kv.Key != "debug" && kv.Key != "interactive")
						Console.WriteLine(kv.Value.GetFullDescription());
				}
				didSomething = true;
			}
			if (args.IsSet("list-profiles")) {
				if (Config.CurrentConfig is null)
					return new CommandResult(false, "No config loaded");
				Console.WriteLine("Available config profiles:");
				var config = Config.CurrentConfig as Config;
				foreach (var p in Config.CurrentConfig.GetProfiles()) {
					Console.WriteLine("  " + p);
					if (verbose) {
						config?.Print(p);
					}
				}
				didSomething = true;
			}
			if (!string.IsNullOrEmpty(cmd)) {
				if (!Command.Commands.ContainsKey(cmd))
					return new CommandResult(false, $"There is no such command: {cmd}");
				Console.WriteLine(Command.Commands[cmd].GetFullDescription());
				didSomething = true;
			}
			if (!didSomething) {
				Console.WriteLine("You can use several commands to import, change, delete, export and backup files in the album.");
				Console.WriteLine("The album is a directory which can contain several subdirectories and files, usually photos.");
				Console.WriteLine();
				Console.WriteLine("When targeting files you can use several ways to do so:");
				Console.WriteLine("  - The path to the file: 'test/img001.jpg'");
				Console.WriteLine("  - A range of files in a directory: 'test/img001.jpg ... test/img010.jpg'");
				Console.WriteLine("  - A unbounded range: 'test/img010.jpg...' or '...test/img010.jpg'");
				Console.WriteLine("  - An entire directory: 'test/'");
				Console.WriteLine("  - An entire directory recursively: 'test/...'");
				Console.WriteLine("  - On windows you can specify to check all available drives: ':\\DCIM\\'");
				Console.WriteLine("  - When using undo features you can target files which you interacted with previously: '@last'");
				Console.WriteLine("  - For example if you import twice and then want to change these files you can use: '@last2'");
				Console.WriteLine();
				Console.WriteLine("The names which will be assigned to the files after importing are specified using a template.");
				Console.WriteLine("The default template is {YYYY}/{MM}/{YY}{MM}{DD}-{hh}{mm}{ss}, the extension is added automatically.");
				Console.WriteLine("Available placeholder values are:");
				Console.WriteLine("  `year`/`Y`\n  `YYYY`\n  `YY` - last two digits of the year\n  `month`/`M`\n  `MM`\n  `day`/`D`\n  `DD`\n" +
					"  `hour`/`h`\n  `hh`\n  `minute`/`m`\n  `mm`\n  `second`/`s`\n  `ss`\n  `device:name`\n  `device:manufacturer`\n  `device:model`\n" +
					"  `noextension`/`noext` - do not append extension\n  `extension`/`ext`/`.` - specify a desired extension, " +
					"when the file has a different extension, this template will be skipped\n" +
					"  `file`/`file:name`\n  `file:extension`/`file:ext`\n  `file:relativePath`/`file:relPath`/`file:rel`");
				Console.WriteLine("Placeholders can take options ({YYYY:exif}, {DD:create}) and length ({M#3}).");
				Console.WriteLine("Available options for date related placeholders are 'exif', 'create' and 'modify'.");
				Console.WriteLine("When no option is specified for a date related placeholder, either the EXIF date or file creation date will be used.");
				Console.WriteLine("Multiple templates can be used to assign different names to different file types:");
				Console.WriteLine("  {YYYY}/{MM}/{YY}{MM}{DD}-{hh}{mm}{ss}{ext:jpg},png/{file:name}{ext:png}");
				Console.WriteLine("When using this option don't forget to allow different file extensions to be processed.");
				Console.WriteLine("You can do this by setting the -x/--extensions option to '.jpg,.png' for example. The default is '.jpg'");
				Console.WriteLine();
				Console.WriteLine("When two files should share the same name, then:");
				Console.WriteLine("  - If --hash-duplicates is set, then the files are hashed to determine if they are the same.");
				Console.WriteLine("  - If the files are the same, then they are skipped.");
				Console.WriteLine("  - If --hash-duplicates is not set, or if the files are not the same, then a suffix is appended to the name.");
				Console.WriteLine("  - If --no-suffix is set, no suffix is appended and the file is skipped.");
				Console.WriteLine("  - This is repeated until a suitable name is or a duplicate is found.");
				Console.WriteLine();
				Console.WriteLine("The name template syntax is also utilized to filter files using the --filter option.");
				Console.WriteLine("The --filter option should be set to some name template which is evaluated for each file.");
				Console.WriteLine("When the result is in the format 'aaa=bbb' or 'xxx=yyy=zzz' etc., the file will be processed if the equations are true.");
				Console.WriteLine();
				Console.WriteLine("Most actions will require confirmation, you can skip this by setting the --yes or --no flags.");
				Console.WriteLine("Actions such as `import`, `change` and `delete` can also be undone.");
				Console.WriteLine();
				Console.WriteLine("You can setup a config file in the directory with the executable or in the album directory.");
				Console.WriteLine("This config file can be used to specify the default values of command arguments.");
				Console.WriteLine("These default values can be grouped to profiles.");
				Console.WriteLine();
				Console.WriteLine("Run `help -g` to create a dummy config file.");
				Console.WriteLine("Run `help -a` to list all commands.");
				Console.WriteLine("Use 'help {command}' or 'command -?' to learn more about specific commands");
			}

			return new CommandResult(true);
		}

		public static CommandResult Interactive(CommandArguments args) {
			var verbose = args.GetArgument<FlagArgument>("verbose").IsSet;
			var strict = args.GetArgument<FlagArgument>("strict").IsSet;
			var dir = args.GetArgument<StringArgument>("album-dir").Value;
			var counter = 0;
			if (verbose)
				Console.WriteLine("This is the interactive mode, you can write commands different commands " +
					"which will be executed with the same --album-dir setting: " + dir);
			Console.WriteLine("To exit the interactive mode type 'exit'\n");

			do {
				Console.Write("> ");
				var cmd = Console.ReadLine();
				if (cmd is null)
					break;
				if (string.IsNullOrWhiteSpace(cmd))
					continue;

				var parsedArgs = CLIDefinition.ParseArguments(cmd);

				IEnumerable<string> newArgs;
				if (parsedArgs.Any(x => x.StartsWith("--") ? x == "--album-dir" : x.StartsWith("-") && x.Contains('d'))) {
					newArgs = (new string[] {
						args.ExecutableDirectory + Path.DirectorySeparatorChar, parsedArgs[0],
					}).Concat(parsedArgs[1..]);
				} else {
					newArgs = (new string[] {
						args.ExecutableDirectory + Path.DirectorySeparatorChar, parsedArgs[0], "--album-dir", dir
					}).Concat(parsedArgs[1..]);
				}

				if (verbose) {
					if (!parsedArgs.Any(x => x == "-v" || x == "--verbose")) {
						newArgs = newArgs.Concat(new string[] { "--verbose" });
					}
				}

				try {
					args = CommandArguments.ParseArguments(verbose ? newArgs.ToArray() : newArgs.ToArray());
				} catch (CLIArgumentException e) {
					if (strict)
						return new CommandResult(false, new List<string> {
							e.Message,
							$"Successfully ran {counter} command" + (counter == 1 ? "" : "s")
						});
					Console.WriteLine($"Error: {e.Message}");
					// Console.Error.WriteLine(e);
					continue;
				}

				if (args.Command != "exit") {
					if (args.Command == "interactive" && !args.GetArgument<FlagArgument>("help").IsSet) {
						if (strict)
							return new CommandResult(false, new List<string> {
								"You cannot run the 'interactive' command from inside the 'interactive' command",
								$"Successfully ran {counter} command" + (counter == 1 ? "" : "s")
							});
						Console.WriteLine("You cannot run the 'interactive' command from inside the 'interactive' command");
					}

					var res = Command.RunCommand(args);
					if (!res.Success) {
						if (strict)
							return new CommandResult(false, res.Messages.Concat(new List<string> {
								$"Successfully ran {counter} command" + (counter == 1 ? "" : "s")
							}).ToList());
						Console.WriteLine("An error ocurred:");
					}
					res.Messages.ForEach(m => Console.WriteLine(m));
					counter++;
				}
			} while (args.Command != "exit");

			Console.WriteLine();
			return new CommandResult(true, $"Successfully ran {counter} command" + (counter == 1 ? "" : "s"));
		}

		public static CommandResult Metadata(CommandArguments args) {
			var verbose = args.IsSet("verbose");
			var longNames = args.IsSet("long-names");

			var errHandler = new ErrorListHandler();
			var files = ImportFilePathProvider.Process(args.GetArgument<FilesArgument>("files").Files, GetExtensions(args), errHandler);
			if (errHandler.IsError)
				return new CommandResult(false, errHandler.GetUnprocessed().ToList());

			var fs = new NormalFileSystemProvider(".");

			var maxCount = args.GetArgument<NumberArgument>("count").Value;
			var count = 0;

			if (verbose) {
				foreach (var file in files.GetFilePaths(fs, errHandler, new ConsoleLogger(longNames))) {
					if (count++ == maxCount)
						break;
					Console.WriteLine($"{file}:");
					foreach (var dir in fs.GetFileInfo(file)) {
						Console.WriteLine($"\t{dir.Name}:");
						foreach (var tag in dir.Tags) {
							Console.WriteLine($"\t\t{tag.Name} = {tag.Description}");
						}
					}
					Console.WriteLine("Test FileInfoProvider:");
					var infoProvider = new EXIFFileInfoProvider();
					var info = infoProvider.GetInfo(file, fs);
					Console.WriteLine($"\tDate/Time = {info.SuitableDateTime}");
					Console.WriteLine("\tExif Date/Time = " + (info.EXIFDateTime is not null ? info.EXIFDateTime : "Unavailable"));
					Console.WriteLine($"\tDevice = {info.DeviceName}");
					Console.WriteLine();
				}
			} else {
				foreach (var file in files.GetFilePaths(fs, errHandler, new ConsoleLogger(longNames))) {
					if (count++ == maxCount)
						break;
					Console.WriteLine($"{file}:");
					var infoProvider = new EXIFFileInfoProvider();
					var info = infoProvider.GetInfo(file, fs);
					Console.WriteLine($"\tDate/Time = {info.SuitableDateTime}");
					Console.WriteLine("\tExif Date/Time = " + (info.EXIFDateTime is not null ? info.EXIFDateTime : "Unavailable"));
					Console.WriteLine($"\tDevice = {info.DeviceName}");
					Console.WriteLine();
				}
			}

			var msg = new List<string>();
			if (!verbose)
				msg.Add("To display more information use --verbose");

			if (errHandler.IsError)
				return new CommandResult(false, errHandler.GetUnprocessed().Concat(new List<string> { string.Empty }).Concat(msg).ToList());
			return new CommandResult(true, msg);
		}

		/// <summary>
		/// Returns the specified file filters given the command-line arguments.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="fileFilters">The output</param>
		/// <returns><see langword="null"/> if successful, a <see cref="CommandResult"/> signifying failure otherwise.</returns>
		public static CommandResult? GetNormalFilters(CommandArguments args, out List<IFileFilter> fileFilters) {
			fileFilters = new();

			bool doDate = false, doShift = false;

			if (args.HasArgument<StringArgument>("after-date")) {
				var afterDate = args.GetArgument<StringArgument>("after-date").Value;
				if (!string.IsNullOrEmpty(afterDate)) {
					if (DateTime.TryParse(afterDate, out DateTime date))
						fileFilters.Add(new AfterDateFilter(date));
					else
						return new CommandResult(false, $"Failed to parse after date: {afterDate}");
					doDate = true;
				}
			}
			if (args.HasArgument<StringArgument>("before-date")) {
				var beforeDate = args.GetArgument<StringArgument>("before-date").Value;
				if (!string.IsNullOrEmpty(beforeDate)) {
					if (DateTime.TryParse(beforeDate, out DateTime date))
						fileFilters.Add(new BeforeDateFilter(date));
					else
						return new CommandResult(false, $"Failed to parse before date: {beforeDate}");
					doDate = true;
				}
			}
			if (args.HasArgument<StringArgument>("filter")) {
				var template = args.GetArgument<StringArgument>("filter").Value;
				if (!string.IsNullOrEmpty(template)) {
					try {
						fileFilters.Add(new TemplateFilter(template));
					} catch (InvalidFileNameTemplateException e) {
						// Console.Error.WriteLine(e);
						return new CommandResult(false, new List<string> { "Invalid filter template!", e.Message });
					}
				}
			}
			if (args.HasArgument<StringArgument>("time-shift")) {
				var timeShift = args.GetArgument<StringArgument>("time-shift").Value;
				if (!string.IsNullOrEmpty(timeShift)) {
					if (TimeSpan.TryParse(timeShift, out TimeSpan time))
						fileFilters.Add(new TimeShiftFilter(time));
					else
						return new CommandResult(false, $"Failed to parse time shift: {timeShift}");
					doShift = true;
				}
			}

			if (doShift && doDate) {
				Console.WriteLine("Please note that --time-shift gets applied after --after-date and --before-date");
			}

			return null;
		}

		/// <summary>
		/// Returns the appropriate <see cref="IFileSystemProvider"/> given the command-line arguments.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="dir"></param>
		/// <param name="fileSystemProvider">The output</param>
		/// <param name="secondaryDir"></param>
		/// <returns><see langword="null"/> if successful, a <see cref="CommandResult"/> signifying failure otherwise.</returns>
		public static CommandResult? GetFileSystemProvider(CommandArguments args, string dir, out IFileSystemProvider fileSystemProvider,
			string? secondaryDir = null) {
			fileSystemProvider = new NormalFileSystemProvider(dir);
			if (args.IsNotSet("no-undo")) {
				if (secondaryDir is not null)
					fileSystemProvider = new UndoFileSystemProvider(fileSystemProvider, secondaryDir);
				else
					fileSystemProvider = new UndoFileSystemProvider(fileSystemProvider);
			}
			if (args.IsSet("use-index")) {
				var res = GetFileInfoProvider(args, out IFileInfoProvider fileInfoProvider);
				if (res is not null)
					return res;
				if (secondaryDir is not null)
					fileSystemProvider = new IndexingFileSystemProvider(fileSystemProvider, fileInfoProvider, metaDirectory: secondaryDir);
				else
					fileSystemProvider = new IndexingFileSystemProvider(fileSystemProvider, fileInfoProvider);
			}
			return null;
		}

		/// <summary>
		/// Extracts the appropriate <see cref="IImportFilePathProvider"/> from the command-line arguments.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="importFilePathProvider">The output</param>
		/// <param name="defaultProvider"></param>
		/// <returns><see langword="null"/> if successful, a <see cref="CommandResult"/> signifying failure otherwise.</returns>
		public static CommandResult? GetImportFilePathProvider(CommandArguments args, out IImportFilePathProvider importFilePathProvider,
			IImportFilePathProvider? defaultProvider = null) {
			if (defaultProvider is not null && args.GetArgument<FilesArgument>("files").IsDefault) {
				importFilePathProvider = defaultProvider;
				return null;
			}
			IErrorHandler errHandler = new ErrorListHandler();
			importFilePathProvider = ImportFilePathProvider.Process(args.GetArgument<FilesArgument>("files").Files, GetExtensions(args), errHandler);
			if (errHandler.IsError) {
				return new CommandResult(false, errHandler.GetUnprocessed().ToList());
			}
			return null;
		}

		/// <summary>
		/// Returns the appropriate <see cref="IFileInfoProvider"/> given the command-line arguments.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="fileInfoProvider">The output</param>
		/// <returns><see langword="null"/> if successful, a <see cref="CommandResult"/> signifying failure otherwise.</returns>
		public static CommandResult? GetFileInfoProvider(CommandArguments args, out IFileInfoProvider fileInfoProvider) {
			if (args.HasArgument<FlagArgument>("use-exif-date")) {
				if (args.GetArgument<FlagArgument>("use-exif-date").IsSet)
					fileInfoProvider = new EXIFFileInfoProvider();
				else
					fileInfoProvider = new NoEXIFDateFileInfoProvider();
			} else {
				if (args.IsSet("no-exif-date"))
					fileInfoProvider = new NoEXIFDateFileInfoProvider();
				else
					fileInfoProvider = new EXIFFileInfoProvider();
			}
			return null;
		}

		/// <summary>
		/// Returns all files and their destinations, which are to be imported (or exported etc.).
		/// </summary>
		/// <param name="args"></param>
		/// <param name="importFilePathProvider"></param>
		/// <param name="fileFilters"></param>
		/// <param name="fileSystem"></param>
		/// <param name="importItems">The output</param>
		/// <param name="message"></param>
		/// <param name="verb"></param>
		/// <param name="useFileInfoProvider"></param>
		/// <param name="useFileNameProvider"></param>
		/// <returns><see langword="null"/> if successful, a <see cref="CommandResult"/> signifying failure otherwise.</returns>
		public static CommandResult? GetImportItems(CommandArguments args, IImportFilePathProvider importFilePathProvider,
			List<IFileFilter> fileFilters, IFileSystemProvider fileSystem, out IEnumerable<ImportItem> importItems, string message, string verb,
			IFileInfoProvider? useFileInfoProvider = null, IFileNameProvider? useFileNameProvider = null) {
			var verbose = args.GetArgument<FlagArgument>("verbose").IsSet;
			var longNames = args.IsSet("long-names");

			IFileNameProvider fileNameProvider;
			if (useFileNameProvider is not null) {
				fileNameProvider = useFileNameProvider;
			} else {
				try {
					fileNameProvider = TemplateFileNameProvider.MultipleTemplates(args.GetArgument<StringArgument>("template").Value.Split(','));
				} catch (InvalidFileNameTemplateException e) {
					importItems = new List<ImportItem>();
					// Console.Error.WriteLine(e);
					return new CommandResult(false, new List<string> { "Invalid file name template!", e.Message });
				}
			}

			IFileInfoProvider fileInfoProvider;
			if (useFileInfoProvider is not null) {
				fileInfoProvider = useFileInfoProvider;
			} else {
				var res = GetFileInfoProvider(args, out fileInfoProvider);
				if (res is not null) {
					importItems = new List<ImportItem>();
					return res;
				}
			}

			IImportListProvider importListProvider = new ImportListProvider(fileInfoProvider, fileNameProvider);
			if (fileFilters.Count > 0) {
				importListProvider = new FilteredImportListProvider(fileFilters, fileInfoProvider, fileNameProvider);
			}

			IErrorHandler errHandler = new ErrorLogHandler();

			if (RequiresConfirmation(args) || !Confirm(args, "You should never see this message.")) {
				var importList = importListProvider.GetImportList(fileSystem, importFilePathProvider, errHandler, new ConsoleLogger(longNames));
				if (verbose) {
					Console.WriteLine("\nSummary:");
					foreach (var item in importList.AllItems) {
						if (item.Cancelled)
							Console.WriteLine($"{GetSuitablePath(item.SourcePath, fileSystem, longNames)}\t -- Cancelled");
						else
							Console.WriteLine($"{GetSuitablePath(item.SourcePath, fileSystem, longNames)}\t -> " +
								$"{GetSuitablePath(item.DestinationPath, fileSystem, longNames)}");
					}
				}
				Console.WriteLine();
				Console.WriteLine(message);
				Console.WriteLine($"Targeted {importList.AllItems.Count} files. Will {verb} {importList.ImportItems.Count} files," +
					$" {importList.CancelledItems.Count} files have been cancelled.\n");
				if (Confirm(args, "Do you wish to proceed?")) {
					importItems = importList.ImportItems;
				} else {
					importItems = new List<ImportItem>();
					return new CommandResult(true, "Cancelled by user");
				}
			} else {
				importItems = importListProvider.GetImportItems(fileSystem, importFilePathProvider, errHandler, new ConsoleLogger(longNames));
			}

			return null;
		}

		/// <summary>
		/// Returns the appropriate <see cref="IDuplicateResolver"/> given the command-line arguments.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="duplicateResolver">The output</param>
		/// <param name="noSuffix"></param>
		/// <returns><see langword="null"/> if successful, a <see cref="CommandResult"/> signifying failure otherwise.</returns>
		public static CommandResult? GetDuplicateResolver(CommandArguments args, out IDuplicateResolver duplicateResolver, bool noSuffix = false) {
			if (noSuffix || args.IsSet("no-suffix"))
				duplicateResolver = new SkipDuplicateResolver();
			else
				duplicateResolver = new SuffixDuplicateResolver();

			if (args.IsNotSet("dont-hash-duplicates"))
				duplicateResolver = new HashDuplicateResolver(duplicateResolver);

			if (args.IsSet("hash-duplicates"))
				duplicateResolver = new HashDuplicateResolver(duplicateResolver);

			return null;
		}

		/// <summary>
		/// Actually imports (or exports etc.) given files.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="fileImporter"></param>
		/// <param name="items"></param>
		/// <param name="imported">List of successfully imported items</param>
		/// <returns><see cref="ErrorLogHandler"/> with errors that occured (might be empty)</returns>
		public static IErrorHandler DoImport(CommandArguments args, IFileImporter fileImporter, IEnumerable<ImportItem> items,
			out List<ImportItem> imported) {
			IErrorHandler errHandler = new ErrorLogHandler();
			try {
				imported = fileImporter.ImportItems(items, errHandler,
					new ConsoleLogger(args.IsSet("long-names"))).ToList();
			} catch (InvalidOperationException e) {
				imported = new();
				errHandler.Error("Invalid operation:");
				errHandler.Error(e.Message);
				// Console.Error.WriteLine(e);
			}
			return errHandler;
		}

		/// <summary>
		/// Actually imports (or exports etc.) given files.
		/// </summary>
		/// <param name="args"></param>
		/// <param name="fileImporter"></param>
		/// <param name="items"></param>
		/// <param name="imported">List of successfully imported items</param>
		/// <returns><see cref="ErrorLogHandler"/> with errors that occured (might be empty)</returns>
		public static IErrorHandler DoImport(CommandArguments args, IFileImporter fileImporter, IEnumerable<ImportItem> items, string message) {
			var errHandler = DoImport(args, fileImporter, items, out List<ImportItem> imported);

			Console.WriteLine(message, imported.Count);
			return errHandler;
		}

		public static CommandResult Import(CommandArguments args) {
			var longNames = args.IsSet("long-names");

			var res = GetNormalFilters(args, out List<IFileFilter> fileFilters);
			if (res is not null)
				return res;

			res = GetFileSystemProvider(args, args.AlbumDirectory, out IFileSystemProvider fileSystem);
			if (res is not null)
				return res;

			res = GetImportFilePathProvider(args, out IImportFilePathProvider importFilePathProvider);
			if (res is not null)
				return res;

			res = GetImportItems(args, importFilePathProvider, fileFilters, fileSystem, out IEnumerable<ImportItem> items,
				$"Importing files to {GetSuitablePath(args.AlbumDirectory, fileSystem, longNames)}", "import");
			if (res is not null)
				return res;

			res = GetDuplicateResolver(args, out IDuplicateResolver duplicateResolver);
			if (res is not null)
				return res;

			IFileImporter importer = new FileCopyImporter(fileSystem, duplicateResolver, "import");
			var errHandler = DoImport(args, importer, items, "{0} files successfully imported.");

			Console.WriteLine();

			return new CommandResult(true, errHandler.GetAll().ToList());
		}

		public static CommandResult Export(CommandArguments args) {
			var longNames = args.IsSet("long-names");

			var res = GetNormalFilters(args, out List<IFileFilter> fileFilters);
			if (res is not null)
				return res;

			var destination = args.GetArgument<StringArgument>("export-to").Value;
			var fileSystem = new NormalFileSystemProvider(destination);

			res = GetImportFilePathProvider(args, out IImportFilePathProvider importFilePathProvider, new ImportFilePathProvider(new List<IFilePathProvider> {
				new DirectoryFilePathProvider(fileSystem.GetFullPath(args.AlbumDirectory), true, false)
			}, GetExtensions(args)));
			if (res is not null)
				return res;

			res = GetImportItems(args, importFilePathProvider, fileFilters, fileSystem, out IEnumerable<ImportItem> items,
				$"Exporting files from {GetSuitablePath(args.AlbumDirectory, fileSystem, longNames)} to " +
				$"{GetSuitablePath(destination, fileSystem, longNames)}", "export");
			if (res is not null)
				return res;

			IDuplicateResolver duplicateResolver = new SkipDuplicateResolver();

			IFileImporter importer = new FileCopyImporter(fileSystem, duplicateResolver, "export");
			var errHandler = DoImport(args, importer, items, "{0} files successfully exported.");

			Console.WriteLine();

			return new CommandResult(true, errHandler.GetAll().ToList());
		}

		public static CommandResult Change(CommandArguments args) {
			var longNames = args.IsSet("long-names");

			var res = GetNormalFilters(args, out List<IFileFilter> fileFilters);
			if (res is not null)
				return res;

			res = GetFileSystemProvider(args, args.AlbumDirectory, out IFileSystemProvider fileSystem);
			if (res is not null)
				return res;

			res = GetImportFilePathProvider(args, out IImportFilePathProvider importFilePathProvider, new ImportFilePathProvider(new List<IFilePathProvider> {
				new DirectoryFilePathProvider(fileSystem.GetFullPath(args.AlbumDirectory), true, false)
			}, GetExtensions(args)));
			if (res is not null)
				return res;

			res = GetFileInfoProvider(args, out IFileInfoProvider fileInfoProvider);
			if (res is not null)
				return res;

			res = GetImportItems(args, importFilePathProvider, fileFilters, fileSystem, out IEnumerable<ImportItem> items,
				$"Changing files in {GetSuitablePath(args.AlbumDirectory, fileSystem, longNames)}", "change",
				new RelativePathFileInfoProvider(fileInfoProvider, fileSystem.GetFullPath(args.AlbumDirectory)));
			if (res is not null)
				return res;

			res = GetDuplicateResolver(args, out IDuplicateResolver duplicateResolver);
			if (res is not null)
				return res;

			IFileImporter importer = new FileMoveImporter(fileSystem, duplicateResolver, "change");
			var errHandler = DoImport(args, importer, items, "{0} files successfully changed.");

			Console.WriteLine();

			return new CommandResult(true, errHandler.GetAll().ToList());
		}

		public static CommandResult Backup(CommandArguments args) {
			var longNames = args.IsSet("long-names");

			// Copy files
			//Console.WriteLine("Copying files from the album to the destination...");

			var res = GetNormalFilters(args, out List<IFileFilter> fileFilters);
			if (res is not null)
				return res;

			var destination = args.GetArgument<StringArgument>("backup-to").Value;
			var fileSystem = new NormalFileSystemProvider(destination);

			if (fileSystem.GetFullPath(destination).StartsWith(fileSystem.GetFullPath(args.AlbumDirectory)) ||
				fileSystem.GetFullPath(args.AlbumDirectory).StartsWith(fileSystem.GetFullPath(destination))) {
				Console.WriteLine("Neither the album nor the backup destination should include the other one as a subdirectory. Unexpected things could occur.");
				if (!Confirm(args, "Do you still wish to proceed?"))
					return new CommandResult(false, "Cancelled by user");
			}

			res = GetImportFilePathProvider(args, out IImportFilePathProvider importFilePathProvider, new ImportFilePathProvider(new List<IFilePathProvider> {
				new DirectoryFilePathProvider(fileSystem.GetFullPath(args.AlbumDirectory), true, false)
			}, GetExtensions(args)));
			if (res is not null)
				return res;

			res = GetFileInfoProvider(args, out IFileInfoProvider fileInfoProvider);
			if (res is not null)
				return res;

			IFileNameProvider fileNameProvider = new TemplateFileNameProvider(@"{file:rel}{noext}");

			res = GetImportItems(args, importFilePathProvider, fileFilters, fileSystem, out IEnumerable<ImportItem> items,
				$"Copying files from {GetSuitablePath(args.AlbumDirectory, fileSystem, longNames)} to " +
				$"{GetSuitablePath(destination, fileSystem, longNames)}", "back up",
				new RelativePathFileInfoProvider(fileInfoProvider, fileSystem.GetFullPath(args.AlbumDirectory)),
				fileNameProvider);
			if (res is not null)
				return res;

			IDuplicateResolver duplicateResolver = new OverwriteDuplicateResolver(true, args.GetArgument<FlagArgument>("hash-duplicates").IsSet);

			IFileImporter importer = new FileCopyImporter(fileSystem, duplicateResolver, "backup");
			var errHandler = DoImport(args, importer, items, out List<ImportItem> backedUp);

			Console.WriteLine("{0} files successfully backed up.", backedUp.Count);
			Console.WriteLine();

			// Delete files
			//Console.WriteLine("Deleting files from the destination, which are not in the album...");

			var fileFilters2 = fileFilters;

			var fileSystem2 = new NormalFileSystemProvider(args.AlbumDirectory);

			IImportFilePathProvider importFilePathProvider2 = new ImportFilePathProvider(new List<IFilePathProvider> {
				new DirectoryFilePathProvider(fileSystem2.GetFullPath(destination), true, false)
			}, GetExtensions(args));

			IFileNameProvider fileNameProvider2 = fileNameProvider;

			res = GetImportItems(args, importFilePathProvider2, fileFilters2, fileSystem2, out IEnumerable<ImportItem> items2,
				$"Deleting files from {GetSuitablePath(destination, fileSystem, longNames)} which are not in " +
				$"{GetSuitablePath(args.AlbumDirectory, fileSystem, longNames)}", "check",
				new RelativePathFileInfoProvider(fileInfoProvider, fileSystem2.GetFullPath(destination)),
				fileNameProvider2);
			if (res is not null) {
				Console.WriteLine("{0} files successfully backed up.", backedUp.Count);
				return res;
			}

			IDuplicateResolver duplicateResolver2 = new SkipDuplicateResolver();
			if (args.GetArgument<FlagArgument>("hash-duplicates").IsSet)
				duplicateResolver2 = new HashDuplicateResolver(duplicateResolver2);

			IFileImporter importer2 = new FileDeleteImporter(fileSystem2, duplicateResolver2, null);
			var errHandler2 = DoImport(args, importer2, items2, out List<ImportItem> deleted);

			Console.WriteLine("{0} files successfully backed up.", backedUp.Count);
			Console.WriteLine("{0} files successfully deleted.", deleted.Count);
			Console.WriteLine();

			return new CommandResult(true, errHandler.GetAll().Concat(errHandler2.GetAll()).ToList());
		}

		public static CommandResult Delete(CommandArguments args) {
			var longNames = args.IsSet("long-names");

			var res = GetNormalFilters(args, out List<IFileFilter> fileFilters);
			if (res is not null)
				return res;

			res = GetFileSystemProvider(args, args.AlbumDirectory, out IFileSystemProvider fileSystem);
			if (res is not null)
				return res;

			res = GetImportFilePathProvider(args, out IImportFilePathProvider importFilePathProvider, new ImportFilePathProvider(new List<IFilePathProvider> {
				new DirectoryFilePathProvider(fileSystem.GetFullPath(args.AlbumDirectory), true, false)
			}, GetExtensions(args)));
			if (res is not null)
				return res;

			res = GetImportItems(args, importFilePathProvider, fileFilters, fileSystem, out IEnumerable<ImportItem> items,
				$"Deleting files from {GetSuitablePath(args.AlbumDirectory, fileSystem, longNames)}", "import", null,
				new TemplateFileNameProvider("NONE{noext}"));
			if (res is not null)
				return res;

			IDuplicateResolver duplicateResolver = new SkipDuplicateResolver();

			IFileImporter importer = new FileDeleteImporter(fileSystem, duplicateResolver, "delete", true);

			if (args.IsSet("no-undo")) {
				Console.WriteLine("This is probably a bad idea since --no-undo is set.");
				if (!ConfirmPrompt("Do you REALLY wish to delete these files?"))
					return new CommandResult(false, "Cancelled by user");
			}
			var errHandler = DoImport(args, importer, items, "{0} files successfully deleted.");

			Console.WriteLine();

			return new CommandResult(true, errHandler.GetAll().ToList());
		}

		public static CommandResult History(CommandArguments args) {
			var verbose = args.IsSet("verbose");
			var longNames = args.IsSet("long-names");

			var fileSystem = new UndoFileSystemProvider(new NormalFileSystemProvider(args.AlbumDirectory));

			var count = 0;

			Console.WriteLine($"Reading UNDO file {GetSuitablePath(fileSystem.GetUndoFilePath(), fileSystem, longNames)}");
			List<FileSystemTransaction> transactions;
			try {
				transactions = fileSystem.ReadUndoFile().ToList();
				Console.WriteLine("{0} transactions:", transactions.Count);
				foreach (var t in transactions) {
					Console.WriteLine("{0}\t({1})\t -- {2} actions", t.Info, t.Date, t.Actions.Count);
					if (verbose) {
						foreach (var a in t.Actions) {
							if (a is FileSystemMoveAction move) {
								Console.WriteLine("  Move\t\t{0}\t -> {1}", GetSuitablePath(move.FromPath, fileSystem, longNames),
									GetSuitablePath(move.ToPath, fileSystem, longNames));
							} else if (a is FileSystemCopyAction copy) {
								Console.WriteLine("  Copy\t\t{0}\t -> {1}", GetSuitablePath(copy.FromPath, fileSystem, longNames),
									GetSuitablePath(copy.ToPath, fileSystem, longNames));
							} else if (a is FileSystemCreationAction creation) {
								Console.WriteLine("  Creation\t{0}\t{1}\t -> {2}", GetSuitablePath(creation.Path, fileSystem, longNames),
									creation.FromDate, creation.ToDate);
							} else if (a is FileSystemModificationAction modification) {
								Console.WriteLine("  Modification\t{0}\t{1}\t -> {2}", GetSuitablePath(modification.Path, fileSystem, longNames),
									modification.FromDate, modification.ToDate);
							} else {
								Console.WriteLine("  Unknown action");
							}
						}
					}
					count++;
				}
			} catch (Exception e) {
				Console.WriteLine("Error reading UNDO file: {0}", e.Message);
				// Console.Error.WriteLine(e);
			}
			Console.WriteLine();

			Console.WriteLine($"Reading REDO file {GetSuitablePath(fileSystem.GetRedoFilePath(), fileSystem, longNames)}");
			try {
				transactions = fileSystem.ReadUndoFile(true).ToList();
				Console.WriteLine("{0} transactions:", transactions.Count);
				foreach (var t in transactions) {
					Console.WriteLine("{0}\t({1})\t -- {2} actions", t.Info, t.Date, t.Actions.Count);
					if (verbose) {
						foreach (var a in t.Actions) {
							if (a is FileSystemMoveAction move) {
								Console.WriteLine("  Move\t\t{0}\t -> {1}", GetSuitablePath(move.FromPath, fileSystem, longNames),
									GetSuitablePath(move.ToPath, fileSystem, longNames));
							} else if (a is FileSystemCopyAction copy) {
								Console.WriteLine("  Copy\t\t{0}\t -> {1}", GetSuitablePath(copy.FromPath, fileSystem, longNames),
									GetSuitablePath(copy.ToPath, fileSystem, longNames));
							} else if (a is FileSystemCreationAction creation) {
								Console.WriteLine("  Creation\t{0}\t{1}\t -> {2}", GetSuitablePath(creation.Path, fileSystem, longNames),
									creation.FromDate, creation.ToDate);
							} else if (a is FileSystemModificationAction modification) {
								Console.WriteLine("  Modification\t{0}\t{1}\t -> {2}", GetSuitablePath(modification.Path, fileSystem, longNames),
									modification.FromDate, modification.ToDate);
							} else {
								Console.WriteLine("  Unknown action");
							}
						}
					}
					count++;
				}
			} catch (Exception e) {
				Console.WriteLine("Error reading REDO file: {0}", e.Message);
				// Console.Error.WriteLine(e);
			}
			Console.WriteLine();

			if (args.IsSet("clear")) {
				if (Confirm(args, "Do you really wish to clear history?")) {
					File.Delete(fileSystem.GetUndoFilePath());
					File.Delete(fileSystem.GetRedoFilePath());
					Console.WriteLine("History cleared.");
					Console.WriteLine("You can now clear the trash directory: {0}",
						GetSuitablePath(fileSystem.GetTrashDirectoryPath(), fileSystem, longNames));
				} else {
					return new CommandResult(true, "Cancelled by user");
				}
			} else {
				if (args.IsSet("clear-redo")) {
					if (Confirm(args, "Do you really wish to clear redo history?")) {
						File.Delete(fileSystem.GetRedoFilePath());
						Console.WriteLine("Redo history cleared.");
					} else {
						return new CommandResult(true, "Cancelled by user");
					}
				}
				if (args.IsSet("clear-undo")) {
					if (Confirm(args, "Do you really wish to clear undo history?")) {
						File.Delete(fileSystem.GetUndoFilePath());
						Console.WriteLine("Undo history cleared.");
					} else {
						return new CommandResult(true, "Cancelled by user");
					}
				}
			}

			return new CommandResult(true, $"{count} transactions in total");
		}

		public static CommandResult Undo(CommandArguments args) {
			var verbose = args.IsSet("verbose");
			var longNames = args.IsSet("long-names");

			var fileSystem = new UndoFileSystemProvider(new IndexingFileSystemProvider(new NormalFileSystemProvider(args.AlbumDirectory),
				new EXIFFileInfoProvider()));

			var t = fileSystem.Undo(verbose ? new ConsoleLogger(longNames) : new NoLogger());
			if (t is not null) {
				return new CommandResult(true, $"Successfully undid {t.Actions.Count} actions from {t.Date}");
			} else {
				return new CommandResult(false, "Nothing to undo");
			}
		}

		public static CommandResult Redo(CommandArguments args) {
			var verbose = args.IsSet("verbose");
			var longNames = args.IsSet("long-names");

			var fileSystem = new UndoFileSystemProvider(new IndexingFileSystemProvider(new NormalFileSystemProvider(args.AlbumDirectory),
				new EXIFFileInfoProvider()));

			var t = fileSystem.Redo(verbose ? new ConsoleLogger(longNames) : new NoLogger());
			if (t is not null) {
				return new CommandResult(true, $"Successfully redid {t.Actions.Count} actions from {t.Date}");
			} else {
				return new CommandResult(false, "Nothing to redo");
			}
		}

		public static CommandResult UpdateIndex(CommandArguments args) {
			var longNames = args.IsSet("long-names");

			var res = GetNormalFilters(args, out List<IFileFilter> fileFilters);
			if (res is not null)
				return res;

			res = GetFileInfoProvider(args, out IFileInfoProvider fileInfoProvider2);
			if (res is not null)
				return res;

			res = GetFileSystemProvider(args, args.AlbumDirectory, out IFileSystemProvider fileSystem2);
			if (res is not null)
				return res;

			var fileInfoProvider = new RelativePathFileInfoProvider(fileInfoProvider2, fileSystem2.GetFullPath(args.AlbumDirectory));
			var fileSystem = new IndexingFileSystemProvider(fileSystem2, fileInfoProvider, dontReadFile: args.IsSet("clear-index"));

			res = GetImportFilePathProvider(args, out IImportFilePathProvider importFilePathProvider, new ImportFilePathProvider(new List<IFilePathProvider> {
				new DirectoryFilePathProvider(fileSystem.GetFullPath(args.AlbumDirectory), true, false)
			}, GetExtensions(args)));
			if (res is not null)
				return res;

			res = GetImportItems(args, importFilePathProvider, fileFilters, fileSystem2, out IEnumerable<ImportItem> items,
				$"Updating files in {GetSuitablePath(args.AlbumDirectory, fileSystem, longNames)}", "update", fileInfoProvider,
				new TemplateFileNameProvider("INDEX{noext}"));
			if (res is not null)
				return res;

			var count = 0;
			foreach (var f in fileSystem.Index.AllFiles()) {
				fileSystem.UpdateFile(f);
				Console.WriteLine("Check\t{0}", GetSuitablePath(f, fileSystem, longNames));
				count++;
			}
			foreach (var item in items) {
				if (!item.Cancelled) {
					fileSystem.UpdateFile(item.SourcePath);
					Console.WriteLine("Update\t{0}", GetSuitablePath(item.SourcePath, fileSystem, longNames));
					count++;
				}
			}
			fileSystem.WriteIndexFile();

			Console.WriteLine("\nSuccessfully updated {0} files", count);

			return new CommandResult(true);
		}
	}
}
