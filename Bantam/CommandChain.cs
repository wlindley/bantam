using System;
using System.Collections.Generic;

namespace Bantam
{
	internal abstract class CommandTemplate
	{
		public Type commandType;

		public abstract void InitializeCommand(Command command, Event ev);
	}

	internal class CommandTemplate<T> : CommandTemplate where T : class, Event
	{
		public Action<Command, T> initializer;

		public override void InitializeCommand(Command command, Event ev)
		{
			if (null != initializer)
				initializer(command, ev as T);
		}
	}

	public abstract class CommandChain
	{
		internal List<CommandTemplate> Commands = new List<CommandTemplate>();

		public abstract CommandChain Do<U>(Action<Command, Event> initializer = null) where U : Command;
	}

	public class CommandChain<T> : CommandChain where T : class, Event
	{
		public CommandChain<T> Do<U>(Action<Command, T> initializer = null) where U : Command
		{
			Commands.Add(new CommandTemplate<T> {
				commandType = typeof(U),
				initializer = initializer
			});
			return this;
		}

		public override CommandChain Do<U>(Action<Command, Event> initializer)
		{
			throw new NotImplementedException();
		}
	}
}
