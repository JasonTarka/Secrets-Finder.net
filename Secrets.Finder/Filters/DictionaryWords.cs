using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Secrets.Finder.Filters {
	/// <summary>
	/// Filter out strings that are composed of dictionary words in camelCase or similar formats
	/// eg: MY_VARIABLE, myVariable, MyHTMLGenerator, an-identifier-string
	/// 
	/// This reduces the number of variables, classes, and such that appear
	/// </summary>
	internal class DictionaryWords : IFilter {

		const int MinLength = 3;
		const float RequiredPercent = 0.30f;

		private static readonly Lazy<HashSet<string>> Dictionary = new Lazy<HashSet<string>>(
			() => {
				IEnumerable<string> dictionary = Resources.words
					.Split( '\n' );
				IEnumerable<string> nouns = Resources.properNouns
					.Split( '\n' )
					.Where(
						str => !string.IsNullOrWhiteSpace( str )
							&& !str.StartsWith( '#' )
					).Select( str => str.ToLowerInvariant() );
				return dictionary.Concat( nouns )
					.Where( x => x.Length >= MinLength )
					.ToHashSet();
			} );

		private static readonly char[] NumbersAndCommonSeparators =
			@"0123456789_!@#$%^&*()=+[{]}\|;:'"",<.>/? -".ToCharArray();

		// Split words like "CIAShellCompany" to "CIA ShellCompany"
		private static readonly Regex UpperCaseWords = new Regex(
				"([A-Z]+?)([A-Z][a-z])",
				RegexOptions.Compiled | RegexOptions.CultureInvariant
			);

		private static readonly Regex CamelCase = new Regex(
				"([a-z])([A-Z])",
				RegexOptions.Compiled | RegexOptions.CultureInvariant
			);


		Task<IEnumerable<string>> IFilter.FilterStringsAsync(
			IEnumerable<string> strings
		) => Task.Run(
			() => strings.Where( str => !MadeOfDictionaryWords( str ) )
		);

		private bool MadeOfDictionaryWords( string str ) {
			HashSet<string> dictionary = Dictionary.Value;
			string[] words = SplitIntoWords( str ).ToArray();

			int totalWords = words.Length;
			int realWords = words
				.Where( x => x.Length >= MinLength )
				.Count( w => dictionary.Contains( w ) );

			float percentRealWords = realWords / (float)totalWords;
			return percentRealWords > RequiredPercent;
		}

		private IEnumerable<string> SplitIntoWords( string str ) {
			IEnumerable<string> parts = str.Split( NumbersAndCommonSeparators, StringSplitOptions.RemoveEmptyEntries );

			parts = parts.SelectMany(
					s => UpperCaseWords.Replace( s, "$1 $2" ).Split( ' ', StringSplitOptions.RemoveEmptyEntries )
				).SelectMany(
					s => CamelCase.Replace( s, "$1 $2" ).Split( ' ', StringSplitOptions.RemoveEmptyEntries )
				).Select( word => word.ToLowerInvariant() );
			return parts;
		}
	}
}
