using System;

namespace Bantam
{
	internal interface CommandAllocator
	{
		Command AllocateCommand(ObjectPool pool, Event ev);
		void FreeCommand(ObjectPool pool, Command command);
	}

	internal class CommandAllocator<T, U> : CommandAllocator where T : class, Event where U : Command, new()
	{
		private CommandInitializer<U, T> initializer;

		public CommandAllocator(CommandInitializer<U, T> initializer = null)
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
