using System;
using System.Collections.Generic;

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

	public abstract class CommandChain
	{
		internal List<CommandTemplate> Commands = new List<CommandTemplate>();
	}

	public class CommandChain<T> : CommandChain where T : class, Event
	{
		public CommandChain<T> Do<U>(Action<U, T> initializer = null) where U : Command, new()
		{
			Commands.Add(new CommandTemplate<T, U> {
				commandType = typeof(U),
				initializer = initializer
			});
			return this;
		}
	}
}
