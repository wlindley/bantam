using System;

namespace Bantam
{
	public abstract class Command : Poolable
	{
		private CommandChainExecutor executor;

		public abstract void Execute();

		virtual public void Reset()
		{
			executor = null;
		}

		internal void Start(CommandChainExecutor executor)
		{
			this.executor = executor;
			Execute();
		}

		protected void Done()
		{
			executor.Complete();
		}
	}
}
