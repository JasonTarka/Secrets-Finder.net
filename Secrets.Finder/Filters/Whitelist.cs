using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Secrets.Finder.Tools;

namespace Secrets.Finder.Filters {
	internal class Whitelist : IFilter {

		async Task<IEnumerable<string>> IFilter.FilterStringsAsync(
			IEnumerable<string> strings
		) {
			HashSet<string> whitelist = await FilterFiles.Whitelist;
			return strings.Where(
					str => !whitelist.Contains( str )
				);
		}

	}
}
