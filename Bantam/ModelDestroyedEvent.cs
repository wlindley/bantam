using System;

namespace Bantam
{
	public class ModelDestroyedEvent : Event
	{
		public Model model;
		public Type type;

		public void Reset()
		{
			model = null;
			type = null;
		}
	}
}
