using AlbumLibrary;
using System.Text.RegularExpressions;

namespace AlbumConsole {
	public interface IConfig {
		IArgument? ProvideDefault(string command, string profile, string argument, CLIArgumentType argType);

		IEnumerable<string> GetProfiles();
	}

	public interface IConfigFileReader {
		IConfig ReadConfig(IFileSystemProvider fileSystem, IErrorHandler errorHandler);
	}

	public class Config : IConfig {
		private Dictionary<string, ConfigProfile> Profiles { get; }
		private Config? FallbackConfig { get; }

		public static IConfig? CurrentConfig { get; set; }
		public static string CurrentProfile { get; set; } = "default";

		public Config(Config? fallbackConfig = null) {
			Profiles = new();
			FallbackConfig = fallbackConfig;
		}

		internal bool AddProfile(string name, ConfigProfile profile) {
			if (Profiles.ContainsKey(name))
				return false;
			Profiles[name] = profile;
			return true;
		}

		public IArgument? ProvideDefault(string command, string profile, string argument, CLIArgumentType argType) {
			try {
				IArgument? output = null;
				if (Profiles.ContainsKey(profile))
					output ??= Profiles[profile].ProvideDefault(command, argument, argType);
				if (Profiles.ContainsKey("default"))
					output ??= Profiles["default"].ProvideDefault(command, argument, argType);
				if (FallbackConfig is not null)
					output ??= FallbackConfig.ProvideDefault(command, profile, argument, argType);
				return output;
			} catch (CLIArgumentExtractValueException e) {
				throw new CLIArgumentExtractValueException("Config error: " + e.Message);
			}
		}

		public void Print() {
			Console.WriteLine("Start of config");
			foreach (var (k, v) in Profiles) {
				Console.WriteLine("  Profile {0}:", k);
				v.Print();
			}
			FallbackConfig?.Print();
			Console.WriteLine("End of config");
		}

		public void Print(string profile) {
			if (Profiles.ContainsKey(profile))
				Profiles[profile].Print();
			FallbackConfig?.Print(profile);
		}

		public IEnumerable<string> GetProfiles() {
			return FallbackConfig is null ? Profiles.Keys : FallbackConfig.GetProfiles().Union(Profiles.Keys);
		}
	}

	internal class ConfigProfile {
		private Dictionary<string, string[]> Defaults { get; }
		private Dictionary<string, CommandDefaults> CommandDefaults { get; }

		public ConfigProfile() {
			Defaults = new();
			CommandDefaults = new();
		}

		public bool AddDefault(string argument, string[] args) {
			if (Defaults.ContainsKey(argument))
				return false;
			Defaults[argument] = args;
			return true;
		}

		public bool AddDefault(string command, string argument, string[] args) {
			if (!CommandDefaults.ContainsKey(command))
				CommandDefaults[command] = new CommandDefaults();
			return CommandDefaults[command].AddDefault(argument, args);
		}

		public IArgument? ProvideDefault(string command, string argument, CLIArgumentType argType) {
			IArgument? output = null;
			if (CommandDefaults.ContainsKey(command))
				output ??= CommandDefaults[command].ProvideDefault(argument, argType);
			if (Defaults.ContainsKey(argument))
				output ??= ArgumentDefinition.ExtractValue(argType, argument, new ArgumentIterator(Defaults[argument]), true);
			return output;
		}

		public void Print() {
			foreach (var (k, v) in Defaults) {
				Console.WriteLine("    {0} -> {1}", k, string.Join(' ', v));
			}
			foreach (var (k, v) in CommandDefaults) {
				v.Print(k);	
			}
		}
	}

	internal class CommandDefaults {
		private Dictionary<string, string[]> Defaults { get; }

		public CommandDefaults() {
			Defaults = new();
		}

		public bool AddDefault(string argument, string[] args) {
			if (Defaults.ContainsKey(argument))
				return false;
			Defaults[argument] = args;
			return true;
		}

