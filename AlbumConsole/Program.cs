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
