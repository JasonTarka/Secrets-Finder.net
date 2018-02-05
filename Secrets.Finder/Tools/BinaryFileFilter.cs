using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Secrets.Finder.Tools {

	internal static class BinaryFileFilter {

		const int CharsToCheck = 1024;
		const float MaxControlCharsPercent = 0.05f; // 5% can be control chars to not be binary

		private static readonly HashSet<string> BinaryExtensions = new HashSet<string> {
			// Images
			"jpg",
			"jpeg",
			"gif",
			"png",
			"ico",
			// Media
			"avi",
			"mpg",
			"mpg4",
			"mpeg",
			"mp4",
			"mp3",
			"mpeg3",
			"mov",
			// Documents
			"pdf",
			"doc",
			"docx",
			"xls",
			"xlsx",
			"odt",
			"odp",
			// Executables
			"exe",
			"dll",
			"so"
		};

		private static readonly HashSet<string> ArchiveExtensions = new HashSet<string> {
			"zip",
			"gzip",
			"gz",
			"tar",
			"tgz",
			"bz",
			"bz2",
			"7zip",
			"7z"
		};

		public static async Task<bool> IsBinaryFileAsync( FileInfo file ) {
			if( IsBinaryExtension( file ) ) {
				return true;
			}

			using( FileStream stream = file.OpenRead() )
			using( TextReader reader = new StreamReader( stream ) ) {
				char[] chars = new char[CharsToCheck];
				int totalChars = await reader.ReadAsync( chars, 0, CharsToCheck );

				if( totalChars == 0 ) {
					return true;
				}

				int controlChars = chars.Take( totalChars )
					.Count(
						c => char.IsControl( c ) && c != '\n' && c != '\r'
					);

				float percentControlChars = controlChars / (float)totalChars;
				bool isBinaryFile = percentControlChars >= MaxControlCharsPercent;
				return isBinaryFile;
			}
		}

		private static bool IsBinaryExtension( FileInfo file ) {
			try {
				string ext = Path.GetExtension( file.Name );
				ext = ext.Substring( 1 ); // Trim the leading '.'

				return BinaryExtensions.Contains( ext )
					|| ArchiveExtensions.Contains( ext );
			} catch( ArgumentException ) {
				// If Path can't parse the file we can't do anything with it
				return false;
			}
		}

	}
}
