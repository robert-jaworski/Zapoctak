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
				new ArgumentDefinition("files", 'f', CLIArgumentType.Files, null, true),
				new ArgumentDefinition("extensions", 'x', CLIArgumentType.String, new StringArgument(@".jpg"), false),
				new ArgumentDefinition("count", 'c', CLIArgumentType.Number, new NumberArgument(-1), false),
			}, DefaultCommandsActions.Metadata) },
			{ "import", new Command("import", "Imports specified files into the album.",
				new List<ArgumentDefinition> {
				new ArgumentDefinition("files", 'f', CLIArgumentType.Files, null, true),
				new ArgumentDefinition("extensions", 'x', CLIArgumentType.String, new StringArgument(@".jpg"), false),
				new ArgumentDefinition("template", 't', CLIArgumentType.String, new StringArgument(@"{YYYY}/{MM}/{YY}{MM}{DD}-{hh}{mm}{ss}"), false),
				new ArgumentDefinition("time-shift", 's', CLIArgumentType.String, new StringArgument(@""), false),
				new ArgumentDefinition("after-date", 'a', CLIArgumentType.String, new StringArgument(@""), false),
				new ArgumentDefinition("before-date", 'b', CLIArgumentType.String, new StringArgument(@""), false),
				new ArgumentDefinition("hash-duplicates", 'h', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgumentDefinition("yes", 'y', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgumentDefinition("no", 'n', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Import) },
			{ "export", new Command("export", "Exports specified files from the album to the specified directory.",
				new List<ArgumentDefinition> {
				new ArgumentDefinition("export-to", 'e', CLIArgumentType.String, null, true),
				new ArgumentDefinition("files", 'f', CLIArgumentType.Files, new FilesArgument(), true),
				new ArgumentDefinition("extensions", 'x', CLIArgumentType.String, new StringArgument(@".jpg"), false),
				new ArgumentDefinition("template", 't', CLIArgumentType.String, new StringArgument(@"{file:name}"), false),
				new ArgumentDefinition("after-date", 'a', CLIArgumentType.String, new StringArgument(@""), false),
				new ArgumentDefinition("before-date", 'b', CLIArgumentType.String, new StringArgument(@""), false),
				new ArgumentDefinition("yes", 'y', CLIArgumentType.Flag, new FlagArgument(false), false),
				new ArgumentDefinition("no", 'n', CLIArgumentType.Flag, new FlagArgument(false), false),
			}, DefaultCommandsActions.Export) },
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

	public class ConsoleLogger : ILogger {
		public bool Verbose { get; }

		public ConsoleLogger(bool verbose) {
			Verbose = verbose;
		}

		public string GetSuitablePath(string path, IFileSystemProvider fileSystem) {
			if (Verbose)
				return fileSystem.GetFullPath(path);
			return DefaultCommandsActions.GetSuitablePath(path, fileSystem);
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
		public static bool RequiresConfirmation(CommandArguments args) {
			if (args.HasArgument<FlagArgument>("yes") && args.GetArgument<FlagArgument>("yes").IsSet) {
				if (args.HasArgument<FlagArgument>("no") && args.GetArgument<FlagArgument>("no").IsSet)
					return true;
				return false;
			}
			if (args.HasArgument<FlagArgument>("no") && args.GetArgument<FlagArgument>("no").IsSet) {
				return false;
			}
			return true;
		}

		public static bool Confirm(CommandArguments args, string msg) {
			if (args.HasArgument<FlagArgument>("yes") && args.GetArgument<FlagArgument>("yes").IsSet) {
				if (args.HasArgument<FlagArgument>("no") && args.GetArgument<FlagArgument>("no").IsSet)
					return ConfirmPrompt(msg, true);
				return true;
			}
			if (args.HasArgument<FlagArgument>("no") && args.GetArgument<FlagArgument>("no").IsSet) {
				return false;
			}
			return ConfirmPrompt(msg);
		}

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

		public static HashSet<string> GetExtensions(CommandArguments args) {
			return new HashSet<string>(args.GetArgument<StringArgument>("extensions").Value.Split(',').Select(x => x.StartsWith(".") ? x : "." + x));
		}

		public static string GetSuitablePath(string path, IFileSystemProvider fileSystem) {
			var fullPath = fileSystem.GetFullPath(path);
			var relPath = fileSystem.GetRelativePath(".", fullPath);
			return relPath.Length < fullPath.Length ? relPath : fullPath;
		}

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

				var m = new Regex(@"'([^']+)'|""([^\\""]+)""|\S+").Matches(cmd);
				var parsedArgs = m.Select(x => x.Groups[1].Success ? x.Groups[1].Value : x.Groups[2].Success ? x.Groups[2].Value : x.Value).ToArray();

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
			var files = ImportFilePathProvider.Process(args.GetArgument<FilesArgument>("files").Files, GetExtensions(args), errHandler);
			if (errHandler.IsError)
				return new CommandResult(false, errHandler.GetUnprocessed().ToList());

			var fs = new NormalFileSystemProvider(".");

			var verbose = args.GetArgument<FlagArgument>("verbose").IsSet;
			var maxCount = args.GetArgument<NumberArgument>("count").Value;
			var count = 0;

			if (verbose) {
				foreach (var file in files.GetFilePaths(fs, errHandler)) {
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
					var infoProvider = new NormalFileInfoProvider();
					var info = infoProvider.GetInfo(file, fs);
					Console.WriteLine($"\tDate/Time = {info.SuitableDateTime}");
					Console.WriteLine("\tExif Date/Time = " + (info.EXIFDateTime is not null ? info.EXIFDateTime : "Unavailable"));
					Console.WriteLine($"\tDevice = {info.DeviceName}");
					Console.WriteLine();
				}
			} else {
				foreach (var file in files.GetFilePaths(fs, errHandler)) {
					if (count++ == maxCount)
						break;
					Console.WriteLine($"{file}:");
					var infoProvider = new NormalFileInfoProvider();
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

		public static CommandResult? GetImportFilePathProvider(CommandArguments args, out IImportFilePathProvider importFilePathProvider) {
			IErrorHandler errHandler = new ErrorListHandler();
			importFilePathProvider = ImportFilePathProvider.Process(args.GetArgument<FilesArgument>("files").Files, GetExtensions(args), errHandler);
			if (errHandler.IsError) {
				return new CommandResult(false, errHandler.GetUnprocessed().ToList());
			}
			return null;
		}

		public static CommandResult? GetImportItems(CommandArguments args, IImportFilePathProvider importFilePathProvider,
			List<IFileFilter> fileFilters, IFileSystemProvider fileSystem, out IEnumerable<ImportItem> importItems) {
			var verbose = args.GetArgument<FlagArgument>("verbose").IsSet;

			IFileNameProvider fileNameProvider;
			try {
				fileNameProvider = TemplateFileNameProvider.MultipleTemplates(args.GetArgument<StringArgument>("template").Value.Split(','));
			} catch (InvalidFileNameTemplateException e) {
				importItems = new List<ImportItem>();
				return new CommandResult(false, new List<string> { "Invalid file name template!", e.Message });
			}

			IImportListProvider importListProvider = new ImportListProvider(new NormalFileInfoProvider(), fileNameProvider);
			if (fileFilters.Count > 0) {
				importListProvider = new FilteredImportListProvider(fileFilters, new NormalFileInfoProvider(), fileNameProvider);
			}

			IErrorHandler errHandler = new ErrorListHandler();

			if (RequiresConfirmation(args) || !Confirm(args, "You should never see this message.")) {
				var importList = importListProvider.GetImportList(fileSystem, importFilePathProvider, errHandler);
				if (verbose) {
					Console.WriteLine("Summary:");
					foreach (var item in importList.AllItems) {
						if (item.Cancelled)
							Console.WriteLine($"{GetSuitablePath(item.SourcePath, fileSystem)} -- Cancelled");
						else
							Console.WriteLine($"{GetSuitablePath(item.SourcePath, fileSystem)} -> {GetSuitablePath(item.DestinationPath, fileSystem)}");
					}
					Console.WriteLine();
				}
				Console.WriteLine($"Targeted {importList.AllItems.Count} files. Will import {importList.ImportItems.Count} files," +
					$" {importList.CancelledItems.Count} files have been cancelled.\n");
				if (Confirm(args, "Do you wish to proceed?")) {
					importItems = importList.ImportItems;
				} else {
					importItems = new List<ImportItem>();
					return new CommandResult(true, "Cancelled by user");
				}
			} else {
				importItems = importListProvider.GetImportItems(fileSystem, importFilePathProvider, errHandler);
			}

			return null;
		}

		public static CommandResult? GetDuplicateResolver(CommandArguments args, out IDuplicateResolver duplicateResolver) {
			duplicateResolver = new SuffixDuplicateResolver();
			if (args.HasArgument<FlagArgument>("hash-duplicates") && args.GetArgument<FlagArgument>("hash-duplicates").IsSet)
				duplicateResolver = new HashDuplicateResolver(duplicateResolver);
			return null;
		}

		public static CommandResult Import(CommandArguments args) {
			var verbose = args.GetArgument<FlagArgument>("verbose").IsSet;

			var res = GetNormalFilters(args, out List<IFileFilter> fileFilters);
			if (res is not null)
				return res;

			var fileSystem = new NormalFileSystemProvider(args.GetArgument<StringArgument>("album-dir").Value);

			res = GetImportFilePathProvider(args, out IImportFilePathProvider importFilePathProvider);
			if (res is not null)
				return res;

			res = GetImportItems(args, importFilePathProvider, fileFilters, fileSystem, out IEnumerable<ImportItem> items);
			if (res is not null)
				return res;

			res = GetDuplicateResolver(args, out IDuplicateResolver duplicateResolver);
			if (res is not null)
				return res;

			IErrorHandler errHandler = new ErrorLogHandler();
			IFileImporter importer = new FileImporter(fileSystem, duplicateResolver);
			var imported = importer.ImportItems(items, errHandler, new ConsoleLogger(verbose)).ToList();

			Console.WriteLine($"{imported.Count} files successfully imported.");
			Console.WriteLine();

			return new CommandResult(true, errHandler.GetAll().ToList());
		}

		public static CommandResult Export(CommandArguments args) {
			var verbose = args.GetArgument<FlagArgument>("verbose").IsSet;

			var res = GetNormalFilters(args, out List<IFileFilter> fileFilters);
			if (res is not null)
				return res;

			var fileSystem = new NormalFileSystemProvider(args.GetArgument<StringArgument>("export-to").Value);

			IImportFilePathProvider importFilePathProvider;
			if (args.GetArgument<FilesArgument>("files").Files.Count != 0) {
				res = GetImportFilePathProvider(args, out importFilePathProvider);
				if (res is not null)
					return res;
			} else {
				importFilePathProvider = new ImportFilePathProvider(new List<IFilePathProvider> {
					new DirectoryFilePathProvider(fileSystem.GetFullPath(args.GetArgument<StringArgument>("album-dir").Value), true)
				}, GetExtensions(args));
			}

			res = GetImportItems(args, importFilePathProvider, fileFilters, fileSystem, out IEnumerable<ImportItem> items);
			if (res is not null)
				return res;

			IDuplicateResolver duplicateResolver = new SkipDuplicateResolver();

			IErrorHandler errHandler = new ErrorLogHandler();
			IFileImporter importer = new FileImporter(fileSystem, duplicateResolver);
			var imported = importer.ImportItems(items, errHandler, new ConsoleLogger(verbose)).ToList();

			Console.WriteLine($"{imported.Count} files successfully imported.");
			Console.WriteLine();

			return new CommandResult(true, errHandler.GetAll().ToList());
		}
	}
}
