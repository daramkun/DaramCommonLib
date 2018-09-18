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
				case '%': return '％';
				case '.': return '．';
				case '\a':
				case '\b':
				case '\t':
				case '\n':
				case '\v':
				case '\f':
				case '\r':
				case '\0':
				case '\u0001':
				case '\u0002':
				case '\u0003':
				case '\u0004':
				case '\u0005':
				case '\u0006':
				case '\u000e':
				case '\u000f':
				case '\u0010':
				case '\u0011':
				case '\u0012':
				case '\u0013':
				case '\u0014':
				case '\u0015':
				case '\u0016':
				case '\u0017':
				case '\u0018':
				case '\u0019':
				case '\u001a':
				case '\u001b':
				case '\u001c':
				case '\u001d':
				case '\u001e':
				case '\u001f':
					return ' ';
				default: return ch;
			}
		}

		public static string ReplaceInvalidPathCharacters ( string path )
		{
			foreach ( var ch in Path.GetInvalidPathChars () )
			{
				if ( path.IndexOf ( ch ) >= 0 )
					path = path.Replace ( ch, GetInvalidToValid ( ch ) );
			}
			return path;
		}

		public static string ReplaceInvalidFilenameCharacters ( string filename )
		{
			foreach ( var ch in Path.GetInvalidFileNameChars () )
			{
				if ( filename.IndexOf ( ch ) >= 0 )
					filename = filename.Replace ( ch, GetInvalidToValid ( ch ) );
			}
			return filename;
		}
	}
}
