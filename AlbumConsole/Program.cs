using AlbumConsole;
using AlbumLibrary;
using AlbumLibrary.CLI;

CommandArguments command;
try {
	command = CommandArguments.ParseArguments(Environment.GetCommandLineArgs());
} catch (ArgumentException e) {
	Console.Error.WriteLine("Invalid arguments:");
	Console.Error.WriteLine(e.Message);
	return 1;
} catch (NotSupportedException e) {
	Console.Error.WriteLine("Unsupported operation:");
	Console.Error.WriteLine(e.Message);
	return 2;
} catch (NotImplementedException e) {
	Console.Error.WriteLine(e.Message);
	return 3;
} catch (Exception e) {
	Console.Error.WriteLine(e);
	return 7;
}

Console.WriteLine($"Exe dir    = {command.ExecutableDirectory}");
Console.WriteLine($"Album dir  = {command.AlbumDirectory}");
Console.WriteLine($"Command    = {command.Command}");
Console.WriteLine($"Named args:\n    {string.Join("\n    ", from x in command.NamedArguments select $"{x.Key} = {x.Value}")}");

if (command.Command == "debug") {
	var files = ImportFilePathProvider.Process(((FilesArgument)command.NamedArguments["test-files"]).Files, new HashSet<string> { ".jpg" });
	Console.WriteLine("\nFiles: ");
	Console.Write(string.Join('\n', files.GetFilePaths(command.AlbumDirectory)));
	Console.WriteLine("\nErrors: ");
	Console.Write(string.Join('\n', files.GetErrors()));
}

return 0;
