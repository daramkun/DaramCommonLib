using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Daramee.DaramCommonLib
{
	public static class ImageSourceHelper
	{
		public static BitmapSource GetClipboardImage ( bool containsAlpha = false )
		{
			BitmapSource bitmapSource = null;

			if ( Clipboard.ContainsImage () )
				bitmapSource = Clipboard.GetImage ();
			else if ( Clipboard.ContainsFileDropList () )
			{
				var fileDropList = Clipboard.GetFileDropList ();
				if ( fileDropList.Count <= 0 )
					return null;
				bitmapSource = GetImageFromFile ( fileDropList [ 0 ] );
			}
			else if ( Clipboard.ContainsText () )
			{
				string text = Clipboard.GetText ();
				if ( !File.Exists ( text ) )
					return null;
				bitmapSource = GetImageFromFile ( text );
			}
			else
				return null;

			if ( bitmapSource == null )
				return null;

			bitmapSource = new FormatConvertedBitmap ( bitmapSource,
				containsAlpha
				? System.Windows.Media.PixelFormats.Bgra32
				: System.Windows.Media.PixelFormats.Bgr24, null, 0 );
			bitmapSource.Freeze ();

			return bitmapSource;
		}

		public static BitmapSource GetImageFromFile ( string filename )
		{
			var bitmapSource = new BitmapImage ();
			( bitmapSource as BitmapImage ).BeginInit ();
			( bitmapSource as BitmapImage ).UriSource = new Uri ( filename );
			( bitmapSource as BitmapImage ).EndInit ();
			bitmapSource.Freeze ();
			return bitmapSource;
		}
	}
}
