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
			IEnumerable<FileInfo> files = directory.EnumerateFileSystemInfos()
				.Where(
					entry => !fileFilters.Any( filter => filter.IsMatch( entry.Name ) )
				).SelectMany(
					entry => entry is DirectoryInfo
						? FindFilesToScan( (DirectoryInfo)entry, fileFilters )
						: new[] { (FileInfo)entry }
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
			IDictionary<FileInfo, bool> results = tasks
				.ToDictionary(
					kv => kv.Key,
					kv => kv.Value.Result
				);
			IEnumerable<FileInfo> nonBinaryFiles = results
				.Where(
					kv => !kv.Value
				).Select( kv => kv.Key );

			return nonBinaryFiles;
		}

	}
}
