using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Daramee.DaramCommonLib
{
	[Serializable]
	public class UndoManager<T> where T : class
	{
		[NonSerialized]
		BinaryFormatter bf = new BinaryFormatter ();
		Stack<byte []> undoStack = new Stack<byte []> ();
		Stack<byte []> redoStack = new Stack<byte []> ();

		public event EventHandler UpdateUndo, UpdateRedo;

		public bool IsUndoStackEmpty { get { return undoStack.Count == 0; } }
		public bool IsRedoStackEmpty { get { return redoStack.Count == 0; } }

		public void SaveToUndoStack ( T fileInfoCollection, bool clearRedoStack = true, [CallerMemberName] string caller = "", [CallerLineNumber] int line = 0 )
		{
			using ( MemoryStream memStream = new MemoryStream () )
			{
				bf.Serialize ( memStream, fileInfoCollection ?? throw new ArgumentNullException () );
				undoStack.Push ( memStream.ToArray () );
			}

			if ( clearRedoStack )
				ClearRedoStack ();

			UpdateUndo?.Invoke ( this, EventArgs.Empty );

			Debug.WriteLine ( "[Debug] SaveToUndoStack ()\nCaller name: {0}({1})\nUndo stacks: {2}\nRedo stacks: {3}\n", caller, line, undoStack.Count, redoStack.Count );
		}

		public void SaveToRedoStack ( T fileInfoCollection, [CallerMemberName] string caller = "", [CallerLineNumber] int line = 0 )
		{
			using ( MemoryStream memStream = new MemoryStream () )
			{
				bf.Serialize ( memStream, fileInfoCollection ?? throw new ArgumentNullException () );
				redoStack.Push ( memStream.ToArray () );
			}

			UpdateRedo?.Invoke ( this, EventArgs.Empty );

			Debug.WriteLine ( "[Debug] SaveToRedoStack ()\nCaller name: {0}({1})\nUndo stacks: {2}\nRedo stacks: {3}\n", caller, line, undoStack.Count, redoStack.Count );
		}

		public T LoadFromUndoStack ()
		{
			if ( IsUndoStackEmpty ) return null;
			using ( MemoryStream memStream = new MemoryStream ( undoStack.Pop () ) )
			{
				var ret = bf.Deserialize ( memStream ) as T ?? throw new InvalidCastException ();
				UpdateUndo?.Invoke ( this, EventArgs.Empty );
				return ret;
			}
		}

		public T LoadFromRedoStack ()
		{
			if ( IsRedoStackEmpty ) return null;
			using ( MemoryStream memStream = new MemoryStream ( redoStack.Pop () ) )
			{
				var ret = bf.Deserialize ( memStream ) as T ?? throw new InvalidCastException ();
				UpdateRedo?.Invoke ( this, EventArgs.Empty );
				return ret;
			}
		}

		/*public void Backup ()
		{
			using ( Stream backupFile = new FileStream ( "crashed_backup.dat", FileMode.Create, FileAccess.Write ) )
			{
				bf.Serialize ( backupFile, this );
			}
		}

		public static UndoManager<T> Restore ()
		{
			if ( !File.Exists ( "crashed_backup.dat" ) )
				return null;

			BinaryFormatter bf = new BinaryFormatter ();
			UndoManager<T> ret;
			try
			{
				using ( Stream backupFile = new FileStream ( "crashed_backup.dat", FileMode.Open, FileAccess.Read ) )
				{
					ret = bf.Deserialize ( backupFile ) as UndoManager<T>;
				}
				ret.bf = new BinaryFormatter ();
			}
			catch { ret = null; }
			File.Delete ( "crashed_backup.dat" );
			return ret;
		}*/

		public void ClearUndoStack () { undoStack.Clear (); }
		public void ClearRedoStack () { redoStack.Clear (); }
		public void ClearAll () { ClearUndoStack (); ClearRedoStack (); }
	}
}
