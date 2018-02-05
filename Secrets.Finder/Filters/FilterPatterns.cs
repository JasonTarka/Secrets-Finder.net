using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Secrets.Finder.Tools;

namespace Secrets.Finder.Filters {
	internal class FilterPatterns : IFilter {

		async Task<IEnumerable<string>> IFilter.FilterStringsAsync(
			IEnumerable<string> strings
		) {
			IEnumerable<Regex> filters = await FilterFiles.Patterns;

			return strings.Where(
					str => !FilterMatches( str, filters )
				);
		}

		private bool FilterMatches( string str, IEnumerable<Regex> filters ) {
			foreach( Regex filter in filters ) {
				if( filter.IsMatch( str ) ) {
					return true;
				}
			}
			return false;
		}

	}
}
