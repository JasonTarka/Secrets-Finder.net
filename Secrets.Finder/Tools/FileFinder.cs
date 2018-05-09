using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Secrets.Finder.Tools {
	internal static class FileFinder {

		public static async Task<IEnumerable<FileInfo>> FindFilesToScanAsync() {
			string directoryPath = Options.Current.Directory;
			IEnumerable<Regex> fileFilters = await FilterFiles.Paths;
			IEnumerable<FileInfo> files;
			try {
				files = FindFilesToScan(
					new DirectoryInfo( directoryPath ),
					fileFilters
				);
			} catch( Exception e ) {
				throw new Exception(
						$"Error occurred finding files in '{directoryPath}'",
						e
					);
			}
			files = FilterOutBinaryFiles( files );
			if( Debugger.IsAttached ) {
				files = files.ToArray();
				var extensions = files.Select( x => Path.GetExtension( x.Name ) )
					.ToHashSet();

				Debug.WriteLine( "===== Non-Binary Extensions =====" );
				foreach( string extension in extensions ) {
					Debug.WriteLine( extension );
				}
			}
			return files;
		}

		private static IEnumerable<FileInfo> FindFilesToScan(
			DirectoryInfo directory,
			IEnumerable<Regex> fileFilters
		) {
			IEnumerable<FileInfo> files = directory.EnumerateFiles()
				.Where(
					entry => !fileFilters.Any( filter => filter.IsMatch( entry.Name ) )
				);

			IEnumerable<DirectoryInfo> directories = directory.EnumerateDirectories()
				.Where(
					entry => !fileFilters.Any( filter => filter.IsMatch( entry.Name ) )
				);

			files = files.Concat(
					directories.SelectMany(
						dir => FindFilesToScan( dir, fileFilters )
					)
				);

			return files;
		}

		private static IEnumerable<FileInfo> FilterOutBinaryFiles(
			IEnumerable<FileInfo> files
		) {
			Dictionary<FileInfo, Task<bool>> tasks = files.ToDictionary(
				file => file,
				BinaryFileFilter.IsBinaryFileAsync
			);

			Task.WaitAll( tasks.Values.ToArray() );

			IEnumerable<FileInfo> nonBinaryFiles = tasks
				.Where(
					kv => !kv.Value.Result
				).Select( kv => kv.Key );

			return nonBinaryFiles.ToList();
		}

	}
}
