using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Secrets.Finder.Filters {
	/// <summary>
	/// Filter out object identifiers, such as "MATHJAX_CODEFILE", or "REG_DSPREG_EMIFA_CECTL0"
	/// 
	/// This exists because custom filters are case-insensitive, while the purpose of this
	/// is to be case-sensitive.
	/// </summary>
	internal sealed class Identifiers : IFilter {

		private static readonly Regex IdentifierRegex = new Regex(
				"^[A-Z0-9]+(_[A-Z0-9]+)+$",
				RegexOptions.Compiled
			);

		Task<IEnumerable<string>> IFilter.FilterStringsAsync(
			IEnumerable<string> strings
		) => Task.Run(
			() => strings.Where( str => !IdentifierRegex.IsMatch( str ) )
		);

	}
}
