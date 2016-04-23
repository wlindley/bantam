using System;
using System.Collections.Generic;

namespace Bantam
{
	public class CommandManager
	{
		private EventBus eventBus;
		private Dictionary<Type, List<CommandChain>> chains = new Dictionary<Type, List<CommandChain>>();
		private List<CommandChainExecutor> activeExecutors = new List<CommandChainExecutor>();

		public CommandManager(EventBus eventBus)
		{
			this.eventBus = eventBus;
		}

		public CommandChain On<T>() where T : class, Event, new()
		{
			EnsureKeyExists<T>();
			var chain = new CommandChain();
			chains[typeof(T)].Add(chain);
			eventBus.AddListener<T>(ev =>
				{
					var executor = new CommandChainExecutor(chain, this);
					activeExecutors.Add(executor);
					executor.Start(ev);
				});
			return chain;
		}

		internal void CompleteChainExecution(CommandChainExecutor executor)
		{
			activeExecutors.Remove(executor);
		}

		private void EnsureKeyExists<T>() where T : Event
		{
			var type = typeof(T);
			if (!chains.ContainsKey(type))
				chains[type] = new List<CommandChain>();
		}
	}
}
