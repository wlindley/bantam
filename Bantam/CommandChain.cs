using System;
using System.Collections.Generic;

namespace Bantam
{
	public class CommandChain
	{
		internal struct CommandTemplate
		{
			public Type commandType;
			public Action<Command> initializer;
		}

		internal List<CommandTemplate> Commands = new List<CommandTemplate>();

		public CommandChain Do<U>() where U : Command
		{
			Commands.Add(new CommandTemplate {
				commandType = typeof(U)
			});
			return this;
		}
	}
}
