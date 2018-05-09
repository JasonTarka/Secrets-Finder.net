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
			"bmp",
			"svg",
			"tiff",
			"tif",
			"bmp",
			// Media
			"avi",
			"mpg",
			"mpg4",
			"mpeg",
			"mp4",
			"mp3",
			"mpeg3",
			"mov",
			"swf",
			// Documents
			"pdf",
			"doc",
			"docx",
			"xls",
			"xlsx",
			"odt",
			"odp",
			"ai",
			"ppt",
			"pptx",
			// Executables
			"exe",
			"dll",
			"so",
			"bin",
			"jar",
			"pyc",
			// Database backups
			"mdf",
			"ldf",
			// Font files
			"ttf",
			"ufm",
			"afm",
			// Not entirely sure
			"nib"
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

		private static readonly HashSet<char> AllowedControlChars = new HashSet<char> {
			'\r',
			'\n',
			'\t'
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
						c => char.IsControl( c ) && !AllowedControlChars.Contains( c )
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
