using AlbumConsole;

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
Console.WriteLine($"Unnamed args:\n    {string.Join("    ", command.UnnamedArguments)}");
Console.WriteLine($"Named args:\n    {string.Join("    ", from x in command.NamedArguments select $"{x.Key} = {x.Value}")}");

return 0;
