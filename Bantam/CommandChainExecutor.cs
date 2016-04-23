using System;
using System.Collections.Generic;

namespace Bantam
{
	class CommandChainExecutor
	{
		private CommandManager manager;
		private List<CommandChain.CommandTemplate>.Enumerator enumerator;
		private Command currentCommand;
		private Event triggeringEvent;

		public CommandChainExecutor(CommandChain chain, CommandManager manager)
		{
			this.manager = manager;
			enumerator = chain.Commands.GetEnumerator();
			enumerator.MoveNext();
		}

		internal void Start(Event ev)
		{
			triggeringEvent = ev;
			Next();
		}

		internal void Complete()
		{
			if (enumerator.MoveNext())
				Next();
			else
				manager.CompleteChainExecution(this);
		}

		private void Next()
		{
			var template = enumerator.Current;
			currentCommand = Activator.CreateInstance(template.commandType) as Command;
			if (null != template.initializer)
				template.initializer(currentCommand, triggeringEvent);
			currentCommand.Start(this);
		}
	}
}
