using System;
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
				@"[`#&()|=[\]{};:'\""/\\?<>,.\s\r\n]+",
				RegexOptions.Compiled | RegexOptions.Singleline
			);

		private static readonly Regex UrlEncodedChars = new Regex(
				//  \s"#&'(), / .     : ; < = > ?     [ ]   `    { | }
				"%(2[0236789CcFfEe]|3[AaBbCcDdEeFf]|5[BbDd]|60|7[BbCcDd])",
				RegexOptions.Compiled | RegexOptions.ExplicitCapture
			);

		public static async Task<IEnumerable<string>> TokenizeFile( FileInfo file ) {
			IEnumerable<Regex> patterns = await FilterFiles.PreParse;

			// Filter all content at once so public key files and such can be removed
			string content = await File.ReadAllTextAsync( file.FullName );
			patterns = patterns.Append( UrlEncodedChars );

			content = PreParseLine( content, patterns );
			IEnumerable<string> tokens = TokenSplit.Split( content );
			return tokens.ToHashSet(); // Needs to be distinct, and .ToHashSet() can be faster than .Distinct()
		}

		private static string PreParseLine(
			string line,
			IEnumerable<Regex> patterns
		) {
			return patterns.Aggregate(
					line,
					// Replace the match with a space so the tokenizer will split around it
					( current, pattern ) => pattern.Replace( current, " " )
				);
		}
	}
}
