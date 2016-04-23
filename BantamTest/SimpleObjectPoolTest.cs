using System;
using NUnit.Framework;

namespace Bantam.Test
{
	[TestFixture]
	public class SimpleObjectPoolTest
	{
		private SimpleObjectPool testObj;

		[SetUp]
		public void SetUp()
		{
			testObj = new SimpleObjectPool();
		}

		[Test]
		public void AllocateReturnsInstanceOfType()
		{
			var instance = testObj.Allocate<DummyType>();
			Assert.NotNull(instance);
		}

		[Test]
		public void AllocateResetsObjectItReturns()
		{
			var instance = testObj.Allocate<DummyType>();
			Assert.AreEqual(0, instance.value);
		}

		[Test]
		public void AllocateReturnsDifferentObjectsOnSubsequentCalls()
		{
			var first = testObj.Allocate<DummyType>();
			var second = testObj.Allocate<DummyType>();
			Assert.AreNotSame(first, second);
		}

		[Test]
		public void ReleaseAllowsObjectToBeUsedAgain()
		{
			var first = testObj.Allocate<DummyType>();
			testObj.Release<DummyType>(first);
			var second = testObj.Allocate<DummyType>();
			Assert.AreSame(first, second);
		}
	}

	public class DummyType : Poolable
	{
		public int value = -1;

		public void Reset()
		{
			value = 0;
		}
	}
}
