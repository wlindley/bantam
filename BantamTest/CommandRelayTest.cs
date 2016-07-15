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
}
