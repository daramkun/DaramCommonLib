using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Microsoft.Win32;
using JsonSerializer = System.Runtime.Serialization.Json.DataContractJsonSerializer;
using JsonSerializerSettings = System.Runtime.Serialization.Json.DataContractJsonSerializerSettings;

namespace Daramee.DaramCommonLib
{
	public class OptionInfoAttribute : Attribute
	{
		public object DefaultValue { get; set; }
		public IValueConverter ValueConverter { get; set; }
	}

	public sealed class Optionizer<T> where T : class
	{
		public static Optionizer<T> SharedOptionizer { get; private set; }
		public static T SharedOptions { get { return SharedOptionizer.Options; } }

		JsonSerializer serializer = new JsonSerializer ( typeof ( T ), new JsonSerializerSettings () { UseSimpleDictionaryFormat = true } );

		string _ownAuthor, _ownTitle;

		public T Options { get; set; }
		public bool IsSaveToRegistry { get; set; } = true;
		
		public Optionizer ( string ownAuthor, string ownTitle )
		{
			SharedOptionizer = this;

			_ownAuthor = ownAuthor;
			_ownTitle = ownTitle;

			if ( File.Exists ( $"{AppDomain.CurrentDomain.BaseDirectory}\\{ownTitle}.config.json" ) )
			{
				IsSaveToRegistry = false;

				using ( Stream stream = File.Open ( $"{AppDomain.CurrentDomain.BaseDirectory}\\{ownTitle}.config.json", FileMode.Open ) )
				{
					if ( stream.Length != 0 )
						Options = serializer.ReadObject ( stream ) as T;
				}
			}
			else
			{
				Options = Activator.CreateInstance<T> ();

				var userKey = Registry.CurrentUser;
				var swKey = userKey.OpenSubKey ( "SOFTWARE" );
				var daramworldKey = swKey.OpenSubKey ( ownAuthor );
				if ( daramworldKey != null )
				{
					var renamerKey = daramworldKey.OpenSubKey ( ownTitle );
					if ( renamerKey != null )
					{
						IsSaveToRegistry = true;

						Type optionType = typeof ( T );
						foreach ( var prop in optionType.GetProperties () )
						{
							DataMemberAttribute dataMember = null;
							OptionInfoAttribute optionInfo = null;
							foreach ( var attr in prop.GetCustomAttributes ( true ) )
								if ( attr is DataMemberAttribute )
									dataMember = attr as DataMemberAttribute;
								else if ( attr is OptionInfoAttribute )
									optionInfo = attr as OptionInfoAttribute;
							if ( dataMember == null )
								continue;

							if ( optionInfo == null )
								prop.SetValue ( Options, renamerKey.GetValue ( dataMember.Name ) );
							else
							{
								var value = renamerKey.GetValue ( dataMember.Name, optionInfo.DefaultValue );
								if ( optionInfo.ValueConverter != null )
									prop.SetValue ( Options, optionInfo.ValueConverter.Convert ( value, null, null, null ) );
								else
									prop.SetValue ( Options, value );
							}
						}
					}
				}
			}
		}

		public void Save ()
		{
			if ( IsSaveToRegistry )
			{
				if ( File.Exists ( $"{AppDomain.CurrentDomain.BaseDirectory}\\{_ownTitle}.config.json" ) )
					File.Delete ( $"{AppDomain.CurrentDomain.BaseDirectory}\\{_ownTitle}.config.json" );

				var userKey = Registry.CurrentUser;
				var swKey = userKey.OpenSubKey ( "SOFTWARE", true );
				var daramworldKey = swKey.OpenSubKey ( _ownAuthor, true );
				if ( daramworldKey == null ) daramworldKey = swKey.CreateSubKey ( _ownAuthor, RegistryKeyPermissionCheck.ReadWriteSubTree );
				var renamerKey = daramworldKey.OpenSubKey ( _ownTitle, true );
				if ( renamerKey == null ) renamerKey = daramworldKey.CreateSubKey ( _ownTitle, RegistryKeyPermissionCheck.ReadWriteSubTree );

				Type optionType = typeof ( T );
				foreach ( var prop in optionType.GetProperties () )
				{
					DataMemberAttribute dataMember = null;
					OptionInfoAttribute optionInfo = null;
					foreach ( var attr in prop.GetCustomAttributes ( true ) )
						if ( attr is DataMemberAttribute )
							dataMember = attr as DataMemberAttribute;
						else if ( attr is OptionInfoAttribute )
							optionInfo = attr as OptionInfoAttribute;
					if ( dataMember == null )
						continue;

					if ( optionInfo == null )
						renamerKey.SetValue ( dataMember.Name, prop.GetValue ( Options ) );
					else
					{
						var value = prop.GetValue ( Options );
						if ( optionInfo.ValueConverter != null )
							renamerKey.SetValue ( dataMember.Name, optionInfo.ValueConverter.ConvertBack ( value, null, null, null ) );
						else
							renamerKey.SetValue ( dataMember.Name, value );
					}
				}
			}
			else
			{
				var userKey = Registry.CurrentUser;
				var swKey = userKey.OpenSubKey ( "SOFTWARE", true );
				var daramworldKey = swKey.OpenSubKey ( _ownAuthor, true );
				if ( daramworldKey != null )
				{
					var renamerKey = daramworldKey.OpenSubKey ( _ownTitle, true );
					if ( renamerKey != null )
						daramworldKey.DeleteSubKey ( _ownTitle, true );
				}

				using ( Stream stream = File.Open ( $"{AppDomain.CurrentDomain.BaseDirectory}\\{_ownTitle}.config.json", FileMode.Create ) )
					serializer.WriteObject ( stream, Options );
			}
		}
	}
}
