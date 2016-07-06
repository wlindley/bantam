using NUnit.Framework;

namespace Bantam.Test
{
	public class ModelRegistryTest
	{
		private ModelRegistry testObj;
		private EventBus eventBus;

		[SetUp]
		public void SetUp()
		{
			var pool = new ObjectPool();
			eventBus = new EventBus(pool);
			testObj = new ModelRegistry(pool, eventBus);
		}

		[Test]
		public void CreatingModelAllowsItToBeRetrieved()
		{
			var modelCount = 0;
			testObj.Create<DummyModel>();
			foreach (var model in testObj.Get<DummyModel>())
				modelCount++;
			Assert.AreEqual(1, modelCount);
		}

		[Test]
		public void CreateCallsInitializerWhenProvided()
		{
			var modelCount = 0;
			var expectedValue = "a random string";
			testObj.Create<DummyModel>((model) => {
				model.value = expectedValue;
			});
			foreach (var model in testObj.Get<DummyModel>())
				if (expectedValue == model.value)
					modelCount++;
			Assert.AreEqual(1, modelCount);
		}

		[Test]
		public void CreateFiresEvent()
		{
			var eventFired = false;
			var expectedValue = "another random string";
			eventBus.AddListener<ModelCreatedEvent>((evt) => {
				var model = (DummyModel)evt.model;
				Assert.AreEqual(expectedValue, model.value);
				Assert.AreEqual(typeof(DummyModel), evt.type);
				eventFired = true;
			});
			testObj.Create<DummyModel>((model) => {
				model.value = expectedValue;
			});
			Assert.IsTrue(eventFired);
		}

		[Test]
		public void GetReturnsEmptyEnumerableWhenNoModelsCreatedForType()
		{
			var wasCalled = false;
			foreach (var model in testObj.Get<DummyModel>())
				wasCalled = true;
			Assert.IsFalse(wasCalled);
		}

		[Test]
		public void DestroyCausesSpecifiedModelToNotBeReturnedByGet()
		{
			var matchingCount = 0;
			var totalCount = 0;
			var expectedValue = "yet another random string";

			DummyModel specifiedModel = null;
			testObj.Create<DummyModel>(model => {
				model.value = expectedValue;
				specifiedModel = model;
			});
			testObj.Create<DummyModel>();

			testObj.Destroy<DummyModel>(specifiedModel);

			foreach (var model in testObj.Get<DummyModel>())
			{
				totalCount++;
				if (expectedValue == model.value)
					matchingCount++;
			}
			Assert.AreEqual(0, matchingCount);
			Assert.AreEqual(1, totalCount);
		}

		[Test]
		public void DestroyDoesNothingSilentlyWhenSpecifiedModelIsNotInModelRegistry()
		{
			var model = new DummyModel();
			testObj.Destroy<DummyModel>(model);
		}

		[Test]
		public void DestroyThrowsNullModelExceptionWhenSpecifiedModelIsNull()
		{
			Assert.Throws<NullModelException>(() => testObj.Destroy<DummyModel>(null));
		}

		[Test]
		public void DestroyFiresEvent()
		{
			var eventFired = false;
			var expectedValue = "the most random string";

			DummyModel actualModel = null;
			eventBus.AddListener<ModelDestroyedEvent>((evt) => {
				var model = (DummyModel)evt.model;
				Assert.AreEqual(expectedValue, model.value);
				Assert.AreEqual(typeof(DummyModel), evt.type);
				eventFired = true;
			});
			testObj.Create<DummyModel>((model) => {
				model.value = expectedValue;
				actualModel = model;
			});

			testObj.Destroy<DummyModel>(actualModel);

			Assert.IsTrue(eventFired);
		}

		[Test]
		public void DestroyDoesNotFireAnEventWhenSpecifiedModelIsNotInModelRegistry()
		{
			var wasCalled = false;
			var model = new DummyModel();
			eventBus.AddListener<ModelDestroyedEvent>(evt => {
				wasCalled = true;
			});

			testObj.Destroy<DummyModel>(model);

			Assert.IsFalse(wasCalled);
		}
	}

	public class DummyModel : Model
	{
		public string value = "";

		public void Reset() {}
	}

	public class DummyModel2 : Model
	{
		public void Reset() {}
	}
}
