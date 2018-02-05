using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Secrets.Finder.Tools {
	internal static class FilterFileReader {

		public static async Task<HashSet<T>> ReadFiles<T>(
			IEnumerable<string> filePaths,
			Func<string, T> constructor
		) {
			if( filePaths == null ) {
				return new HashSet<T>();
			}

			HashSet<string> allLines = new HashSet<string>();
			foreach( string path in filePaths ) {
				IEnumerable<string> lines = await File.ReadAllLinesAsync( path );
				lines = lines.Where( ShouldIncludeString );
				allLines.UnionWith( lines );
			}

			return allLines.Select( constructor ).ToHashSet();
		}

		private static bool ShouldIncludeString( string str ) {
			return !(
					string.IsNullOrWhiteSpace( str )
					|| str.StartsWith( '#' )
				);
		}
    }
}
