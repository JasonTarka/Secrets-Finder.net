using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Secrets.Finder.Filters;

namespace Secrets.Finder.Parser {
	internal static class FileScanner {

		// TODO: Make these lists external
		/// <summary>
		/// Filters to run before determining entropy of string.
		/// these will be run on *every* string found, so should be very fast
		/// </summary>
		private static readonly IFilter[] PreFilters = {
			new MinLength(),
			new Whitelist(),
			new Identifiers()
		};

		/// <summary>
		/// Filters to run on strings that contain more than the allowed level of entropy.
		/// These should be quick, but the number of strings they will be run on is very
		/// small compared to the pre filters
		/// </summary>
		private static readonly IFilter[] PostFilters = {
			new FilterPatterns(),
			new DictionaryWords()
		};

		public static async Task<IEnumerable<string>> FindSecrets(
			FileInfo file
		) {
			IEnumerable<string> secrets = await Tokenizer.TokenizeFile( file );

			foreach( IFilter filter in PreFilters ) {
				secrets = await filter.FilterStringsAsync( secrets );
			}

			double maxAllowedEntropy = Options.Current.MaxAllowedEntropy;
			secrets = secrets.Where(
					s => EntropyCalculator.CalculateEntropy( s ) > maxAllowedEntropy
				);

			foreach( IFilter filter in PostFilters ) {
				secrets = await filter.FilterStringsAsync( secrets );
			}

			return secrets;
		}
	}
}
