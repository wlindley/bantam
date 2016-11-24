using NUnit.Framework;
using System;

namespace Bantam.Test
{
	[TestFixture]
	public class EventBusTest
	{
		private EventBus testObj;

		[SetUp]
		public void SetUp()
		{
			testObj = new EventBus(new ObjectPool());
		}

		[Test]
		public void DispatchEventSendsEventsToRegisteredListeners()
		{
			var wasCalled = false;
			testObj.AddListener<DummyEvent>(ev => wasCalled = true);
			testObj.Dispatch<DummyEvent>();
			Assert.IsTrue(wasCalled);
		}

		[Test]
		public void DispatchEventCallsProvidedInitializer()
		{
			var wasCalled = false;
			var expectedValue = 15;
			testObj.AddListener<DummyEvent>(ev =>
				{
					wasCalled = true;
					Assert.AreEqual(expectedValue, ev.value);
				});
			testObj.Dispatch<DummyEvent>(ev =>
				{
					ev.value = expectedValue;
				});
			Assert.IsTrue(wasCalled);
		}

		[Test]
		public void AddOnceListenerOnlyGetsCalledOnNextEvent()
		{
			var callCount = 0;
			testObj.AddOnce<DummyEvent>(ev => callCount++);
			testObj.Dispatch<DummyEvent>();
			testObj.Dispatch<DummyEvent>();
			Assert.AreEqual(1, callCount);
		}

		[Test]
		public void AddListenerGetsCalledForAllSubsequentEvents()
		{
			var callCount = 0;
			testObj.AddListener<DummyEvent>(ev => callCount++);
			testObj.Dispatch<DummyEvent>();
			testObj.Dispatch<DummyEvent>();
			Assert.AreEqual(2, callCount);
		}

		[Test]
		public void DispatchDoesNotThrowExceptionWhenNoListenersAreRegistered()
		{
			testObj.Dispatch<DummyEvent>();
		}

		[Test]
		public void RemoveListenerStopsEventsFromBeingDispatchedToThatListener()
		{
			var callCount = 0;
			var listener = new EventListener<DummyEvent>(ev => callCount++);
			testObj.AddListener(listener);
			testObj.Dispatch<DummyEvent>();
			testObj.RemoveListener<DummyEvent>(listener);
			testObj.Dispatch<DummyEvent>();
			Assert.AreEqual(1, callCount);
		}

		[Test]
		public void RemoveListenerPreventsCallToOnceListener()
		{
			var wasCalled = false;
			var listener = new EventListener<DummyEvent>(ev => wasCalled = true);
			testObj.AddOnce(listener);
			testObj.RemoveListener(listener);
			testObj.Dispatch<DummyEvent>();
			Assert.IsFalse(wasCalled);
		}

		[Test]
		public void OnceListenersForMultipleEventsAllGetCalled()
		{
			var wasFirstCalled = false;
			var wasSecondCalled = false;
			testObj.AddOnce<DummyEvent>(ev => wasFirstCalled = true);
			testObj.AddOnce<DummyEvent2>(ev => wasSecondCalled = true);
			testObj.Dispatch<DummyEvent>();
			testObj.Dispatch<DummyEvent2>();
			Assert.IsTrue(wasFirstCalled);
			Assert.IsTrue(wasSecondCalled);
		}

		[Test]
		public void AddListenerForAllCausesListenerToBeCalledForAllEvents()
		{
			var callCount = 0;
			testObj.AddListenerForAll(ev => callCount++);
			testObj.Dispatch<DummyEvent>();
			testObj.Dispatch<DummyEvent2>();
			Assert.AreEqual(2, callCount);
		}

		[Test]
		public void RemoveListenerPreventsEventsFromGoingToAnAllListener()
		{
			var wasCalled = false;
			var listener = new EventListener<Event>(ev => wasCalled = true);
			testObj.AddListenerForAll(listener);
			testObj.RemoveListener<Event>(listener);
			testObj.Dispatch<DummyEvent>();
			Assert.IsFalse(wasCalled);
		}

		[Test]
		public void RemoveListenerSucceedsWhenCalledFromHandlerForEvent()
		{
			var wasSecondListenerCalled = false;
			Action action = () => {};
			var listener = new EventListener<DummyEvent>(evt => action());
			action = () => {
				testObj.RemoveListener<DummyEvent>(listener);
			};
			testObj.AddListener<DummyEvent>(listener);
			testObj.AddListener<DummyEvent>(evt => wasSecondListenerCalled = true);

			testObj.Dispatch<DummyEvent>();

			Assert.IsTrue(wasSecondListenerCalled, "Second listener was not executed.");
		}

		[Test]
		public void RemoveListenerSucceedsWhenCalledFromOnceHandlerForEvent()
		{
			var wasSecondListenerCalled = false;
			Action action = () => {};
			var listener = new EventListener<DummyEvent>(evt => action());
			action = () => {
				testObj.RemoveListener<DummyEvent>(listener);
			};
			testObj.AddOnce<DummyEvent>(listener);
			testObj.AddOnce<DummyEvent>(evt => wasSecondListenerCalled = true);

			testObj.Dispatch<DummyEvent>();

			Assert.IsTrue(wasSecondListenerCalled, "Second listener was not executed.");
		}
	}

	public class DummyEvent : Event
	{
		public int value;

		public void Reset()
		{
			
		}
	}

	public class DummyEvent2 : Event
	{
		public void Reset()
		{
			
		}
	}
}
