using System;
using System.Collections.Generic;

namespace Bantam
{
	public class CommandManager
	{
		private EventBus eventBus;
		private Dictionary<Type, CommandChain> chains = new Dictionary<Type, CommandChain>();
		private List<CommandChainExecutor> activeExecutors = new List<CommandChainExecutor>();

		public CommandManager(EventBus eventBus)
		{
			this.eventBus = eventBus;
		}

		public CommandChain On<T>() where T : class, Event, new()
		{
			var chain = new CommandChain();
			chains[typeof(T)] = chain;
			eventBus.AddListener<T>(ev =>
				{
					var executor = new CommandChainExecutor(chain, this);
					activeExecutors.Add(executor);
					executor.Start();
				});
			return chain;
		}

		internal void CompleteChainExecution(CommandChainExecutor executor)
		{
			activeExecutors.Remove(executor);
		}
	}
}
