using AlbumLibrary;
using AlbumLibrary.CLI;
using System.Text.RegularExpressions;

namespace AlbumConsole {
	/// <summary>
	/// Class which defines a command which can be run. This includes the arguments the command takes and the code that will be run.
	/// </summary>
	public class Command {
		public static CLIDefinition UniversalParameters { get; } = new CLIDefinition("general parameters", new List<ArgumentDefinition> {
			new ArgumentDefinition("verbose", 'v', CLIArgumentType.Flag, new FlagArgument(false), false),
			new ArgumentDefinition("help", '?', CLIArgumentType.Flag, new FlagArgument(false), false),
			new ArgumentDefinition("album-dir", 'd', CLIArgumentType.String, new StringArgument("."), false)
		});

		public static Dictionary<string, Command> Commands { get; } = new Dictionary<string, Command> {
			{ "help", new Command("help", "Prints information about available commands", new List<ArgumentDefinition> {
				new ArgumentDefinition("command", 'c', CLIArgumentType.String, new StringArgument(""), true)
			}, DefaultCommandsActions.Help) },
			{ "debug", new Command("debug", "For debugging arguments parsing", new List<ArgumentDefinition> {
				new ArgumentDefinition("test-implicit", ' ', CLIArgumentType.String, null, true),
				new ArgumentDefinition("test-number", 'n', CLIArgumentType.Number, new NumberArgument(0), false),
				new ArgumentDefinition("test-files", 'f', CLIArgumentType.Files, new FilesArgument(), false),
			}, args => {
				Console.WriteLine($"Exe dir    = {args.ExecutableDirectory}");
				Console.WriteLine($"Album dir  = {args.AlbumDirectory}");
				Console.WriteLine($"Command    = {args.Command}");
				Console.WriteLine($"Named args:\n    {string.Join("\n    ", from x in args.NamedArguments select $"{x.Key} = {x.Value}")}");
				return new CommandResult(true);
			}) },
			{ "interactive", new Command("interactive", "Allows you to run multiple commands after each other, " +
				"propagates the following parameters: verbose, album-dir", new List<ArgumentDefinition> {
				new ArgumentDefinition("strict", 's', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Interactive) },
			{ "metadata", new Command("metadata", "Show the metadata of specified images. Use --verbose to show all available metadata.",
				new List<ArgumentDefinition> {
				new ArgumentDefinition("files", 'f', CLIArgumentType.Files, null, true)
			}, DefaultCommandsActions.Metadata) }
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
			this(name, description, new CLIDefinition($"{name} parameters", argsDef, new List<CLIDefinition> { UniversalParameters }), action) { }

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

		public static CLIDefinition GetCLIDefinition(string command) {
			if (!Commands.ContainsKey(command))
				return UniversalParameters;
			return Commands[command].Parameters;
		}
	}

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
	/// A class containing the code from the default commands
	/// </summary>
	public static class DefaultCommandsActions {
		public static CommandResult Help(CommandArguments args) {
			var cmd = args.GetArgument<StringArgument>("command").Value;
			var verbose = args.GetArgument<FlagArgument>("verbose").IsSet;
			if (string.IsNullOrEmpty(cmd)) {
				Console.WriteLine("Use 'help {command}' or 'command --help' to learn more about specific commands\n" +
					(verbose ? "" : "Run 'help' with the --verbose option to learn more about {general parameters}\n"));
				Console.WriteLine("Here is a list of available commands:\n");
				foreach (var kv in Command.Commands) {
					Console.WriteLine(kv.Value.GetFullDescription(verbose ? -1 : 0));
				}
			} else {
				if (!Command.Commands.ContainsKey(cmd))
					return new CommandResult(false, $"There is no such command: {cmd}");
				Console.WriteLine(Command.Commands[cmd].GetFullDescription());
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
				var input = new Regex(@"\s+").Split(cmd);
				var newArgs = (new string[] {
					args.ExecutableDirectory + Path.DirectorySeparatorChar, input[0], "--album-dir", dir
				}).Concat(input[1..]);

				try {
					args = CommandArguments.ParseArguments(verbose ? newArgs.Concat(new string[] { "--verbose" }).ToArray() : newArgs.ToArray());
				} catch (CLIArgumentException e) {
					if (strict)
						return new CommandResult(false, new List<string> {
							e.Message,
							$"Successfully ran {counter} command" + (counter == 1 ? "" : "s")
						});
					Console.WriteLine($"Error: {e.Message}");
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
			var errHandler = new ErrorListHandler();
			var files = ImportFilePathProvider.Process(args.GetArgument<FilesArgument>("files").Files,
				new HashSet<string> { ".jpg" }, errHandler);
			if (errHandler.IsError)
				return new CommandResult(false, errHandler.GetUnprocessed().ToList());

			var fs = new DirectoryFileSystemProvider(".");

			var verbose = args.GetArgument<FlagArgument>("verbose").IsSet;
			if (verbose) {
				foreach (var file in files.GetFilePaths(fs, errHandler)) {
					Console.WriteLine($"{file}:");
					foreach (var dir in fs.GetFileInfo(file)) {
						Console.WriteLine($"\t{dir.Name}:");
						foreach (var tag in dir.Tags) {
							Console.WriteLine($"\t\t{tag.Name} = {tag.Description}");
						}
					}
					Console.WriteLine("Test FileInfoProvider:");
					var infoProvider = new NormalFileInfoProvider();
					var info = infoProvider.GetInfo(file, fs);
					Console.WriteLine($"\tDate/Time = {info.SuitableDateTime}");
					Console.WriteLine($"\tDevice = {info.DeviceName}");
					Console.WriteLine();
				}
			} else {
				foreach (var file in files.GetFilePaths(fs, errHandler)) {
					Console.WriteLine($"{file}:");
					var infoProvider = new NormalFileInfoProvider();
					var info = infoProvider.GetInfo(file, fs);
					Console.WriteLine($"\tDate/Time = {info.SuitableDateTime}");
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
	}
}
