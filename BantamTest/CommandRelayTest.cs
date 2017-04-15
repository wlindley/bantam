using System.Collections.Generic;
using NUnit.Framework;

namespace Bantam.Test
{
	[TestFixture]
	public class CommandRelayTest
	{
		private CommandRelay testObj;
		private EventBus eventBus;

		[SetUp]
		public void SetUp()
		{
			var pool = new ObjectPool();
			eventBus = new EventBus(pool);
			testObj = new CommandRelay(eventBus, pool);
			DummyCommand.ExecuteCount = 0;
			DummyCommand.LastValue = 0;
			AsyncCommand.triggeringEvents.Clear();
			AsyncCommand.ClearAll();
		}

		[Test]
		public void OnDoCausesCommandToBeExecuted()
		{
			testObj.On<DummyEvent>().Do<DummyCommand>();
			eventBus.Dispatch<DummyEvent>();
			Assert.AreEqual(1, DummyCommand.ExecuteCount);
		}

		[Test]
		public void DoCanBeChainedForMultipleCommands()
		{
			testObj.On<DummyEvent>().Do<DummyCommand>().Do<DummyCommand>();
			eventBus.Dispatch<DummyEvent>();
			Assert.AreEqual(2, DummyCommand.ExecuteCount);
		}

		[Test]
		public void OnCanBeCalledManyTimesForTheSameEventToCreateParallelChains()
		{
			testObj.On<DummyEvent>().Do<DummyCommand>();
			testObj.On<DummyEvent>().Do<DummyCommand>();
			eventBus.Dispatch<DummyEvent>();
			Assert.AreEqual(2, DummyCommand.ExecuteCount);
		}

		[Test]
		public void DoCanTakeOptionalInitializerForCommand()
		{
			var expectedValue = 100;
			testObj.On<DummyEvent>().Do<DummyCommand>((cmd, ev) =>
				{
					cmd.value = ev.value;
				});
			eventBus.Dispatch<DummyEvent>(ev => ev.value = expectedValue);
			Assert.AreEqual(expectedValue, DummyCommand.LastValue);
		}

		[Test]
		public void CommandFailureStopsChainExecution()
		{
			testObj.On<DummyEvent>().Do<FailingCommand>().Do<DummyCommand>();
			eventBus.Dispatch<DummyEvent>();
			Assert.AreEqual(0, DummyCommand.ExecuteCount);
		}

		[Test]
		public void LaunchCreatesCommandDirectly()
		{
			testObj.Launch<DummyCommand>();
			Assert.AreEqual(1, DummyCommand.ExecuteCount);
		}

		[Test]
		public void LaunchRunsInitializerOnCommandIfProvided()
		{
			testObj.Launch<DummyCommand>(cmd => cmd.value = 7500);
			Assert.AreEqual(7500, DummyCommand.LastValue);
		}

		[Test]
		public void OnFailureExecutesCommandWhenChainFails()
		{
			testObj.On<DummyEvent>().Do<FailingCommand>().OnFailure<DummyCommand>();
			eventBus.Dispatch<DummyEvent>();
			Assert.AreEqual(1, DummyCommand.ExecuteCount);
		}

		[Test]
		public void OnFailureCommandCanFailWithoutCreatingInfiniteRecursion()
		{
			testObj.On<DummyEvent>().Do<FailingCommand>().OnFailure<FailingCommand>();
			eventBus.Dispatch<DummyEvent>();
		}

		[Test]
		public void OnFailureCanTakeAnOptionalInitializerForCommand()
		{
			var expectedValue = 55;
			testObj.On<DummyEvent>().Do<FailingCommand>().OnFailure<DummyCommand>((cmd, evt) =>
				{
					cmd.value = evt.value;
				});
			eventBus.Dispatch<DummyEvent>(evt => evt.value = expectedValue);
			Assert.AreEqual(expectedValue, DummyCommand.LastValue);
		}

		[Test]
		public void OnFailureDoesNotReturnToMainChainWhenItSucceeds()
		{
			testObj.On<DummyEvent>().Do<FailingCommand>().Do<DummyCommand>().OnFailure<DummyCommand>();
			eventBus.Dispatch<DummyEvent>();
			Assert.AreEqual(1, DummyCommand.ExecuteCount);
		}

		[Test]
		public void AsyncCommandChainsLockTriggeringEvent()
		{
			testObj.On<DummyEvent>().Do<AsyncCommand>((cmd, evt) => cmd.trigger = evt);
			eventBus.Dispatch<DummyEvent>();
			eventBus.Dispatch<DummyEvent>();
			Assert.AreNotSame(AsyncCommand.triggeringEvents[0], AsyncCommand.triggeringEvents[1]);
		}

		[Test]
		public void AsyncCommandChainsReleaseLockOnTriggeringEventWhenTheyComplete()
		{
			testObj.On<DummyEvent>().Do<AsyncCommand>((cmd, evt) => cmd.trigger = evt);
			eventBus.Dispatch<DummyEvent>();
			eventBus.Dispatch<DummyEvent>();
			AsyncCommand.CompleteAll();
			eventBus.Dispatch<DummyEvent>();
			Assert.AreSame(AsyncCommand.triggeringEvents[0], AsyncCommand.triggeringEvents[2]);
		}
	}

	public class DummyCommand : Command
	{
		public static int ExecuteCount;
		public static int LastValue;

		public int value;

		override public void Execute()
		{
			ExecuteCount++;
			LastValue = value;
		}
	}

	public class FailingCommand : Command
	{
		public override void Execute()
		{
			Fail();
		}
	}

	public class AsyncCommand : Command
	{
		private static List<AsyncCommand> commands = new List<AsyncCommand>();
		public static List<Event> triggeringEvents = new List<Event>();
		public Event trigger;

		public override void Execute()
		{
			commands.Add(this);
			Retain();
			triggeringEvents.Add(trigger);
		}

		public static void CompleteAll()
		{
			foreach (var c in commands)
				c.Done();
			ClearAll();
		}

		public static void ClearAll()
		{
			commands.Clear();
		}

		public override void Reset()
		{
			base.Reset();
			trigger = null;
		}
	}
}
