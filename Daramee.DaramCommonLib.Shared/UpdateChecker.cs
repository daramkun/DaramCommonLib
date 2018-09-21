using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Daramee.DaramCommonLib
{
	public sealed class UpdateChecker
	{
		string _versionFormat;

		string newestVersion = null;

		public string UpdateURL { get { return $"https://github.com/{ProgramHelper.GitHubAuthor}/{ProgramHelper.GitHubRepositoryName}/releases"; } }

		public UpdateChecker ( string versionFormat )
		{
			_versionFormat = versionFormat;
		}

		public string ThisVersion
		{
			get
			{
				Version currentVersion = ProgramHelper.ApplicationVersion;
				return string.Format ( _versionFormat, currentVersion.Major, currentVersion.Minor, currentVersion.Build, currentVersion.Revision );
			}
		}

		public async Task<string> GetNewestVersion ( bool forceCheck = false, string updateUrl = null )
		{
			if ( newestVersion != null && !forceCheck )
				return newestVersion;

			Stream stream = null;
			try
			{
				HttpWebRequest req = WebRequest.CreateHttp ( updateUrl ?? UpdateURL );
				req.Proxy = null;

				HttpWebResponse res = await req.GetResponseAsync () as HttpWebResponse;

				stream = res.GetResponseStream ();
				using ( StreamReader reader = new StreamReader ( stream ) )
				{
					stream = null;
					string text = reader.ReadToEnd ();

					/*int begin = text.IndexOf ( "<span class=\"css-truncate-target\">" );
					if ( begin == -1 ) { return null; }
					else begin += "<span class=\"css-truncate-target\">".Length;

					int end = text.IndexOf ( "</span>", begin );
					if ( end == -1 ) { return null; };*/
					var match = Regex.Match ( text,
						updateUrl == null
						? "<div class=\"f1 flex-auto min-width-0 text-normal\">[ \t\r\n]*<a href=\"/daramkun/DaramRenamer/releases/tag/[0-9.]+\">[ \t\r\n]*([0-9.]+)[ \t\r\n]*</a>[ \t\r\n]*</div>"
						: "\\[assembly: AssemblyVersion \\( \"([0-9].[0-9].[0-9].[0-9])\" \\)\\]" );

					//return newestVersion = text.Substring ( begin, end - begin );
					if ( match.Groups.Count > 1 )
						return newestVersion = match.Groups [ 1 ].Value;
					if ( updateUrl == null )
						return await GetNewestVersion ( forceCheck, "https://raw.githubusercontent.com/daramkun/DaramRenamer/master/Daramkun.DaramRenamer/Properties/AssemblyInfo.cs" );
					return null;
				}
			}
			catch { return newestVersion = null; }
			finally { if ( stream != null ) stream.Dispose (); }
		}

		public async Task<bool?> CheckUpdate ()
		{
			string newest = await GetNewestVersion ( true );
			if ( newest == null ) return false;
			return newest != ThisVersion;
		}

		public void ShowDownloadPage ()
		{
			Process.Start ( UpdateURL );
		}
	}
}
