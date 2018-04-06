using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		public bool IsUndoStackEmpty { get { return undoStack.Count == 0; } }
		public bool IsRedoStackEmpty { get { return redoStack.Count == 0; } }

		public void SaveToUndoStack ( T fileInfoCollection )
		{
			using ( MemoryStream memStream = new MemoryStream () )
			{
				bf.Serialize ( memStream, fileInfoCollection );
				undoStack.Push ( memStream.ToArray () );
			}
			ClearRedoStack ();
		}

		public void SaveToRedoStack ( T fileInfoCollection )
		{
			using ( MemoryStream memStream = new MemoryStream () )
			{
				bf.Serialize ( memStream, fileInfoCollection );
				redoStack.Push ( memStream.ToArray () );
			}
		}

		public T LoadFromUndoStack ()
		{
			if ( IsUndoStackEmpty ) return null;
			using ( MemoryStream memStream = new MemoryStream ( undoStack.Pop () ) )
				return bf.Deserialize ( memStream ) as T;
		}

		public T LoadFromRedoStack ()
		{
			if ( IsRedoStackEmpty ) return null;
			using ( MemoryStream memStream = new MemoryStream ( redoStack.Pop () ) )
				return bf.Deserialize ( memStream ) as T;
		}

		public void Backup ()
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
			using ( Stream backupFile = new FileStream ( "crashed_backup.dat", FileMode.Open, FileAccess.Read ) )
			{
				ret = bf.Deserialize ( backupFile ) as UndoManager<T>;
			}

			File.Delete ( "crashed_backup.dat" );
			return ret;
		}

		public void ClearUndoStack () { undoStack.Clear (); }
		public void ClearRedoStack () { redoStack.Clear (); }
		public void ClearAll () { ClearUndoStack (); ClearRedoStack (); }
	}
}
