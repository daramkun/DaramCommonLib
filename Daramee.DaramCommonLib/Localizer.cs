using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using JsonSerializer = System.Runtime.Serialization.Json.DataContractJsonSerializer;
using JsonSerializerSettings = System.Runtime.Serialization.Json.DataContractJsonSerializerSettings;

namespace Daramee.DaramCommonLib
{
	[DataContract]
	public class LocalizeCulture
	{
		[DataMember ( Name = "version" )]
		public string Version;
		[DataMember ( Name = "target" )]
		public string Target;
		[DataMember ( Name = "culture" )]
		public string Culture;
		[DataMember ( Name = "author" )]
		public string Author;
		[DataMember ( Name = "contact", IsRequired = false )]
		public string Contact;
		[DataMember ( Name = "contents" )]
		public Dictionary<string, string> Contents;
	}

	[DataContract]
	class LocalizationContainer
	{
		[DataMember ( Name = "lang" )]
		public List<LocalizeCulture> Languages = new List<LocalizeCulture> ();
	}

	public class Localizer
	{
		CultureInfo [] cultureInfos;
		Dictionary<CultureInfo, LocalizeCulture> Cultures = new Dictionary<CultureInfo, LocalizeCulture> ();

		public static Localizer SharedLocalizer { get; private set; }
		public static Dictionary<string,string> SharedStrings { get { return SharedLocalizer.Strings; } }

		public CultureInfo CurrentCulture
		{
			get { return CultureInfo.CurrentUICulture; }
		}
		public LocalizeCulture Culture { get; private set; }
		public CultureInfo [] AvailableCultures { get { return cultureInfos; } }
		public Dictionary<string, string> Strings { get { return Culture.Contents; } }

		public Localizer ()
		{
			SharedLocalizer = this;

			var json = new JsonSerializer ( typeof ( LocalizeCulture ), new JsonSerializerSettings () { UseSimpleDictionaryFormat = true } );
			var json2 = new JsonSerializer ( typeof ( LocalizationContainer ), new JsonSerializerSettings () { UseSimpleDictionaryFormat = true } );

			var queue = new Queue<LocalizeCulture> ();

			foreach ( var ci in CultureInfo.GetCultures ( CultureTypes.InstalledWin32Cultures ) )
			{
				var localizationFiles = new string [] {
					$"Localization\\Localization.{ProgramHelper.ApplicationName}.{ci}.json",
					$"Localization.{ProgramHelper.ApplicationName}.{ci}.json",
				};
				foreach ( var globalizationFile in localizationFiles )
				{
					Stream gs = null;
					if ( File.Exists ( globalizationFile ) )
						gs = new FileStream ( globalizationFile, FileMode.Open, FileAccess.Read );
					else continue;

					gs.Position = 3;
					var igc = json.ReadObject ( gs ) as LocalizeCulture;
					AddLocalizeCulture ( ProgramHelper.ApplicationName, queue, igc );

					gs.Dispose ();
				}
			}

			var localizationContainerFiles = new string [] {
				$"Localization\\Localization.{ProgramHelper.ApplicationName}.json",
				$"Localization.{ProgramHelper.ApplicationName}.json",
			};
			foreach ( var globalizationContainerFile in localizationContainerFiles )
			{
				Stream gs2 = null;
				if ( File.Exists ( globalizationContainerFile ) )
					gs2 = new FileStream ( globalizationContainerFile, FileMode.Open, FileAccess.Read );
				else continue;

				gs2.Position = 3;
				var iggc = json2.ReadObject ( gs2 ) as LocalizationContainer;
				foreach ( var l in iggc.Languages )
					AddLocalizeCulture ( ProgramHelper.ApplicationName, queue, l );

				gs2.Dispose ();
			}

			using ( var stream = ProgramHelper.ApplicationAssembly.GetManifestResourceStream ( $"{ProgramHelper.ApplicationNamespace}.Localization.json" ) )
			{
				stream.Position = 3;
				var ggc = json2.ReadObject ( stream ) as LocalizationContainer;
				foreach ( var l in ggc.Languages )
					AddLocalizeCulture ( ProgramHelper.ApplicationName, queue, l );
			}

			while ( queue.Count > 0 )
			{
				var g = queue.Dequeue ();
				var cultureInfo = CultureInfo.GetCultureInfo ( g.Culture );
				if ( Cultures.ContainsKey ( cultureInfo ) )
				{
					if ( new Version ( Cultures [ cultureInfo ].Version ) < new Version ( g.Version ) )
						Cultures [ cultureInfo ] = g;
				}
				else Cultures.Add ( cultureInfo, g );
			}

			int i = 0;
			cultureInfos = new CultureInfo [ Cultures.Count ];
			foreach ( var key in Cultures.Keys )
				cultureInfos [ i++ ] = key;

			Refresh ();
		}

		public bool Refresh ()
		{
			if ( Cultures.ContainsKey ( CultureInfo.CurrentUICulture ) )
				Culture = Cultures [ CultureInfo.CurrentUICulture ];
			else if ( CultureInfo.CurrentUICulture.Parent != null && Cultures.ContainsKey ( CultureInfo.CurrentUICulture.Parent ) )
				Culture = Cultures [ CultureInfo.CurrentUICulture.Parent ];
			else if ( Cultures.ContainsKey ( CultureInfo.GetCultureInfo ( "en-US" ) ) )
				Culture = Cultures [ CultureInfo.GetCultureInfo ( "en-US" ) ];
			else if ( Cultures.ContainsKey ( CultureInfo.GetCultureInfo ( "en" ) ) )
				Culture = Cultures [ CultureInfo.GetCultureInfo ( "en" ) ];
			else
				return false;
			return true;
		}

		private void AddLocalizeCulture ( string ownTitle, Queue<LocalizeCulture> queue, LocalizeCulture localizeCulture )
		{
			if ( !Cultures.ContainsKey ( CultureInfo.GetCultureInfo ( localizeCulture.Culture ) ) )
				if ( localizeCulture.Target == ownTitle )
					queue.Enqueue ( localizeCulture );
		}
	}
}
