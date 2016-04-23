﻿using System;
using System.Collections.Generic;

namespace Bantam
{
	class CommandChainExecutor : Poolable
	{
		private CommandManager manager;
		private Event triggeringEvent;
		private List<CommandChain.CommandTemplate>.Enumerator enumerator;
		private Command currentCommand;

		public void Reset()
		{
			manager = null;
			triggeringEvent = null;
			enumerator.Dispose();
			currentCommand = null;
		}

		internal void Start(Event triggeringEvent, CommandChain chain, CommandManager manager)
		{
			this.triggeringEvent = triggeringEvent;
			this.manager = manager;
			enumerator = chain.Commands.GetEnumerator();
			enumerator.MoveNext();
			Next();
		}

		internal void Complete()
		{
			currentCommand = null;
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