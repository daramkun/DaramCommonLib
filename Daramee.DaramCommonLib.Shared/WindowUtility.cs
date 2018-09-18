using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Daramee.DaramCommonLib
{
    public static class WindowUtility
    {
		const int GWL_EXSTYLE = -20;
		const int WS_EX_DLGMODALFRAME = 0x0001;
		const int SWP_NOSIZE = 0x0001;
		const int SWP_NOMOVE = 0x0002;
		const int SWP_NOZORDER = 0x0004;
		const int SWP_FRAMECHANGED = 0x0020;
		const uint WM_SETICON = 0x0080;

		[DllImport ( "user32.dll" )]
		static extern int GetWindowLong ( IntPtr hwnd, int index );
		[DllImport ( "user32.dll" )]
		static extern int SetWindowLong ( IntPtr hwnd, int index, int newStyle );
		[DllImport ( "user32.dll" )]
		static extern bool SetWindowPos ( IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags );
		[DllImport ( "user32.dll" )]
		static extern IntPtr SendMessage ( IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam );

		public static void RemoveWindowIcon ( Window window )
		{
			IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper ( window ).Handle;
			SendMessage ( hWnd, WM_SETICON, new IntPtr ( 1 ), IntPtr.Zero );
			SendMessage ( hWnd, WM_SETICON, IntPtr.Zero, IntPtr.Zero );

			int extendedStyle = GetWindowLong ( hWnd, GWL_EXSTYLE );
			SetWindowLong ( hWnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME );
			// Update the window's non-client area to reflect the changes
			SetWindowPos ( hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED );
		}
	}
}
