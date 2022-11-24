namespace AlbumLibrary {
	namespace CLI {
		public class CLIDefinition {
			public List<ArgumentDefinition> Arguments { get; }
			public List<CLIDefinition> Include { get; }

			public Dictionary<string, ArgumentDefinition> NameMap { get; }
			public Dictionary<char, ArgumentDefinition> ShortNameMap { get; }
			public List<ArgumentDefinition> ImplicitArguments { get; }

			public CLIDefinition(List<ArgumentDefinition> args) : this(args, new List<CLIDefinition>()) { }

			public CLIDefinition(List<ArgumentDefinition> args, List<CLIDefinition> include) {
				Arguments = args;
				Include = include;
				NameMap = new Dictionary<string, ArgumentDefinition>();
				ShortNameMap = new Dictionary<char, ArgumentDefinition>();
				ImplicitArguments = Arguments.Where(a => a.IsImplicit).ToList();
				foreach (var a in Arguments) {
					if (!NameMap.TryAdd(a.Name, a))
						throw new ArgumentException($"Duplicate argument name: {a.Name}");
					if (a.ShortName != ' ')
						if (!ShortNameMap.TryAdd(a.ShortName, a))
							throw new ArgumentException($"Duplicate argument short name: {a.ShortName}");
				}
				foreach (var i in Include) {
					foreach (var a in i.Arguments) {
						if (!NameMap.TryAdd(a.Name, a))
							throw new ArgumentException($"Duplicate argument name: {a.Name}");
						if (a.ShortName != ' ')
							if (!ShortNameMap.TryAdd(a.ShortName, a))
								throw new ArgumentException($"Duplicate argument short name: {a.ShortName}");
					}
				}
			}

			public Dictionary<string, IArgument> GetArguments(IEnumerable<string> args) {
				var named = new Dictionary<string, IArgument>();

				var en = args.GetEnumerator();
				var i = 0;
				while (en.MoveNext()) {
					if (en.Current.StartsWith("--")) {
						if (NameMap.TryGetValue(en.Current[2..], out ArgumentDefinition a)) {
							if (named.ContainsKey(a.Name))
								throw new ArgumentException($"Duplicate argument: {a.Name}");
							named[a.Name] = a.ExtractValue(en);
						} else
							throw new NotSupportedException($"Unknown argument: {en.Current}");
					} else if (en.Current.StartsWith("-")) {
						foreach (var ch in en.Current[1..]) {
							if (ShortNameMap.TryGetValue(ch, out ArgumentDefinition a)) {
								if (named.ContainsKey(a.Name))
									throw new ArgumentException($"Duplicate argument: {a.Name}");
								named[a.Name] = a.ExtractValue(en);
							} else
								throw new NotSupportedException($"Unknown argument: -{ch}");
						}
					} else if (i < ImplicitArguments.Count) {
						while (named.ContainsKey(ImplicitArguments[i].Name)) {
							i++;
							if (i >= ImplicitArguments.Count)
								throw new ArgumentException($"Unexpected implicit argument: {en.Current}");
						}
						var a = ImplicitArguments[i++];
						named[a.Name] = a.ExtractValue(en, true);
					} else {
						throw new ArgumentException($"Unexpected implicit argument: {en.Current}");
					}
				}

				foreach (var x in NameMap) {
					if (!named.ContainsKey(x.Key)) {
						if (x.Value.DefaultValue is null)
							throw new ArgumentException($"Missing required parameter: {x.Key}");
						named[x.Key] = x.Value.DefaultValue;
					}
				}

				return named;
			}
		}

		public class Arguments {
			public Dictionary<string, IArgument> NamedArgs { get; }
			public List<string> UnnamedArgs { get; }

			public Arguments() : this(new Dictionary<string, IArgument>(), new List<string>()) { }

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
			public IArgument? DefaultValue { get; }
			public bool IsImplicit { get; }

			public ArgumentDefinition(string name, char shortName, CLIArgumentType type, IArgument? defaultValue, bool isImplicit) {
				Name = name;
				ShortName = shortName;
				Type = type;
				DefaultValue = defaultValue;
				IsImplicit = isImplicit;
			}

			public IArgument ExtractValue(IEnumerator<string> en, bool isImplicit = false) {
				switch (Type) {
				case CLIArgumentType.Flag:
					return new FlagArgument(true);
				case CLIArgumentType.Number:
					if (!isImplicit && !en.MoveNext())
						throw new ArgumentException($"Argument {Name} requires a number, but nothing was given");
					int x;
					if (int.TryParse(en.Current, out x)) {
						return new NumberArgument(x);
					}
					throw new ArgumentException($"Argument {Name} requires a number, but '{en.Current}' was given");
				case CLIArgumentType.String:
					if (!isImplicit && !en.MoveNext())
						throw new ArgumentException($"Argument {Name} requires a string, but nothing was given");
					return new StringArgument(en.Current);
				case CLIArgumentType.Files:
					var files = new List<string>();
					if (isImplicit) {
						if (en.Current == "-")
							return new FilesArgument(files);
						files.Add(en.Current);
					}
					while (en.MoveNext()) {
						if (en.Current == "-")
							break;
						files.Add(en.Current);
					}
					return new FilesArgument(files);
				default:
					throw new NotImplementedException($"Unimplemented argument type: {Type}");
				}
			}
		}

		public interface IArgument { }

		public class FlagArgument : IArgument {
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
		}

		public class NumberArgument : IArgument {
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
		}

		public class StringArgument : IArgument {
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
		}

		public class FilesArgument : IArgument {
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
		}

		public static class EnumeratorExtensions {
			public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator) {
				while (enumerator.MoveNext())
					yield return enumerator.Current;
			}
		}
	}
}
