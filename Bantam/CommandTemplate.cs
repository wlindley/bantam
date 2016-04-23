using System;

namespace Bantam
{
	internal abstract class CommandTemplate
	{
		public Type commandType;

		public abstract Command AllocateCommand(ObjectPool pool, Event ev);
		public abstract void FreeCommand(ObjectPool pool, Command command);
	}

	internal class CommandTemplate<T, U> : CommandTemplate where T : class, Event where U : Command, new()
	{
		public Action<U, T> initializer;

		public override Command AllocateCommand(ObjectPool pool, Event ev)
		{
			var command = pool.Allocate<U>();
			if (null != initializer)
				initializer(command as U, ev as T);
			return command;
		}

		public override void FreeCommand(ObjectPool pool, Command command)
		{
			pool.Free<U>(command as U);
		}
	}
}
