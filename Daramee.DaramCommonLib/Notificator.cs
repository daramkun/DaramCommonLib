﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Daramee.DaramCommonLib
{
	public enum NotifyType
	{
		Message,
		Information,
		Warning,
		Error,

		CustomType1,
		CustomType2,
		CustomType3,
		CustomType4,
		CustomType5,
		CustomType6,
		CustomType7,
		CustomType8,
	}

	public struct NotificatorInitializer
	{
		public string Title;
		public Icon Icon;

		public DateTimeOffset? ExpirationTime;
		
		public string InformationTypeImagePath;
		public string WarningTypeImagePath;
		public string ErrorTypeImagePath;
		public string CustomTypeImagePath1;
		public string CustomTypeImagePath2;
		public string CustomTypeImagePath3;
		public string CustomTypeImagePath4;
		public string CustomTypeImagePath5;
		public string CustomTypeImagePath6;
		public string CustomTypeImagePath7;
		public string CustomTypeImagePath8;
	}

	public interface INotificator : IDisposable
	{
		event EventHandler Clicked;

		bool IsEnabledNotification { get; set; }
		void Notify ( string title, string text, NotifyType type );
	}

	public sealed class LegacyNotificator : INotificator
	{
		NotifyIcon notifyIcon;
		NotificatorInitializer initializer;

		public event EventHandler Clicked;

		public bool IsEnabledNotification { get; set; } = true;

		public LegacyNotificator ( NotificatorInitializer initializer )
		{
			this.initializer = initializer;
			notifyIcon = new NotifyIcon ()
			{
				Text = initializer.Title,
				Visible = true,

				Icon = initializer.Icon,
			};
			notifyIcon.BalloonTipClicked += ( sender, e ) => { Clicked?.Invoke ( this, EventArgs.Empty ); };
		}

		public void Dispose ()
		{
			notifyIcon.Dispose ();
		}

		public void Notify ( string title, string text, NotifyType type )
		{
			if ( !IsEnabledNotification ) return;
			int time = ( int ) initializer.ExpirationTime?.Offset.TotalSeconds;
			if ( time <= 0 )
				time = 10;
			notifyIcon.ShowBalloonTip ( time, title, text, ConvertIcon ( type ) );
		}

		private ToolTipIcon ConvertIcon ( NotifyType type )
		{
			switch ( type )
			{
				case NotifyType.Information: return ToolTipIcon.Info;
				case NotifyType.Warning: return ToolTipIcon.Warning;
				case NotifyType.Error: return ToolTipIcon.Error;
				default: return ToolTipIcon.None;
			}
		}
	}

	public sealed class Win8Notificator : INotificator
	{
		NotificatorInitializer initializer;

		public bool IsEnabledNotification { get; set; } = true;

		public event EventHandler Clicked;

		public Win8Notificator ( NotificatorInitializer initializer )
		{
			this.initializer = initializer;
		}

		public void Dispose ()
		{

		}

		public void Notify ( string title, string text, NotifyType type )
		{
			if ( !IsEnabledNotification ) return;

			XmlDocument toastXml = ToastNotificationManager.GetTemplateContent ( type != NotifyType.Message ? ToastTemplateType.ToastImageAndText04 : ToastTemplateType.ToastText04 );

			XmlNodeList stringElements = toastXml.GetElementsByTagName ( "text" );
			stringElements [ 0 ].AppendChild ( toastXml.CreateTextNode ( title ) );
			stringElements [ 1 ].AppendChild ( toastXml.CreateTextNode ( text ) );

			if ( type != NotifyType.Message )
			{
				XmlNodeList imageElements = toastXml.GetElementsByTagName ( "image" );
				imageElements [ 0 ].Attributes.GetNamedItem ( "src" ).NodeValue = GetIconPath ( type );
			}

			ToastNotification toast = new ToastNotification ( toastXml )
			{
				ExpirationTime = initializer.ExpirationTime,
			};
			toast.Activated += ( sender, e ) => { Clicked?.Invoke ( this, EventArgs.Empty ); };

			ToastNotificationManager.CreateToastNotifier ( initializer.Title ).Show ( toast );
		}

		private string GetIconPath ( NotifyType type )
		{
			switch ( type )
			{
				case NotifyType.Warning: return new Uri ( Path.GetFullPath ( initializer.WarningTypeImagePath ) ).AbsoluteUri;
				case NotifyType.Information: return new Uri ( Path.GetFullPath ( initializer.InformationTypeImagePath ) ).AbsoluteUri;
				case NotifyType.Error: return new Uri ( Path.GetFullPath ( initializer.ErrorTypeImagePath ) ).AbsoluteUri;
				case NotifyType.CustomType1: return new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath1 ) ).AbsoluteUri;
				case NotifyType.CustomType2: return new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath2 ) ).AbsoluteUri;
				case NotifyType.CustomType3: return new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath3 ) ).AbsoluteUri;
				case NotifyType.CustomType4: return new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath4 ) ).AbsoluteUri;
				case NotifyType.CustomType5: return new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath5 ) ).AbsoluteUri;
				case NotifyType.CustomType6: return new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath6 ) ).AbsoluteUri;
				case NotifyType.CustomType7: return new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath7 ) ).AbsoluteUri;
				case NotifyType.CustomType8: return new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath8 ) ).AbsoluteUri;
				default: return null;
			}
		}
	}

	public class NotificatorManager
	{
		public static INotificator Notificator { get; private set; }

		public static void Initialize ( NotificatorInitializer initializer )
		{
			if ( Environment.OSVersion.Version.Major >= 8 && ( Environment.OSVersion.Version.Major == 8 && Environment.OSVersion.Version.Minor == 1 ) )
				Notificator = new Win8Notificator ( initializer );
			else
				Notificator = new LegacyNotificator ( initializer );
		}

		public static void Uninitialize ()
		{
			Notificator.Dispose ();
		}

		public static void Notify ( string title, string text, NotifyType type )
		{
			Notificator.Notify ( title, text, type );
		}
	}
}
