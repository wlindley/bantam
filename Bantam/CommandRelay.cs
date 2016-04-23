using System;
using System.Collections.Generic;

namespace Bantam
{
	public class CommandRelay
	{
		private EventBus eventBus;
		private ObjectPool pool;
		private Dictionary<Type, List<CommandChain>> chains = new Dictionary<Type, List<CommandChain>>();
		private List<CommandChainExecutor> activeExecutors = new List<CommandChainExecutor>();

		public CommandRelay(EventBus eventBus, ObjectPool pool)
		{
			this.eventBus = eventBus;
			this.pool = pool;
		}

		public CommandChain<T> On<T>() where T : class, Event, new()
		{
			EnsureKeyExists<T>();
			var chain = new CommandChain<T>();
			chains[typeof(T)].Add(chain);
			eventBus.AddListener<T>(ev =>
				{
					var executor = pool.Allocate<CommandChainExecutor>();
					activeExecutors.Add(executor);
					executor.Start(ev, chain, this, pool);
				});
			return chain;
		}

		internal void CompleteChainExecution(CommandChainExecutor executor)
		{
			activeExecutors.Remove(executor);
			pool.Free<CommandChainExecutor>(executor);
		}

		private void EnsureKeyExists<T>() where T : Event
		{
			var type = typeof(T);
			if (!chains.ContainsKey(type))
				chains[type] = new List<CommandChain>();
		}
	}
}
