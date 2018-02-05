using Plossum.CommandLine;

namespace Secrets.Finder {
	[CommandLineManager(
		ApplicationName = "Secrets Finder",
		Copyright = "Copyright (c) Jason Tarka",
		EnabledOptionStyles = OptionStyles.Unix
	)]
    internal class Options {

		public static Options Current { get; private set; }

		public Options() {
			Current = this;
		}

		[CommandLineOption(
			Name = "dir",
			Aliases = "d",
			Description = "The directory to scan",
			MinOccurs = 1,
			MaxOccurs = 1
		)]
		public string Directory { get; set; }

		[CommandLineOption(
			Name = "whitelist",
			Aliases = "w",
			Description = "File containing strings that are whitelisted"
		)]
		public string[] WhitelistFiles { get; set; } = new string[0];

		[CommandLineOption(
			Name = "filters",
			Aliases = "l",
			Description = "File containing regex patterns to ignore"
		)]
		public string[] PatternFiles { get; set; } = new string[0];

		[CommandLineOption(
			Name = "pathFilter",
			Aliases = "p",
			Description = "File containing a list of regexes to filter out paths" +
						  "\nIndividual directories & files are checked, not the final resulting path" +
						  " (eg: node_modules, but not ./node_modules/my-lib/file.js)"
		)]
		public string[] PathFilterFiles { get; set; } = new string[0];

		[CommandLineOption(
			Name = "preParseFilter",
			Aliases = "r",
			Description = "File containing regex patterns to ignore"
		)]
		public string[] PreParseFiles { get; set; } = new string[0];

		[CommandLineOption(
			Name = "maxAllowedEntropy",
			Aliases = "e",
			Description = "The maximum allowed entropy of a string (default: 4.0)" +
						  "\nUse --whitelist or --filters if setting a lower value"
		)]
		public double MaxAllowedEntropy { get; set; } = 4.0;

		[CommandLineOption(
			Name = "help",
			Aliases = "h",
			Description = "Display this help, then exit",
			BoolFunction = BoolFunction.TrueIfPresent
		)]
		public bool Help { get; set; }

		[CommandLineOption(
			Name = "minLength",
			Aliases = "m",
			Description = "Minimum length a string can be for it to be examined",
			BoolFunction = BoolFunction.TrueIfPresent
		)]
		public short MinLength { get; set; } = 15;

		[CommandLineOption(
			Name = "sort",
			Aliases = "s",
			Description = "Sort output by file paths & names",
			BoolFunction = BoolFunction.TrueIfPresent
		)]
		public bool SortOutput { get; set; } = false;
    }
}
