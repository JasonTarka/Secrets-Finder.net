using System;
using System.Collections.Generic;
using System.Linq;

namespace Secrets.Finder.Parser {
	internal static class EntropyCalculator {

		/// <summary>
		/// Calculate Shannon's entropy for a string
		/// </summary>
		/// <param name="str">The string to calculate entropy for</param>
		/// <returns></returns>
		public static double CalculateEntropy( string str ) {
			char[] chars = str.ToCharArray();
			IDictionary<char, int> occurrences = new Dictionary<char, int>( chars.Length );

			foreach( char c in chars ) {
				occurrences[c] = occurrences.ContainsKey( c )
					? occurrences[c] + 1
					: 1;
			}

			double length = str.Length;
			double entropy = occurrences.Values
				.Select( count => count / length )
				.Aggregate<double, double>(
					0, ( current, p ) => current - p * ( Math.Log( p ) / Math.Log( 2 ) )
				);

			return entropy;
		}

	}
}
