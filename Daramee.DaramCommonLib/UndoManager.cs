using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramee.DaramCommonLib
{
	public class UndoManager<T> where T : class, ICloneable
	{
		Stack<T> undoStack = new Stack<T> ();
		Stack<T> redoStack = new Stack<T> ();

		public bool IsUndoStackEmpty { get { return undoStack.Count == 0; } }
		public bool IsRedoStackEmpty { get { return redoStack.Count == 0; } }

		public void SaveToUndoStack ( T obj )
		{
			undoStack.Push ( obj.Clone () as T );
			ClearRedoStack ();
		}

		public void SaveToRedoStack ( T obj )
		{
			redoStack.Push ( obj.Clone () as T );
		}

		public T LoadFromUndoStack ()
		{
			if ( IsUndoStackEmpty ) return null;
			return undoStack.Pop ();
		}

		public T LoadFromRedoStack ()
		{
			if ( IsRedoStackEmpty ) return null;
			return redoStack.Pop ();
		}

		public void ClearUndoStack () { undoStack.Clear (); }
		public void ClearRedoStack () { redoStack.Clear (); }
		public void ClearAll () { ClearUndoStack (); ClearRedoStack (); }
	}
}
