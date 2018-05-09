using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Plossum.CommandLine;
using Secrets.Finder.Parser;
using Secrets.Finder.Tools;

namespace Secrets.Finder {
	class Program {

		const long SafeFileSizeInMegabytes = 1;
		const long MegabytesInBytes = 1024 * 1024;
		const long SafeFileSizeInBytes = SafeFileSizeInMegabytes * MegabytesInBytes;
		const long SafeLengthInSeconds = 2;
		const long SafeLengthinMilliseconds = SafeLengthInSeconds * 1000;

		static int Main( string[] args ) {
			Options options = GetOptions();
			if( options == null ) {
				return -1;
			}
			if( options.Help ) {
				return 0;
			}

			if( options.GetEntropy ) {
				OutputEntropy( options.Directory );
			} else {
				RunScan();
			}

			if( Debugger.IsAttached ) {
				Console.WriteLine( "Done scanning" );
				Console.WriteLine( "Press any key to continue..." );
				Console.ReadKey();
			}
			return 0;
		}

		private static Options GetOptions() {
			Options options = new Options();
			CommandLineParser parser = new CommandLineParser( options );
			parser.Parse();

			const int width = 78;
			if( options.Help ) {
				DisplayHelp();
			} else if( parser.HasErrors ) {
				Console.WriteLine( parser.UsageInfo.GetErrorsAsString( width ) );
				Console.WriteLine();
				DisplayHelp();
				return null;
			}

			return options;

			void DisplayHelp() {
				Console.WriteLine( parser.UsageInfo.GetHeaderAsString( width ) );
				Console.WriteLine( parser.UsageInfo.GetOptionsAsString( width ) );
				Console.WriteLine( parser.UsageInfo.GetHeaderAsString( width ) );

				if( Debugger.IsAttached ) {
					Console.WriteLine( "Press any key to continue..." );
					Console.ReadKey();
				}
			}
		}

		private static void OutputEntropy( string str ) {
			double entropy = EntropyCalculator.CalculateEntropy( str );
			Console.WriteLine( $"Entropy of '{str}': {entropy}" );
		}

		private static void RunScan() {
			Task<IEnumerable<FileInfo>> findFilesToScan = FileFinder.FindFilesToScanAsync();
			findFilesToScan.Wait();
			IEnumerable<FileInfo> files = findFilesToScan.Result;

			if( Options.Current.SortOutput ) {
				RunScanSorted( files );
				return;
			}

			IEnumerable<Task> tasks = files.Select( ScanAndOutput );
			Task.WaitAll( tasks.ToArray() );
		}

		private static void RunScanSorted(
			IEnumerable<FileInfo> files
		) {
			IDictionary<FileInfo, Task<IList<string>>> tasks = files.ToDictionary(
					f => f,
					ScanFile
				);

			Task.WaitAll( tasks.Values.ToArray() );

			IEnumerable<KeyValuePair<FileInfo, Task<IList<string>>>> ordered = tasks
				.Where( kv => kv.Value.Result.Count > 0 )
				.OrderBy( kv => kv.Key.FullName );

			foreach( var pair in ordered ) {
				OutputSecrets(
						pair.Key,
						pair.Value.Result
					);
			}
		}

		private static async Task<IList<string>> ScanFile( FileInfo file ) {
			Stopwatch timer = Stopwatch.StartNew();

			IEnumerable<string> secrets = await FileScanner.FindSecrets( file );
#endif

			timer.Stop();
			if( Options.Current.ShowWarnings && timer.ElapsedMilliseconds > SafeLengthinMilliseconds ) {
				secrets = secrets.Prepend(
						$"[Took {timer.ElapsedMilliseconds}ms; are you sure this should be included?]"
					);
			}
			return secrets.ToList();
		}

		private static async Task ScanAndOutput( FileInfo file ) {
			Stopwatch timer = Stopwatch.StartNew();

			IEnumerable<string> secrets = await FileScanner.FindSecrets( file );

			timer.Stop();
			if( Options.Current.ShowWarnings && timer.ElapsedMilliseconds > SafeLengthinMilliseconds ) {
				secrets = secrets.Prepend(
					$"[Took {timer.ElapsedMilliseconds}ms; are you sure this should be included?]"
				);
			}

			OutputSecrets( file, secrets );
		}

		private static void OutputSecrets(
			FileInfo file,
			IEnumerable<string> secrets
		) {
			StringBuilder output = new StringBuilder();

			bool headerAdded = false; // Track this to avoid multiple enumeration with .Any() or similar
			foreach( string secret in secrets ) {
				if( !headerAdded ) {
					output.AppendLine( GetHeader( file ) );
					headerAdded = true;
				}

				output.AppendLine( secret );
			}

			if( Options.Current.ShowWarnings && !headerAdded && file.Length >= SafeFileSizeInBytes ) {
				output.AppendLine( GetHeader( file ) );
				headerAdded = true;
			}

			if( headerAdded ) {
				Console.WriteLine( output.ToString() );
			}
		}

		private static string GetHeader( FileInfo file ) {
			string dir = Options.Current.Directory;
			dir = Path.GetFullPath( dir );
			string filePath = file.FullName.Replace( $@"{dir}\", "" ).Replace( '\\', '/' );

			string header = $"[{filePath}]"
				+ $"\r\n[ { GetUrl( dir, filePath )} ]";

			if( Options.Current.ShowWarnings && file.Length >= SafeFileSizeInBytes ) {
				float megabytes = file.Length / (float)MegabytesInBytes;
				header += $"\r\n[File is {megabytes.ToString( "F" )} MiB; are you sure this should be included?]";
			}

			return header;
		}

		private static readonly Regex GithubRegex = new Regex(
				@"github[/\\]([a-zA-Z0-9_.-]+)$",
				RegexOptions.Compiled
			);

		private static string GetUrl( FileInfo file ) {
			return GetUrl(
				Options.Current.Directory,
				file.FullName.Replace( $@"{Options.Current.Directory}\", "" ).Replace( '\\', '/' )
			);
		}

		private static string GetUrl(
			string dir,
			string filePath
		) {
			filePath = filePath.Replace( " ", "%20" );
			Match match = GithubRegex.Match( dir );
			if( match.Success ) {
				// Special case, due to Windows not allowing "CON." filenames
				string repoName = match.Groups[1].Value.StartsWith( "CON-" )
					? match.Groups[1].Value.Replace( "CON-", "CON." )
					: match.Groups[1].Value;
				string url = $"https://github.com/Brightspace/{repoName}/blob/master/{filePath}";
				return url;
			}

			return null;
		}
	}
}
