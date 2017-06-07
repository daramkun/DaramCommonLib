using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramee.DaramCommonLib
{
	public static class FilesHelper
	{

		public static char GetInvalidToValid ( char ch )
		{
			switch ( ch )
			{
				case '?': return '？';
				case '\\': return '＼';
				case '/': return '／';
				case '<': return '〈';
				case '>': return '〉';
				case '*': return '＊';
				case '|': return '｜';
				case ':': return '：';
				case '"': return '＂';
				default: return ch;
			}
		}

		public static string ReplaceInvalidPathCharacters ( string path )
		{
			foreach ( var ch in Path.GetInvalidPathChars () )
			{
				if ( path.IndexOf ( ch ) < 0 )
					path = path.Replace ( ch, GetInvalidToValid ( ch ) );
			}
			return path;
		}
	}
}
