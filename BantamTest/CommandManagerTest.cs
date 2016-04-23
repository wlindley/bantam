using NUnit.Framework;

namespace Bantam.Test
{
	[TestFixture]
	public class CommandManagerTest
	{
		private CommandManager testObj;
		private EventBus eventBus;

		[SetUp]
		public void SetUp()
		{
			var pool = new ObjectPool();
			eventBus = new EventBus(pool);
			testObj = new CommandManager(eventBus, pool);
		}

		[Test]
		public void OnDoCausesCommandToBeExecuted()
		{
			DummyCommand.ExecuteCount = 0;
			testObj.On<DummyEvent>().Do<DummyCommand>();
			eventBus.Dispatch<DummyEvent>();
			Assert.AreEqual(1, DummyCommand.ExecuteCount);
		}

		[Test]
		public void DoCanBeChainedForMultipleCommands()
		{
			DummyCommand.ExecuteCount = 0;
			testObj.On<DummyEvent>().Do<DummyCommand>().Do<DummyCommand>();
			eventBus.Dispatch<DummyEvent>();
			Assert.AreEqual(2, DummyCommand.ExecuteCount);
		}

		[Test]
		public void OnCanBeCalledManyTimesForTheSameEventToCreateParallelChains()
		{
			DummyCommand.ExecuteCount = 0;
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
					(cmd as DummyCommand).value = (ev as DummyEvent).value;
				});
			eventBus.Dispatch<DummyEvent>(ev => ev.value = expectedValue);
			Assert.AreEqual(expectedValue, DummyCommand.LastValue);
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
			Done();
		}
	}
}
