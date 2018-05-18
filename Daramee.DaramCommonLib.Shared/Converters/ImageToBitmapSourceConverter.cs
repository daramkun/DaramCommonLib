using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daramee.DaramCommonLib.Converters
{
	public sealed class ImageToBitmapSourceConverter : IValueConverter
	{
		static Dictionary<Bitmap, WriteableBitmap> cachedBitmaps = new Dictionary<Bitmap, WriteableBitmap> ();

		[DllImport ( "kernel32.dll" )]
		static extern void CopyMemory ( IntPtr destination, IntPtr source, int length );

		public object Convert ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			var image = value as Bitmap;

			if ( !cachedBitmaps.ContainsKey ( image ) )
			{
				BitmapPalette palette = null;
				if ( image.Palette != null )
				{
					List<System.Windows.Media.Color> colorList = new List<System.Windows.Media.Color> ();
					foreach ( var color in image.Palette.Entries )
						colorList.Add ( System.Windows.Media.Color.FromArgb ( color.A, color.R, color.G, color.B ) );
					palette = new BitmapPalette ( colorList );
				}
				cachedBitmaps.Add ( image,
					new WriteableBitmap ( image.Width, image.Height, image.HorizontalResolution, image.VerticalResolution, ConvertPixelFormat ( image.PixelFormat ), palette ) );
			}
			var cachedBitmap = cachedBitmaps [ image ];

			BitmapData data = image.LockBits ( new Rectangle ( 0, 0, image.Width, image.Height ), ImageLockMode.ReadOnly, image.PixelFormat );
			cachedBitmap.Lock ();
			CopyMemory ( cachedBitmap.BackBuffer, data.Scan0, cachedBitmap.BackBufferStride * data.Height );
			cachedBitmap.AddDirtyRect ( new Int32Rect ( 0, 0, image.Width, image.Height ) );
			cachedBitmap.Unlock ();
			image.UnlockBits ( data );

			return cachedBitmap;
		}

		private System.Windows.Media.PixelFormat ConvertPixelFormat ( System.Drawing.Imaging.PixelFormat pixelFormat )
		{
			switch ( pixelFormat )
			{
				case System.Drawing.Imaging.PixelFormat.Format32bppArgb: return PixelFormats.Bgra32;
				case System.Drawing.Imaging.PixelFormat.Format24bppRgb: return PixelFormats.Bgr24;
				case System.Drawing.Imaging.PixelFormat.Format8bppIndexed: return PixelFormats.Indexed8;
				default: return PixelFormats.Default;
			}
		}

		public object ConvertBack ( object value, Type targetType, object parameter, CultureInfo culture )
		{
			foreach ( var kv in cachedBitmaps )
			{
				if ( kv.Value == value )
					return kv.Key;
			}

			Bitmap bitmap;
			using ( MemoryStream outStream = new MemoryStream () )
			{
				BitmapEncoder enc = new BmpBitmapEncoder ();

				enc.Frames.Add ( BitmapFrame.Create ( value as BitmapSource ) );
				enc.Save ( outStream );
				outStream.Position = 0;

				bitmap = new Bitmap ( outStream );
			}
			return bitmap;
		}
	}
}
