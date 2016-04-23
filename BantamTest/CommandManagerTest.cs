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
			eventBus = new EventBus();
			testObj = new CommandManager(eventBus);
		}

		[Test]
		public void OnDoCausesCommandToBeExecuted()
		{
			DummyCommand.ExecuteCount = 0;
			testObj.On<DummyEvent>().Do<DummyCommand>();
			eventBus.Dispatch<DummyEvent>();
			Assert.AreEqual(1, DummyCommand.ExecuteCount);
		}
	}

	public class DummyCommand : Command
	{
		public static int ExecuteCount;

		override public void Execute()
		{
			ExecuteCount++;
			Done();
		}
	}
}
