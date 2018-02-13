using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Secrets.Finder.Tools {
	internal static class FilterFiles {

		private static readonly Lazy<Task<HashSet<string>>> m_whitelist = new Lazy<Task<HashSet<string>>>(
				async() => await FilterFileReader.ReadFiles(
						Options.Current.WhitelistFiles,
						s => s
					)
			);

		private static readonly Lazy<Task<IEnumerable<Regex>>> m_pathFilters = new Lazy<Task<IEnumerable<Regex>>>(
				async() => await FilterFileReader.ReadFiles(
						Options.Current.PathFilterFiles,
						pattern => new Regex(
							pattern,
							RegexOptions.Compiled | RegexOptions.CultureInvariant
							| RegexOptions.IgnoreCase | RegexOptions.Singleline
							| RegexOptions.ExplicitCapture
						)
					)
			);

		private static readonly Lazy<Task<IEnumerable<Regex>>> m_patternFilters = new Lazy<Task<IEnumerable<Regex>>>(
				async() => await FilterFileReader.ReadFiles(
						Options.Current.PatternFiles,
						pattern => new Regex(
							pattern,
							RegexOptions.Compiled | RegexOptions.CultureInvariant
							| RegexOptions.IgnoreCase | RegexOptions.Singleline
							| RegexOptions.ExplicitCapture
						)
					)
			);

		private static readonly Lazy<Task<IEnumerable<Regex>>> m_preParseFilters = new Lazy<Task<IEnumerable<Regex>>>(
				async() => await FilterFileReader.ReadFiles(
						Options.Current.PreParseFiles,
						pattern => new Regex(
							pattern,
							RegexOptions.Compiled | RegexOptions.CultureInvariant
							| RegexOptions.ExplicitCapture
						)
					)
			);

		public static Task<HashSet<string>> Whitelist => m_whitelist.Value;
		public static Task<IEnumerable<Regex>> Paths => m_pathFilters.Value;
		public static Task<IEnumerable<Regex>> Patterns => m_patternFilters.Value;
		public static Task<IEnumerable<Regex>> PreParse => m_preParseFilters.Value;
	}
}
