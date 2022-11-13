namespace AlbumLibrary {
	namespace CLI {
		public class CLIDefinition {
			public List<ArgumentDefinition> Arguments { get; }
			public List<CLIDefinition> Include { get; }

			public Dictionary<string, ArgumentDefinition> NameMap { get; }
			public Dictionary<char, ArgumentDefinition> ShortNameMap { get; }

			public CLIDefinition(List<ArgumentDefinition> args) : this(args, new List<CLIDefinition>()) {}

			public CLIDefinition(List<ArgumentDefinition> args, List<CLIDefinition> include) {
				Arguments = args;
				Include = include;
				NameMap = new Dictionary<string, ArgumentDefinition>();
				ShortNameMap = new Dictionary<char, ArgumentDefinition>();
				foreach (var a in Arguments) {
					if (!NameMap.TryAdd(a.Name, a))
						throw new ArgumentException($"Duplicate argument name: {a.Name}");
					if (!ShortNameMap.TryAdd(a.ShortName, a))
						throw new ArgumentException($"Duplicate argument short name: {a.ShortName}");
				}
				foreach (var i in Include) {
					foreach (var a in i.Arguments) {
						if (!NameMap.TryAdd(a.Name, a))
							throw new ArgumentException($"Duplicate argument name: {a.Name}");
						if (!ShortNameMap.TryAdd(a.ShortName, a))
							throw new ArgumentException($"Duplicate argument short name: {a.ShortName}");
					}
				}
			}

			public Arguments GetArguments(IEnumerable<string> args) {
				var named = new Dictionary<string, IArgument>();
				var unnamed = new List<string>();

				foreach (var arg in Arguments) {
					named[arg.Name] = arg.DefaultValue;
				}

				var en = args.GetEnumerator();
				while (en.MoveNext()) {
					if (en.Current.StartsWith("--")) {
						if (NameMap.TryGetValue(en.Current[2..], out ArgumentDefinition a)) {
							named[a.Name] = a.ExtractValue(en);
						} else
							throw new NotSupportedException($"Unknown argument: {en.Current}");
					} else if (en.Current.StartsWith("-")) {
						foreach (var ch in en.Current[1..]) {
							if (ShortNameMap.TryGetValue(ch, out ArgumentDefinition a)) {
								named[a.Name] = a.ExtractValue(en);
							} else
								throw new NotSupportedException($"Unknown argument: -{ch}");
						}
					} else {
						unnamed.Add(en.Current);
					}
				}

				return new Arguments(named, unnamed);
			}
		}

		public class Arguments {
			public Dictionary<string, IArgument> NamedArgs { get; }
			public List<string> UnnamedArgs { get; }

			public Arguments() : this(new Dictionary<string, IArgument>(), new List<string>()) {}

			public Arguments(Dictionary<string, IArgument> namedArgs, List<string> unnamedArgs) {
				NamedArgs = namedArgs;
				UnnamedArgs = unnamedArgs;
			}
		}

		public enum CLIArgumentType {
			Flag,
			Number,
			String,
			Files
		}

		public class ArgumentDefinition {
			public string Name { get; }
			public char ShortName { get; }
			public CLIArgumentType Type { get; }
			public IArgument DefaultValue { get; }

			public ArgumentDefinition(string name, char shortName, CLIArgumentType type, IArgument defaultValue) {
				Name = name;
				ShortName = shortName;
				Type = type;
				DefaultValue = defaultValue;
			}

			public IArgument ExtractValue(IEnumerator<string> args) {
				switch (Type) {
				case CLIArgumentType.Flag:
					return new FlagArgument(true);
				case CLIArgumentType.Number:
					if (!args.MoveNext())
						throw new ArgumentException($"Argument {Name} requires a number, but nothing was given");
					int x;
					if (int.TryParse(args.Current, out x)) {
						args.MoveNext();
						return new NumberArgument(x);
					}
					throw new ArgumentException($"Argument {Name} requires a number, but '{args.Current}' was given");
				case CLIArgumentType.String:
					if (!args.MoveNext())
						throw new ArgumentException($"Argument {Name} requires a string, but nothing was given");
					return new StringArgument(args.Current);
				case CLIArgumentType.Files:
					return new FilesArgument(args.ToEnumerable().ToList());
				default:
					throw new NotImplementedException($"Unimplemented argument type: {Type}");
				}
			}
		}

		public interface IArgument {}

		public class FlagArgument : IArgument {
			public bool IsSet { get; }

			public FlagArgument(bool isSet) {
				IsSet = isSet;
			}

			public override string ToString() {
				return IsSet.ToString();
			}
		}

		public class NumberArgument : IArgument {
			public int Value { get; }

			public NumberArgument(int value) {
				Value = value;
			}

			public override string ToString() {
				return Value.ToString();
			}
		}

		public class StringArgument : IArgument {
			public string Value { get; }

			public StringArgument(string value) {
				Value = value;
			}

			public override string ToString() {
				return Value.ToString();
			}
		}

		public class FilesArgument : IArgument {
			public List<string> Files { get; }

			public FilesArgument(List<string> files) {
				Files = files;
			}

			public override string ToString() {
				return string.Join(", ", Files);
			}
		}

		public static class EnumeratorExtensions {
			public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator) {
				while (enumerator.MoveNext())
					yield return enumerator.Current;
			}
		}
	}
}
