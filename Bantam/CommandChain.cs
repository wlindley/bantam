using System;
using System.Collections.Generic;

namespace Bantam
{
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
