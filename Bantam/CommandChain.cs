using System;
using System.Collections.Generic;

namespace Bantam
{
	public class CommandChain
	{
		internal struct CommandTemplate
		{
			public Type commandType;
			public Action<Command, Event> initializer;
		}

		internal List<CommandTemplate> Commands = new List<CommandTemplate>();

		public CommandChain Do<U>(Action<Command, Event> initializer = null) where U : Command
		{
			Commands.Add(new CommandTemplate {
				commandType = typeof(U),
				initializer = initializer
			});
			return this;
		}
	}
}
