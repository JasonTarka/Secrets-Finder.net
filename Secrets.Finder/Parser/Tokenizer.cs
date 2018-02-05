using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Secrets.Finder.Tools;

namespace Secrets.Finder.Parser {
	internal static class Tokenizer {
		// Note: this includes / to ensure URLs are split, but also splits base64-encoded strings
		private static readonly Regex TokenSplit = new Regex(
				@"[`#&()|=[\]{};:'\""/?<>,.\s]+",
				RegexOptions.Compiled
			);

		public static async Task<IEnumerable<string>> TokenizeFile( FileInfo file ) {
			IEnumerable<string> lines = await File.ReadAllLinesAsync( file.FullName );

			IEnumerable<Regex> patterns = await FilterFiles.PreParse;
			lines = PreParseLines( lines, patterns );
			IEnumerable<string> tokens = lines.SelectMany(
					line => TokenSplit.Split( line )
				);
			return tokens.ToHashSet(); // Needs to be distinct, and .ToHashSet() can be faster than .Distinct()
		}

		/// <summary>
		/// Pre-parse lines to remove filters that the user has specified
		/// </summary>
		/// <param name="lines"></param>
		/// <returns></returns>
		public static IEnumerable<string> PreParseLines(
			IEnumerable<string> lines,
			IEnumerable<Regex> patterns
		) {
			return lines.Select( line => PreParseLine( line, patterns ) )
				.ToList();
		}

		public static string PreParseLine(
			string line,
			IEnumerable<Regex> patterns
		) {
			foreach( Regex pattern in patterns ) {
				// Just replace it with a space so the tokenizer will split around it
				line = pattern.Replace( line, " " );
			}
			return line;
		}
	}
}
