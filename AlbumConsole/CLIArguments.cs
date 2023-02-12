using System.Text.RegularExpressions;

namespace AlbumConsole {
	/// <summary>
	/// Class defining the types and names of arguments that a command will take
	/// </summary>
	public class CLIDefinition {
		protected List<ArgumentDefinition> Arguments { get; }
		protected List<CLIDefinition> Include { get; }

		protected Dictionary<string, ArgumentDefinition> NameMap { get; }
		protected Dictionary<char, ArgumentDefinition> ShortNameMap { get; }
		protected List<ArgumentDefinition> ImplicitArguments { get; }

		public string Name { get; }
		public string Description { get; }

		public CLIDefinition(string name, List<ArgumentDefinition> args) : this(name, args, new List<CLIDefinition>()) { }

		public CLIDefinition(string name, List<ArgumentDefinition> args, List<CLIDefinition> include) {
			Name = name;
			Arguments = args;
			Include = include;
			NameMap = new Dictionary<string, ArgumentDefinition>();
			ShortNameMap = new Dictionary<char, ArgumentDefinition>();
			ImplicitArguments = Arguments.Where(a => a.IsImplicit).ToList();

			var desc = new List<string>();
			foreach (var a in Arguments) {
				if (!NameMap.TryAdd(a.Name, a))
					throw new CLIDefinitionException($"Duplicate argument name: {a.Name}");
				if (a.ShortName != ' ')
					if (!ShortNameMap.TryAdd(a.ShortName, a))
						throw new CLIDefinitionException($"Duplicate argument short name: {a.ShortName}");
				desc.Add(a.Description);
			}
			Description = string.Join(' ', desc);

			foreach (var i in Include) {
				foreach (var a in i.Arguments) {
					if (!NameMap.TryAdd(a.Name, a))
						throw new CLIDefinitionException($"Duplicate argument name: {a.Name}");
					if (a.ShortName != ' ')
						if (!ShortNameMap.TryAdd(a.ShortName, a))
							throw new CLIDefinitionException($"Duplicate argument short name: {a.ShortName}");
				}
			}
		}

		/// <summary>
		/// Extracts argument values from an argument list.
		/// </summary>
		/// <param name="args">Command line arguments to parse</param>
		/// <returns>The values of the specified arguments or the default values</returns>
		/// <exception cref="CLIArgumentException"></exception>
		/// <exception cref="CLIUnknownArgumentException"></exception>
		public Dictionary<string, IArgument> GetArguments(string[] args) {
			var named = new Dictionary<string, IArgument>();

			var en = new ArgumentIterator(args);
			var i = 0; // implicit argument index
			while (en.Valid) {
				try {
					if (en.Current.StartsWith("--")) {
						if (NameMap.TryGetValue(en.Current[2..], out ArgumentDefinition? a)) {
							if (named.ContainsKey(a.Name))
								throw new CLIArgumentException($"Duplicate argument: {a.Name}", named);
							en.MoveNext();
							named[a.Name] = a.ExtractValue(en);
						} else
							throw new CLIUnknownArgumentException($"Unknown argument: {en.Current}", named);
					} else if (en.Current.StartsWith("-")) {
						var curr = en.Current[1..];
						en.MoveNext();
						foreach (var ch in curr) {
							if (ShortNameMap.TryGetValue(ch, out ArgumentDefinition? a)) {
								if (named.ContainsKey(a.Name))
									throw new CLIArgumentException($"Duplicate argument: {a.Name}", named);
								named[a.Name] = a.ExtractValue(en);
							} else
								throw new CLIUnknownArgumentException($"Unknown argument: -{ch}", named);
						}
					} else if (i < ImplicitArguments.Count) {
						while (named.ContainsKey(ImplicitArguments[i].Name)) {
							i++;
							if (i >= ImplicitArguments.Count)
								throw new CLIArgumentException($"Unexpected implicit argument: {en.Current}", named);
						}
						var a = ImplicitArguments[i++];
						named[a.Name] = a.ExtractValue(en);
					} else {
						throw new CLIArgumentException($"Unexpected implicit argument: {en.Current}", named);
					}
				} catch (CLIArgumentExtractValueException e) {
					throw new CLIArgumentException(e.Message, named);
				}
			}

			foreach (var x in NameMap) {
				if (!named.ContainsKey(x.Key)) {
					named[x.Key] = x.Value.DefaultValue.ProvideDefault() ??
						throw new CLIArgumentException($"Missing required parameter: {x.Key}", named);
				}
			}

			return named;
		}

		protected static Regex ArgsRegex { get; } = new Regex(@"'([^']+)'|""([^\\""]+)""|\S+");

		public static string[] ParseArguments(string args) {
			var m = ArgsRegex.Matches(args);
			var parsedArgs = m.Select(x => x.Groups[1].Success ? x.Groups[1].Value : x.Groups[2].Success ? x.Groups[2].Value : x.Value).ToArray();
			return parsedArgs;
		}

