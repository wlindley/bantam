using System;

namespace Bantam
{
	internal interface CommandTemplate
	{
		Command AllocateCommand(ObjectPool pool, Event ev);
		void FreeCommand(ObjectPool pool, Command command);
	}

	internal class CommandTemplate<T, U> : CommandTemplate where T : class, Event where U : Command, new()
	{
		private Action<U, T> initializer;

		public CommandTemplate(Action<U, T> initializer = null)
		{
			this.initializer = initializer;
		}

		public Command AllocateCommand(ObjectPool pool, Event ev)
		{
			var command = pool.Allocate<U>();
			if (null != initializer)
				initializer(command as U, ev as T);
			return command;
		}

		public void FreeCommand(ObjectPool pool, Command command)
		{
			pool.Free<U>(command as U);
		}
	}
}
