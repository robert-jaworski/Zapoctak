using AlbumConsole;
using AlbumLibrary.CLI;

try {
	CommandArguments command = CommandArguments.ParseArguments(Environment.GetCommandLineArgs());

	var res = Command.RunCommand(command);
	if (!res.Success)
		Console.WriteLine("There has been an error:");
	res.Messages.ForEach(x => Console.WriteLine(x));
	return res.Success ? 0 : 1;
} catch (CLIArgumentException e) {
	Console.Error.WriteLine("Invalid arguments:");
	Console.Error.WriteLine(e.Message);
	return 2;
} catch (InternalException e) {
	Console.Error.WriteLine("An unexpected internal error:");
	Console.Error.WriteLine(e.Message);
	return 3;
} catch (NotImplementedException e) {
	Console.Error.WriteLine("Not implemented:");
	Console.Error.WriteLine(e.Message);
	return 4;
} catch (Exception e) {
	Console.Error.WriteLine("An unexpected error:");
	Console.Error.WriteLine(e);
	return 15;
}

/*
Console.WriteLine($"Exe dir    = {command.ExecutableDirectory}");
Console.WriteLine($"Album dir  = {command.AlbumDirectory}");
Console.WriteLine($"Command    = {command.Command}");
Console.WriteLine($"Named args:\n    {string.Join("\n    ", from x in command.NamedArguments select $"{x.Key} = {x.Value}")}");

if (command.Command == "debug") {
	var errHandler = new ErrorListHandler();
	var files = ImportFilePathProvider.Process(((FilesArgument)command.NamedArguments["test-files"]).Files,
		new HashSet<string> { ".jpg" }, errHandler);
	if (errHandler.IsError) {
		Console.WriteLine("\nParse errors: ");
		Console.Write(string.Join('\n', errHandler.GetUnprocessed()));
	} else {
		Console.WriteLine("Parsed successfully");
		Console.WriteLine("\nFiles: ");
		var fs = new DirectoryFileSystemProvider(command.AlbumDirectory);
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
		}
		if (errHandler.IsError) {
			Console.WriteLine("\nErrors: ");
			Console.Write(string.Join('\n', errHandler.GetUnprocessed()));
		} else {
			Console.WriteLine("No errors");
		}
	}
}

return 0;
*/