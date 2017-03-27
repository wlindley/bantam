using System;
using System.Collections.Generic;

namespace Bantam
{
	internal interface CommandChainExecutor : Poolable
	{
		void CurrentCommandComplete();
		void CurrentCommandFailed();
	}

	internal class SimpleCommandChainExecutor<T> : CommandChainExecutor where T : Command, new()
	{
		private CommandRelay manager;
		private ObjectPool pool;
		private T command;

		public void Reset()
		{
			manager = null;
			pool = null;
			command = null;
		}

		internal void Start(CommandRelay manager, ObjectPool pool, SimpleCommandInitializer<T> initializer = null)
		{
			this.manager = manager;
			this.pool = pool;
			command = pool.Allocate<T>();
			if (null != initializer)
				initializer(command);
			command.Start(this);
		}

		public void CurrentCommandComplete()
		{
			pool.Free<T>(command);
			manager.CompleteChainExecution<SimpleCommandChainExecutor<T>>(this);
		}

		public void CurrentCommandFailed()
		{
			pool.Free<T>(command);
			manager.CompleteChainExecution<SimpleCommandChainExecutor<T>>(this);
		}
	}

	internal class EventCommandChainExecutor : CommandChainExecutor
	{
		private CommandRelay manager;
		private Event triggeringEvent;
		private List<CommandAllocator>.Enumerator enumerator;
		private CommandAllocator failureAllocator;
		private CommandAllocator currentAllocator;
		private ObjectPool pool;
		private Command currentCommand;

		public void Reset()
		{
			manager = null;
			triggeringEvent = null;
			currentCommand = null;
			pool = null;
			enumerator.Dispose();
			failureAllocator = null;
			currentAllocator = null;
		}

		internal void Start(Event triggeringEvent, CommandChain chain, CommandRelay manager, ObjectPool pool)
		{
			this.triggeringEvent = triggeringEvent;
			this.manager = manager;
			this.pool = pool;
			failureAllocator = chain.FailureCommand;
			enumerator = chain.Commands.GetEnumerator();
			pool.Lock(triggeringEvent, this);
			enumerator.MoveNext();
			Next();
		}

		public void CurrentCommandComplete()
		{
			currentAllocator.FreeCommand(pool, currentCommand);
			currentCommand = null;
			if (enumerator.MoveNext())
				Next();
			else
			{
				pool.Unlock(triggeringEvent.GetType(), triggeringEvent, this);
				manager.CompleteChainExecution<EventCommandChainExecutor>(this);
			}
		}

		public void CurrentCommandFailed()
		{
			currentAllocator.FreeCommand(pool, currentCommand);
			currentCommand = null;
			if (null != failureAllocator)
				ExecuteFailureCommand();
			else
				manager.CompleteChainExecution<EventCommandChainExecutor>(this);
		}

		private void Next()
		{
			currentAllocator = enumerator.Current;
			StartCommandFromCurrentAllocator();
		}

		private void ExecuteFailureCommand()
		{
			currentAllocator = failureAllocator;
			failureAllocator = null;
			StartCommandFromCurrentAllocator();
		}

		void StartCommandFromCurrentAllocator()
		{
			currentCommand = currentAllocator.AllocateCommand(pool, triggeringEvent);
			currentCommand.Start(this);
		}
	}
}
