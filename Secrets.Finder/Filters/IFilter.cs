using System.Collections.Generic;
using System.Threading.Tasks;

namespace Secrets.Finder.Filters {
	internal interface IFilter {
		Task<IEnumerable<string>> FilterStringsAsync( IEnumerable<string> strings );
	}
}
