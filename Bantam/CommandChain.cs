using System;
using System.Collections.Generic;

namespace Bantam
{
	public abstract class CommandChain
	{
		internal List<CommandAllocator> Commands = new List<CommandAllocator>();
	}

	public class CommandChain<T> : CommandChain where T : class, Event
	{
		public CommandChain<T> Do<U>(Action<U, T> initializer = null) where U : Command, new()
		{
			Commands.Add(new CommandAllocator<T, U>(initializer));
			return this;
		}
	}
}
