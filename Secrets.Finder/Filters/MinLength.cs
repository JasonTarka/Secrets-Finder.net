using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Secrets.Finder.Filters {
	internal class MinLength : IFilter {

		Task<IEnumerable<string>> IFilter.FilterStringsAsync(
			IEnumerable<string> strings
		) {
			return Task.Run( () => strings.Where(
				str => !string.IsNullOrWhiteSpace( str )
					&& str.Length >= Options.Current.MinLength
			) );
		}

	}
}
