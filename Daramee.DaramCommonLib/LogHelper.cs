using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Daramee.DaramCommonLib
{
	public interface ILogWriter : IDisposable
	{
		void WriteLog ( string message );
	}

	[Flags]
	public enum MessageFormat
	{
		Message = 0,

		LogLevel = 1 << 0,
		Thread = 1 << 1,
		DateTime = 1 << 2,
		CultureName = 1 << 3,
	}

	public enum LogLevel
	{
		None,

		Level1,
		Level2,
		Level3,
		Level4,
		Level5,
	}

	public sealed class DebugLogWriter : ILogWriter
	{
		public void WriteLog ( string message ) { Debug.WriteLine ( message ); }
		public void Dispose () { }
	}
	
	public sealed class StreamLogWriter : ILogWriter
	{
		private bool leaveOpen;

		public Stream BaseStream { get; private set; }
		public Encoding StringEncoding { get; set; }

		public StreamLogWriter ( Stream stream, bool leaveOpen = false )
		{
			BaseStream = stream ?? throw new ArgumentNullException ( "stream" );
			StringEncoding = Encoding.UTF8;
			this.leaveOpen = leaveOpen;
		}

		public void WriteLog ( string message )
		{
			if ( BaseStream != null )
			{
				byte [] messageData = StringEncoding.GetBytes ( message + Environment.NewLine );
				BaseStream.Write ( messageData, 0, messageData.Length );
			}
		}

		public void Dispose ()
		{
			if ( !leaveOpen )
				BaseStream.Dispose ();
		}
	}

	public static class LogHelper
	{
		public static MessageFormat MessageFormat { get; set; }
		public static IEnumerable<ILogWriter> LogWriters { get; private set; }

		public static bool IsParallelLoggingMode { get; set; }

		public static LogLevel LogLevel { get; set; }

		static LogHelper ()
		{
			LogWriters = new List<ILogWriter> ();
			LogLevel = LogLevel.Level5;
		}

		public static void AddDefaultLogWriter ()
		{
			AddLogWriter ( new DebugLogWriter () );
		}

		public static void AddLogWriter ( ILogWriter logWriter )
		{
			( LogWriters as List<ILogWriter> ).Add ( logWriter );
		}

		private static bool HasFlag ( MessageFormat messageFormat )
		{
			return ( ( MessageFormat & messageFormat ) != 0 );
		}

		public static void Write ( LogLevel level, string message, params object [] args )
		{
			if ( level == LogLevel.None ) throw new ArgumentException ();
			if ( level > LogLevel ) return;

			StringBuilder builder = new StringBuilder ();

			if ( HasFlag ( MessageFormat.LogLevel ) )
				builder.Append ( String.Format ( "[{0}]", level ) );
			if ( HasFlag ( MessageFormat.Thread ) )
				builder.Append ( String.Format ( "[{0}]", Thread.CurrentThread.ManagedThreadId ) );
			if ( HasFlag ( MessageFormat.CultureName ) )
				builder.Append ( String.Format ( "[{0}]", CultureInfo.CurrentUICulture.Name ) );
			if ( HasFlag ( MessageFormat.DateTime ) )
				builder.Append ( String.Format ( "[{0}]", DateTime.UtcNow.ToString ( CultureInfo.CurrentUICulture.DateTimeFormat ) ) );
			builder.Append ( String.Format ( message, args ) );

			string tempString = builder.ToString ();
			Logging ( tempString );
		}

		private static void Logging ( object tempString )
		{
			foreach ( ILogWriter logWriter in LogWriters )
				logWriter.WriteLog ( tempString as string );
		}
	}
}
