using System;
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
			int time = 10;
			if ( initializer.ExpirationTime != null )
				time = ( int ) initializer.ExpirationTime?.Offset.TotalSeconds; ;
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
		ToastNotifier notifier;
		NotificatorInitializer initializer;

		public bool IsEnabledNotification { get; set; } = true;

		public event EventHandler Clicked;

		public Win8Notificator ( NotificatorInitializer initializer )
		{
			this.initializer = initializer;
			notifier = ToastNotificationManager.CreateToastNotifier ( initializer.Title );
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

			notifier.Show ( toast );
		}

		private string GetIconPath ( NotifyType type )
		{
			switch ( type )
			{
				case NotifyType.Warning:
					return initializer.WarningTypeImagePath != null ? new Uri ( Path.GetFullPath ( initializer.WarningTypeImagePath ) ).AbsoluteUri : null;
				case NotifyType.Information:
					return initializer.InformationTypeImagePath != null ? new Uri ( Path.GetFullPath ( initializer.InformationTypeImagePath ) ).AbsoluteUri : null;
				case NotifyType.Error:
					return initializer.ErrorTypeImagePath != null ? new Uri ( Path.GetFullPath ( initializer.ErrorTypeImagePath ) ).AbsoluteUri : null;
				case NotifyType.CustomType1:
					return initializer.CustomTypeImagePath1 != null ? new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath1 ) ).AbsoluteUri : null;
				case NotifyType.CustomType2:
					return initializer.CustomTypeImagePath2 != null ? new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath2 ) ).AbsoluteUri : null;
				case NotifyType.CustomType3:
					return initializer.CustomTypeImagePath3 != null ? new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath3 ) ).AbsoluteUri : null;
				case NotifyType.CustomType4:
					return initializer.CustomTypeImagePath4 != null ? new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath4 ) ).AbsoluteUri : null;
				case NotifyType.CustomType5:
					return initializer.CustomTypeImagePath5 != null ? new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath5 ) ).AbsoluteUri : null;
				case NotifyType.CustomType6:
					return initializer.CustomTypeImagePath6 != null ? new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath6 ) ).AbsoluteUri : null;
				case NotifyType.CustomType7:
					return initializer.CustomTypeImagePath7 != null ? new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath7 ) ).AbsoluteUri : null;
				case NotifyType.CustomType8:
					return initializer.CustomTypeImagePath8 != null ? new Uri ( Path.GetFullPath ( initializer.CustomTypeImagePath8 ) ).AbsoluteUri : null;
				default: return null;
			}
		}
	}

	public class NotificatorManager
	{
		public static INotificator Notificator { get; private set; }

		public static void Initialize ( NotificatorInitializer initializer )
		{
			if ( Environment.OSVersion.Version.Major >= 10 || ( Environment.OSVersion.Version.Major == 8 && Environment.OSVersion.Version.Minor == 1 ) )
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
