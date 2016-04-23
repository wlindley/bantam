using System;
using System.Collections.Generic;

namespace Bantam
{
	internal class CommandChainExecutor : Poolable
	{
		private CommandRelay manager;
		private Event triggeringEvent;
		private List<CommandAllocator>.Enumerator enumerator;
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

		internal void Start(Event triggeringEvent, CommandChain chain, CommandRelay manager, ObjectPool pool)
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
			enumerator.Current.FreeCommand(pool, currentCommand);
			currentCommand = null;
			if (enumerator.MoveNext())
				Next();
			else
				manager.CompleteChainExecution(this);
		}

		private void Next()
		{
			currentCommand = enumerator.Current.AllocateCommand(pool, triggeringEvent);
			currentCommand.Start(this);
		}
	}
}
