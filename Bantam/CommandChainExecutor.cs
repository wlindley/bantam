using System;
using System.Collections.Generic;

namespace Bantam
{
	class CommandChainExecutor : Poolable
	{
		private CommandManager manager;
		private Event triggeringEvent;
		private List<CommandChain.CommandTemplate>.Enumerator enumerator;
		private ObjectPool pool;
		private Command currentCommand;

		public void Reset()
		{
			manager = null;
			triggeringEvent = null;
			currentCommand = null;
			pool = null;
			enumerator.Dispose();
		}

		internal void Start(Event triggeringEvent, CommandChain chain, CommandManager manager, ObjectPool pool)
		{
			this.triggeringEvent = triggeringEvent;
			this.manager = manager;
			this.pool = pool;
			enumerator = chain.Commands.GetEnumerator();
			enumerator.MoveNext();
			Next();
		}

		internal void Complete()
		{
			pool.Release(enumerator.Current.commandType, currentCommand);
			currentCommand = null;
			if (enumerator.MoveNext())
				Next();
			else
				manager.CompleteChainExecution(this);
		}

		private void Next()
		{
			var template = enumerator.Current;
			currentCommand = pool.Allocate(template.commandType) as Command;
			if (null != template.initializer)
				template.initializer(currentCommand, triggeringEvent);
			currentCommand.Start(this);
		}
	}
}
