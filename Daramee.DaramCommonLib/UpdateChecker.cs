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

		public string UpdateURL { get { return $"https://github.com/{ProgramHelper.GitHubAuthor}/{ProgramHelper.GitHubRepositoryName}/releases"; } }

		public UpdateChecker ( string versionFormat )
		{
			_versionFormat = versionFormat;
		}

		public async Task<bool?> CheckUpdate ()
		{
			Stream stream = null;
			string version = null;
			bool checkUpdate = false;
			try
			{
				HttpWebRequest req = WebRequest.CreateHttp ( UpdateURL );
				req.Proxy = null;

				HttpWebResponse res = await req.GetResponseAsync () as HttpWebResponse;

				stream = res.GetResponseStream ();
				using ( StreamReader reader = new StreamReader ( stream ) )
				{
					stream = null;
					string text = reader.ReadToEnd ();
					int begin = text.IndexOf ( "<span class=\"css-truncate-target\">" );
					if ( begin == -1 ) { version = null; } else begin += "<span class=\"css-truncate-target\">".Length;
					int end = text.IndexOf ( "</span>", begin );
					if ( end == -1 ) { version = null; };
					version = text.Substring ( begin, end - begin );
					Version currentVersion = ProgramHelper.ApplicationVersion;
					string current = string.Format ( _versionFormat, currentVersion.Major, currentVersion.Minor, currentVersion.Build, currentVersion.Revision );
					checkUpdate = version != current;
				}
			}
			catch { version = null; return null; }
			finally { if ( stream != null ) stream.Dispose (); }

			return checkUpdate;
		}

		public void ShowDownloadPage ()
		{
			Process.Start ( UpdateURL );
		}
	}
}
