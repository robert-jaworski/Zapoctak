﻿using AlbumConsole;
using AlbumLibrary;

try {
	// First get info about the current executable and album directory and profile
	CommandArguments command = CommandArguments.ParseArguments(Environment.GetCommandLineArgs(), true);

	var fileSystem = new NormalFileSystemProvider(command.AlbumDirectory);
	var errHandler = new ErrorLogHandler();
	var configReader = new ConfigFileReader(new List<string> { Path.Combine(command.ExecutableDirectory, "config"),
		Path.Combine(command.AlbumDirectory, "config") });

	// Now use to truly parse arguments
	Config.CurrentConfig = configReader.ReadConfig(fileSystem, errHandler);
	Config.CurrentProfile = command.Profile;

	Console.WriteLine("Using profile: {0}\n", Config.CurrentProfile);

	command = CommandArguments.ParseArguments(Environment.GetCommandLineArgs());

	var res = Command.RunCommand(command);
	if (!res.Success)
		Console.WriteLine("There has been an error:");
	res.Messages.ForEach(x => Console.WriteLine(x));
	return res.Success ? 0 : 1;
} catch (CLIArgumentException e) {
	Console.WriteLine("Invalid arguments:");
	Console.WriteLine(e.Message);
	//Console.Error.WriteLine(e);
	return 2;
} catch (InternalException e) {
	Console.WriteLine("An unexpected internal error:");
	Console.WriteLine(e.Message);
	Console.Error.WriteLine(e);
	return 3;
} catch (NotImplementedException e) {
	Console.WriteLine("Not implemented:");
	Console.WriteLine(e.Message);
	Console.Error.WriteLine(e);
	return 4;
} catch (Exception e) {
	Console.WriteLine("An unexpected error:");
	Console.WriteLine(e.Message);
	Console.Error.WriteLine(e);
	return 15;
}