		public IArgument? ProvideDefault(string argument, CLIArgumentType argType) {
			if (Defaults.ContainsKey(argument))
				return ArgumentDefinition.ExtractValue(argType, argument, new ArgumentIterator(Defaults[argument]), true);
			return null;
		}

		public void Print(string cmd) {
			foreach (var (k, v) in Defaults) {
				Console.WriteLine("    ({2}) {0} -> {1}", k, string.Join(' ', v), cmd);
			}
		}
	}

	public class ConfigFileReader : IConfigFileReader {
		public List<string> Paths { get; }

		public ConfigFileReader(IEnumerable<string> path) {
			Paths = path.ToList();
		}

		protected static Regex SectionRegex { get; } = new Regex(@"^\[(.*)\]\s*(?:[;#].*)?$");
		protected static Regex KeyValueRegex { get; } = new Regex(@"^(.*?)\s*=\s*(.*)$");
		protected static Regex CommandRegex { get; } = new Regex(@"^(.*?)--?(.*)$");

		public IConfig ReadConfig(IFileSystemProvider fileSystem, IErrorHandler errorHandler) {
			Config? config = null;
			foreach (var p in Paths) {
				var fullPath = fileSystem.GetFullPath(p);
				if (!fileSystem.FileExists(fullPath)) {
					errorHandler.Error($"No config file found: {fullPath}");
					continue;
				}
				config = new Config(config);
				using var file = fileSystem.ReadText(fullPath);

				var section = "default";
				ConfigProfile? profile = null;
				var lineCount = 0;
				var sectionStart = 0;

				while (!file.EndOfStream) {
					lineCount++;
					var line = file.ReadLine()?.Trim();
					if (string.IsNullOrEmpty(line) || line.StartsWith(';') || line.StartsWith("#"))
						continue;

					var m = SectionRegex.Match(line);
					if (m.Success) {
						var s = m.Groups[1].Value;
						if (section != s) {
							if (profile is not null) {
								if (!config.AddProfile(section, profile))
									errorHandler.Error($"Duplicate config section (line {sectionStart}): {section}");
							}
							profile = new ConfigProfile();
							section = s;
							sectionStart = lineCount;
						} else if (profile is not null) {
							errorHandler.Error($"Duplicate config section (line {lineCount}): {section}");
						}
						continue;
					}

					m = KeyValueRegex.Match(line);
					if (m.Success) {
						profile ??= new ConfigProfile();

						var key = m.Groups[1].Value;
						var value = m.Groups[2].Value;

						m = CommandRegex.Match(key);
						if (m.Success) {
							var arg = m.Groups[2].Value;
							if (!string.IsNullOrEmpty(m.Groups[1].Value)) {
								var command = m.Groups[1].Value;
								if (!profile.AddDefault(command, arg, CLIDefinition.ParseArguments(value)))
									errorHandler.Error($"Duplicate default definition (line {lineCount}): {key}");
							} else {
								if (!profile.AddDefault(arg, CLIDefinition.ParseArguments(value)))
									errorHandler.Error($"Duplicate default definition (line {lineCount}): {key}");
							}
						}
						continue;
					}

					errorHandler.Error($"Syntax error (line {lineCount}): {line}");
				}
				if (profile is not null) {
					if (!config.AddProfile(section, profile))
						errorHandler.Error($"Duplicate config section (line {sectionStart}): {section}");
				}
			}

			return config ?? new Config();
		}
	}

	public class ConfigDefaultValueProvider : IDefaultValueProvider {
		public string Command { get; }
		public string Argument { get; }
		public CLIArgumentType Type { get; }
		public IArgument? Default { get; }

		public ConfigDefaultValueProvider(string command, string argument, CLIArgumentType type, IArgument? @default) {
			Command = command;
			Argument = argument;
			Type = type;
			Default = @default;
		}

		public IArgument? ProvideDefault() {
			return Config.CurrentConfig?.ProvideDefault(Command, Config.CurrentProfile, Argument, Type) ?? Default?.MarkAsDefault();
		}
	}
}