		/// <summary>
		/// Returns the full description of the command. Used by help command.
		/// </summary>
		/// <param name="recurse"></param>
		/// <returns></returns>
		public string GetFullDescription(int recurse) {
			if (Include.Count == 0) {
				return Description;
			}
			if (recurse == 0) {
				return string.Join(' ', from i in Include select $"{{{i.Name}}}") + " " + Description;
			}
			return string.Join(' ', from i in Include select i.GetFullDescription(recurse - 1)) + " " + Description;
		}
	}

	/// <summary>
	/// Custom exception for invalid CLI definitions.
	/// </summary>
	public class CLIDefinitionException : Exception {
		public CLIDefinitionException(string msg) : base(msg) { }
	}

	/// <summary>
	/// Custom exception for invalid CLI usage.
	/// </summary>
	public class CLIArgumentException : Exception {
		public Dictionary<string, IArgument> ProcessedArgs { get; set; }

		public CLIArgumentException(string msg, Dictionary<string, IArgument> processedArgs) : base(msg) {
			ProcessedArgs = processedArgs;
		}
	}

	/// <summary>
	/// Custom exception for incorrect argument type used.
	/// </summary>
	public class CLIArgumentExtractValueException : Exception {
		public CLIArgumentExtractValueException(string msg) : base(msg) { }
	}

	/// <summary>
	/// Custom exception for when an unknown argument is specified.
	/// </summary>
	public class CLIUnknownArgumentException : CLIArgumentException {
		public CLIUnknownArgumentException(string msg, Dictionary<string, IArgument> processedArgs) : base(msg, processedArgs) { }
	}

	public interface IDefaultValueProvider {
		IArgument? ProvideDefault();
	}

	public class NoDefaultValue : IDefaultValueProvider {
		public IArgument? ProvideDefault() => null;
	}

	public class FixedDefaultValue : IDefaultValueProvider {
		public IArgument Value { get; }

		public FixedDefaultValue(IArgument value) {
			Value = value;
		}

		public IArgument? ProvideDefault() => Value.MarkAsDefault();
	}

	/// <summary>
	/// Different types of arguments.
	/// </summary>
	public enum CLIArgumentType {
		Flag,
		Number,
		String,
		Files
	}

	/// <summary>
	/// The definition of an command-line argument.
	/// </summary>
	public class ArgumentDefinition {
		public string Name { get; }
		public char ShortName { get; }
		public CLIArgumentType Type { get; }
		public IDefaultValueProvider DefaultValue { get; }
		public bool IsImplicit { get; }

		public string Description { get; }

		public ArgumentDefinition(string name, char shortName, CLIArgumentType type, IDefaultValueProvider defaultValue, bool isImplicit) {
			Name = name;
			ShortName = shortName;
			Type = type;
			DefaultValue = defaultValue;
			IsImplicit = isImplicit;

			var valueDesc = type.GetName();
			var valueSep = string.IsNullOrEmpty(valueDesc) ? "" : " ";
			var nameDesc = ShortName == ' ' ? $"--{Name}" : $"-{ShortName}/--{Name}";
			if (IsImplicit)
				nameDesc = $"[{nameDesc}]";
			Description = $"{nameDesc}{valueSep}{valueDesc}";
			if (DefaultValue is not NoDefaultValue)
				Description = $"[{Description}]";
		}

		/// <summary>
		/// Extracts the current argument.
		/// </summary>
		/// <param name="en"></param>
		/// <returns></returns>
		/// <exception cref="CLIArgumentExtractValueException"></exception>
		/// <exception cref="NotImplementedException"></exception>
		internal IArgument ExtractValue(ArgumentIterator en) {
			return ExtractValue(Type, Name, en);
		}

		/// <summary>
		/// Extracts an argument of a given type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="en"></param>
		/// <returns></returns>
		/// <exception cref="CLIArgumentExtractValueException"></exception>
		/// <exception cref="NotImplementedException"></exception>
		internal static IArgument ExtractValue(CLIArgumentType type, string name, ArgumentIterator en) {
			switch (type) {
			case CLIArgumentType.Flag:
				return new FlagArgument(true);
			case CLIArgumentType.Number:
				if (!en.Valid)
					throw new CLIArgumentExtractValueException($"Argument {name} requires a number but nothing was given");
				int x;
				if (int.TryParse(en.Current, out x)) {
					en.MoveNext();
					return new NumberArgument(x);
				}
				throw new CLIArgumentExtractValueException($"Argument {name} requires a number but '{en.Current}' was given");
			case CLIArgumentType.String:
				if (!en.Valid)
					throw new CLIArgumentExtractValueException($"Argument {name} requires a string but nothing was given");
				return new StringArgument(en.Extract());
			case CLIArgumentType.Files:
				var files = new List<string>();
				if (!en.Valid)
					return new FilesArgument(files);
				if (en.Current.StartsWith("-")) {
					return new FilesArgument(files);
				}
				files.Add(en.Current);
				while (en.MoveNext()) {
					if (en.Current.StartsWith("-")) {
						break;
					}
					files.Add(en.Current);
				}
				return new FilesArgument(files);
			default:
				throw new NotImplementedException($"Unimplemented argument type: {type}");
			}
		}
	}

