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
		
		public Optionizer ( string ownAuthor, string ownTitle )
		{
			SharedOptionizer = this;

			_ownAuthor = ownAuthor;
			_ownTitle = ownTitle;

			if ( File.Exists ( $"{AppDomain.CurrentDomain.BaseDirectory}\\{ownTitle}.config.json" ) )
			{
				using ( Stream stream = File.Open ( $"{AppDomain.CurrentDomain.BaseDirectory}\\{ownTitle}.config.json", FileMode.Open ) )
				{
					if ( stream.Length != 0 )
						Options = serializer.ReadObject ( stream ) as T;
				}
			}
			else
				Options = Activator.CreateInstance<T> ();
		}

		public void Save ()
		{
			using ( Stream stream = File.Open ( $"{AppDomain.CurrentDomain.BaseDirectory}\\{_ownTitle}.config.json", FileMode.Create ) )
				serializer.WriteObject ( stream, Options );
		}
	}
}
