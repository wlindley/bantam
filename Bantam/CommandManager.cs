using System;
using System.Collections.Generic;

namespace Bantam
{
	public class CommandManager
	{
		private EventBus eventBus;
		private ObjectPool pool;
		private Dictionary<Type, List<CommandChain>> chains = new Dictionary<Type, List<CommandChain>>();
		private List<CommandChainExecutor> activeExecutors = new List<CommandChainExecutor>();

		public CommandManager(EventBus eventBus, ObjectPool pool)
		{
			this.eventBus = eventBus;
			this.pool = pool;
		}

		public CommandChain On<T>() where T : class, Event, new()
		{
			EnsureKeyExists<T>();
			var chain = new CommandChain();
			chains[typeof(T)].Add(chain);
			eventBus.AddListener<T>(ev =>
				{
					var executor = pool.Allocate<CommandChainExecutor>();
					activeExecutors.Add(executor);
					executor.Start(ev, chain, this);
				});
			return chain;
		}

		internal void CompleteChainExecution(CommandChainExecutor executor)
		{
			activeExecutors.Remove(executor);
			pool.Release<CommandChainExecutor>(executor);
		}

		private void EnsureKeyExists<T>() where T : Event
		{
			var type = typeof(T);
			if (!chains.ContainsKey(type))
				chains[type] = new List<CommandChain>();
		}
	}
}
