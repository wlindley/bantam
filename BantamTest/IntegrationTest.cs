using System;
using NUnit.Framework;

namespace Bantam.Test
{
	[TestFixture]
	public class IntegrationTest
	{
		private ObjectPool pool;
		private EventBus eventBus;
		private CommandRelay commandRelay;

		private LoginServer server;
		private UserService userService;
		private UserWidget userWidget;

		[SetUp]
		public void SetUp()
		{
			pool = new ObjectPool();
			eventBus = new EventBus(pool);
			commandRelay = new CommandRelay(eventBus, pool);

			server = new LoginServer();
			userService = new UserService();
			userWidget = new UserWidget();
		}

		[Test]
		public void FullIntegrationTest()
		{
			commandRelay.On<LoginEvent>().Do<LoginCommand>((cmd, ev) =>
				{
					cmd.eventBus = eventBus;
					cmd.server = server;
					cmd.loginEvent = ev;
				});

			commandRelay.On<LoginFailedEvent>().Do<LoginCommand>((cmd, ev) =>
				{
					cmd.eventBus = eventBus;
					cmd.server = server;
					cmd.loginEvent = ev;
				});

			commandRelay.On<LoginSuccessEvent>().Do<RecordLoginCommand>((cmd, ev) =>
				{
					cmd.userService = userService;
					cmd.loginEvent = ev;
				}).Do<UpdateDisplayNameCommand>((cmd, ev) =>
				{
					cmd.userService = userService;
					cmd.userWidget = userWidget;
				}).Do<FailingCommand>()
				.Do<ScrambleDisplayNameCommand>((cmd, ev) =>
				{
					cmd.userWidget = userWidget;
				});

			TestLogin("Foo Bar");
			TestLogin("Bing Baz");

			Assert.AreEqual(1, pool.UniqueInstances[typeof(LoginEvent)]);
			Assert.AreEqual(1, pool.UniqueInstances[typeof(LoginFailedEvent)]);
			Assert.AreEqual(1, pool.UniqueInstances[typeof(LoginSuccessEvent)]);
			Assert.AreEqual(2, pool.UniqueInstances[typeof(LoginCommand)]); //A second LoginCommand is needed to respond to the first one failing before the first LoginCommand's Done method is called.
			Assert.AreEqual(1, pool.UniqueInstances[typeof(RecordLoginCommand)]);
			Assert.AreEqual(1, pool.UniqueInstances[typeof(UpdateDisplayNameCommand)]);
			Assert.AreEqual(1, pool.UniqueInstances[typeof(FailingCommand)]);
			Assert.IsFalse(pool.UniqueInstances.ContainsKey(typeof(ScrambleDisplayNameCommand)));
		}

		void TestLogin(string username)
		{
			eventBus.Dispatch<LoginEvent>(ev => 
			{
				ev.username = username;
				ev.password = "password";
			});
			
			server.DoLogin(false);
			server.DoLogin(true);

			Assert.AreEqual(username, userService.currentUser);
			Assert.AreEqual(username, userWidget.DisplayName);
		}
	}

	class LoginEvent : Event
	{
		public string username;
		public string password;

		public void Reset()
		{
			username = null;
			password = null;
		}
	}

	class LoginSuccessEvent : LoginEvent {}

	class LoginFailedEvent : LoginEvent {}

	class LoginServer
	{
		public event Action<bool> LoginComplete = delegate {};

		public void DoLogin(bool success)
		{
			LoginComplete(success);
		}
	}

	class UserService
	{
		public string currentUser;
	}

	class UserWidget
	{
		public string DisplayName { get { return name; } }

		private string name;

		public void SetName(string name)
		{
			this.name = name;
		}
	}

	class LoginCommand : Command
	{
		public EventBus eventBus;
		public LoginServer server;
		public LoginEvent loginEvent;

		public override void Execute()
		{
			server.LoginComplete += OnLoginComplete;
		}

		public override void Reset()
		{
			base.Reset();
			eventBus = null;
			server = null;
			loginEvent = null;
		}

		private void OnLoginComplete(bool success)
		{
			server.LoginComplete -= OnLoginComplete;

			if (success)
				eventBus.Dispatch<LoginSuccessEvent>(ev => {
					ev.username = loginEvent.username;
					ev.password = loginEvent.password;
				});
			else
				eventBus.Dispatch<LoginFailedEvent>(ev => {
					ev.username = loginEvent.username;
					ev.password = loginEvent.password;
				});
			Done();
		}
	}

	class RecordLoginCommand : Command
	{
		public UserService userService;
		public LoginSuccessEvent loginEvent;

		public override void Execute()
		{
			userService.currentUser = loginEvent.username;
			Done();
		}

		public override void Reset()
		{
			base.Reset();
			userService = null;
			loginEvent = null;
		}
	}

	class UpdateDisplayNameCommand : Command
	{
		public UserService userService;
		public UserWidget userWidget;

		public override void Execute()
		{
			userWidget.SetName(userService.currentUser);
			Done();
		}

		public override void Reset()
		{
			base.Reset();
			userService = null;
			userWidget = null;
		}
	}

	class ScrambleDisplayNameCommand : Command
	{
		public UserWidget userWidget;

		public override void Execute()
		{
			var scrambledName = userWidget.DisplayName;
			var midPoint = scrambledName.Length / 2;
			scrambledName = scrambledName.Substring(midPoint) + scrambledName.Substring(0, midPoint);
			userWidget.SetName(scrambledName);
		}

		public override void Reset()
		{
			base.Reset();
			userWidget = null;
		}
	}
}
