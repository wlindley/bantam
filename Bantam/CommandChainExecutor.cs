using System;
using System.Collections.Generic;

namespace Bantam
{
	class CommandChainExecutor
	{
		private CommandManager manager;
		private List<CommandChain.CommandTemplate>.Enumerator enumerator;
		private Command currentCommand;

		public CommandChainExecutor(CommandChain chain, CommandManager manager)
		{
			this.manager = manager;
			enumerator = chain.Commands.GetEnumerator();
			enumerator.MoveNext();
		}

		internal void Start()
		{
			currentCommand = Activator.CreateInstance(enumerator.Current.commandType) as Command;
			currentCommand.Start(this);
		}

		internal void Complete()
		{
			if (enumerator.MoveNext())
				Start();
			else
				manager.CompleteChainExecution(this);
		}
	}
}
