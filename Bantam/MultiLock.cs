using System.Collections.Generic;

namespace Bantam
{
	public class MultiLock : Poolable
	{
		public bool IsLocked { get { return keys.Count > 0; } }
		private readonly Dictionary<object, bool> keys = new Dictionary<object, bool>(); 

		public void Lock(object key)
		{
			keys[key] = true;
		}

		public void Unlock(object key)
		{
			keys.Remove(key);
		}

		public void Reset()
		{
			keys.Clear();
		}
	}
}