	/// <summary>
	/// Interface for different types of arguments.
	/// </summary>
	public interface IArgument {
		public bool IsDefault { get; }
		public IArgument MarkAsDefault();
	}

	public class AbstractArgument : IArgument {
		public bool IsDefault { get; protected set; }

		public IArgument MarkAsDefault() {
			IsDefault = true;
			return this;
		}
	}

	/// <summary>
	/// Boolean argument.
	/// </summary>
	public class FlagArgument : AbstractArgument, IArgument {
		public bool IsSet { get; }

		public FlagArgument(bool isSet) {
			IsSet = isSet;
		}

		public override string ToString() {
			return IsSet.ToString();
		}

		public override bool Equals(object? obj) {
			if (obj == null)
				return false;
			if (obj is not FlagArgument)
				return false;
			return IsSet.Equals(((FlagArgument)obj).IsSet);
		}

		public override int GetHashCode() {
			return IsSet.GetHashCode();
		}

		public static IDefaultValueProvider Default(bool isSet) {
			return new FixedDefaultValue(new FlagArgument(isSet));
		}
	}

	/// <summary>
	/// Integer argument.
	/// </summary>
	public class NumberArgument : AbstractArgument, IArgument {
		public int Value { get; }

		public NumberArgument(int value) {
			Value = value;
		}

		public override string ToString() {
			return Value.ToString();
		}

		public override bool Equals(object? obj) {
			if (obj == null)
				return false;
			if (obj is not NumberArgument)
				return false;
			return Value.Equals(((NumberArgument)obj).Value);
		}

		public override int GetHashCode() {
			return Value.GetHashCode();
		}

		public static IDefaultValueProvider Default(int value) {
			return new FixedDefaultValue(new NumberArgument(value));
		}
	}

	/// <summary>
	/// Single string argument.
	/// </summary>
	public class StringArgument : AbstractArgument, IArgument {
		public string Value { get; }

		public StringArgument(string value) {
			Value = value;
		}

		public override string ToString() {
			return Value.ToString();
		}

		public override bool Equals(object? obj) {
			if (obj == null) return false;
			if (obj is not StringArgument) return false;
			return Value.Equals(((StringArgument)obj).Value);
		}

		public override int GetHashCode() {
			return Value.GetHashCode();
		}

		public static IDefaultValueProvider Default(string value) {
			return new FixedDefaultValue(new StringArgument(value));
		}
	}

	/// <summary>
	/// An argument for a lis of files - basically an array of strings.
	/// </summary>
	public class FilesArgument : AbstractArgument, IArgument {
		public List<string> Files { get; }

		public FilesArgument() : this(new List<string>()) { }

		public FilesArgument(List<string> files) {
			Files = files;
		}

		public override string ToString() {
			return string.Join(", ", Files);
		}

		public override bool Equals(object? obj) {
			if (obj == null) return false;
			if (obj is not FilesArgument) return false;
			if (Files.Count != ((FilesArgument)obj).Files.Count) return false;
			return Files.Zip(((FilesArgument)obj).Files).All((x) => x.First == x.Second);
		}

		public override int GetHashCode() {
			return Files.GetHashCode();
		}

		public static IDefaultValueProvider Default(List<string> files) {
			return new FixedDefaultValue(new FilesArgument(files));
		}

		public static IDefaultValueProvider Default() {
			return new FixedDefaultValue(new FilesArgument());
		}
	}

	/// <summary>
	/// Class which iterates over command-line arguments.
	/// </summary>
	internal class ArgumentIterator {
		protected string[] Args { get; }

		protected int Index { get; set; } = 0;

		public ArgumentIterator(string[] args) {
			Args = args;
		}

		public bool Valid => 0 <= Index && Index < Args.Length;
		public string Current => Args[Index];

		public bool MoveNext() {
			if (Index < Args.Length)
				Index++;
			return Index < Args.Length;
		}

		public bool MoveBack() {
			if (Index >= 0)
				Index--;
			return Index >= 0;
		}

		public string Extract() {
			return Args[Index++];
		}
	}

	internal static class Extensions {
		public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator) {
			while (enumerator.MoveNext())
				yield return enumerator.Current;
		}

		public static string GetName(this CLIArgumentType type) {
			return type switch {
				CLIArgumentType.Flag => "",
				CLIArgumentType.Number => "{number}",
				CLIArgumentType.String => "{string}",
				CLIArgumentType.Files => "{file list}",
				_ => throw new NotImplementedException($"Unimplemented argument type: {type}"),
			};
		}
	}
}
