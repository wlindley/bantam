using System;

namespace Bantam
{
	public abstract class Command : Poolable
	{
		private CommandChainExecutor executor;
		private bool autoSucceed = true;

		public abstract void Execute();

		virtual public void Reset()
		{
			executor = null;
			autoSucceed = true;
		}

		internal void Start(CommandChainExecutor executor)
		{
			this.executor = executor;
			Execute();
			if (autoSucceed)
				Done();
		}

		protected void Done()
		{
			autoSucceed = false;
			executor.CurrentCommandComplete();
		}

		protected void Fail()
		{
			autoSucceed = false;
			executor.CurrentCommandFailed();
		}

		protected void Retain()
		{
			autoSucceed = false;
		}
	}
}
