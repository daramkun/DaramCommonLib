using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Daramee.DaramCommonLib
{
	public sealed class StringTable
	{
		public static StringTable SharedTable { get; private set; }
		public static Dictionary<string, string> SharedStrings => SharedTable.Strings;
		static DataContractJsonSerializer serializer = new DataContractJsonSerializer ( typeof ( IOContract ), new DataContractJsonSerializerSettings ()
		{
			UseSimpleDictionaryFormat = true,
		} );

		Dictionary<CultureInfo, Dictionary<string, string>> tables;

		public string TargetApp { get; private set; }
		public string TargetVersion { get; private set; }
		public string Author { get; private set; }
		public string Copyright { get; private set; }
		public string Contact { get; private set; }

		public IEnumerable<CultureInfo> AvailableCultures => tables.Keys;
		CultureInfo lastCultureInfo = CultureInfo.CurrentUICulture, cultureCache = CultureInfo.CurrentUICulture;
		public Dictionary<string, string> Strings => tables [ cultureCache ];

		public CultureInfo CurrentCulture
		{
			get
			{
				if ( lastCultureInfo != CultureInfo.CurrentUICulture )
				{
					if ( tables.ContainsKey ( CultureInfo.CurrentUICulture ) )
						cultureCache = CultureInfo.CurrentUICulture;
					else if ( tables.Count > 1 )
					{
						bool changed = false;
						foreach ( CultureInfo c in AvailableCultures )
						{
							if ( c.ThreeLetterISOLanguageName == CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName )
							{
								cultureCache = c;
								changed = true;
								break;
							}
						}

						if ( !changed )
							cultureCache = CultureInfo.CurrentUICulture;
					}
					lastCultureInfo = CultureInfo.CurrentUICulture;
				}
				return cultureCache;
			}
		}

		public Dictionary<string, string> this [ CultureInfo cultureInfo ] => tables [ cultureInfo ];

		[DataContract]
		private class IOContract
		{
			[DataMember ( Name = "target" )]
			public string TargetApp;
			[DataMember ( Name = "version", IsRequired = false )]
			public string TargetVersion;
			[DataMember ( Name = "author", IsRequired = false )]
			public string Author;
			[DataMember ( Name = "copyright", IsRequired = false )]
			public string Copyright;
			[DataMember ( Name = "contact", IsRequired = false )]
			public string Contact;

			[DataContract]
			public class Language
			{
				[DataMember ( Name = "language" )]
				public string LanguageRegion;
				[DataMember ( Name = "table" )]
				public Dictionary<string, string> Table;
			}

			[DataMember ( Name = "languages" )]
			public List<Language> Languages;
		}

		public StringTable ()
		{
			if ( Directory.Exists ( "./Strings" ) )
			{
				foreach ( string filename in Directory.GetFiles ( "./Strings/", $"{ProgramHelper.ApplicationName}.Strings.*.json" ) )
				{
					if ( Regex.IsMatch ( filename, $"{ProgramHelper.ApplicationName}.Strings.[a-zA-Z0-9\\-_].json" ) )
					{
						if ( filename.IndexOf ( CultureInfo.CurrentUICulture.Name ) >= 0 )
						{
							using ( Stream stream = new FileStream ( filename, FileMode.Open, FileAccess.Read ) )
							{
								LoadTable ( stream );
								return;
							}
						}
					}
				}
			}

			LoadTable ( ProgramHelper.ApplicationAssembly.GetManifestResourceStream ( $"{ProgramHelper.ApplicationNamespace}.Strings.json" ) );

			SharedTable = this;
		}

		public StringTable ( Stream stream )
		{
			LoadTable ( stream );
			SharedTable = this;
		}

		private IOContract GetContract ( Stream stream )
		{
			using ( StreamReader reader = new StreamReader ( stream, Encoding.UTF8, true ) )
			{
				using ( Stream mem = new MemoryStream ( Encoding.UTF8.GetBytes ( reader.ReadToEnd () ) ) )
				{
					return serializer.ReadObject ( mem ) as IOContract;
				}
			}
		}

		private void LoadTable ( Stream stream )
		{
			IOContract contract = GetContract ( stream );
			TargetApp = contract.TargetApp;
			TargetVersion = contract.TargetVersion;
			Author = contract.Author;
			Copyright = contract.Copyright;
			Contact = contract.Contact;
			tables = new Dictionary<CultureInfo, Dictionary<string, string>> ();
			foreach ( var table in contract.Languages )
			{
				tables.Add ( CultureInfo.GetCultureInfo ( table.LanguageRegion ), table.Table );
			}
		}
	}
}
