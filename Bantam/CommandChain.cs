using System;
using System.Collections.Generic;

namespace Bantam
{
	public delegate void CommandInitializer<U, T>(U cmd, T evt) where U : Command where T : Event;
	public delegate void SimpleCommandInitializer<U>(U cmd) where U : Command;

	public abstract class CommandChain
	{
		internal List<CommandAllocator> Commands = new List<CommandAllocator>();
		internal CommandAllocator FailureCommand;
	}

	public class CommandChain<T> : CommandChain where T : class, Event
	{
		public CommandChain<T> Do<U>(CommandInitializer<U, T> initializer = null) where U : Command, new()
		{
			Commands.Add(new CommandAllocator<T, U>(initializer));
			return this;
		}

		public void OnFailure<U>(CommandInitializer<U, T> initializer = null) where U : Command, new()
		{
			FailureCommand = new CommandAllocator<T, U>(initializer);
		}
	}
}
