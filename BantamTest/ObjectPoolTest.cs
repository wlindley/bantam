using System;
using NUnit.Framework;

namespace Bantam.Test
{
	[TestFixture]
	public class ObjectPoolTest
	{
		private ObjectPool testObj;

		[SetUp]
		public void SetUp()
		{
			testObj = new ObjectPool();
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

		[Test]
		public void ReleaseThrowsExceptionIfInstanceIsNull()
		{
			Assert.Throws<NullInstanceException>(() => testObj.Release<DummyType>(null));
		}

		[Test]
		public void AllocateWithTypeParameterReturnsInstanceOfType()
		{
			var instance = testObj.Allocate(typeof(DummyType)) as DummyType;
			Assert.NotNull(instance);
		}

		[Test]
		public void AllocateWithTypeResetsObjectItReturns()
		{
			var instance = testObj.Allocate(typeof(DummyType)) as DummyType;
			Assert.AreEqual(0, instance.value);
		}

		[Test]
		public void AllocateWithTypeReturnsDifferentObjectsOnSubsequentCalls()
		{
			var first = testObj.Allocate(typeof(DummyType)) as DummyType;
			var second = testObj.Allocate(typeof(DummyType)) as DummyType;
			Assert.AreNotSame(first, second);
		}

		[Test]
		public void ReleaseWithTypeAllowsObjectToBeUsedAgain()
		{
			var first = testObj.Allocate(typeof(DummyType)) as DummyType;
			testObj.Release(typeof(DummyType), first);
			var second = testObj.Allocate(typeof(DummyType)) as DummyType;
			Assert.AreSame(first, second);
		}

		[Test]
		public void AllocateWithTypeThrowsExceptionForInvalidTypes()
		{
			Assert.Throws<InvalidTypeException>(() => testObj.Allocate(typeof(NonPoolableType)));
			Assert.Throws<InvalidTypeException>(() => testObj.Allocate(typeof(PoolableStruct)));
			Assert.Throws<InvalidTypeException>(() => testObj.Allocate(typeof(PoolableWithConstructor)));
		}

		[Test]
		public void ReleaseWithTypeThrowsExceptionForInvalidTypes()
		{
			Assert.Throws<InvalidTypeException>(() => testObj.Release(typeof(NonPoolableType), new DummyType()));
			Assert.Throws<InvalidTypeException>(() => testObj.Release(typeof(PoolableStruct), new PoolableStruct()));
			Assert.Throws<InvalidTypeException>(() => testObj.Release(typeof(PoolableWithConstructor), new PoolableWithConstructor(5)));
		}

		[Test]
		public void ReleaseWithTypeThrowsExceptionIfInstanceIsNull()
		{
			Assert.Throws<NullInstanceException>(() => testObj.Release(typeof(DummyType), null));
		}

		[Test]
		public void ReleaseWithTypeThrowsExceptionIfInstanceDoesNotMatchGivenType()
		{
			Assert.Throws<MismatchedTypeException>(() => testObj.Release(typeof(DummyType), new DummyEvent()));
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

	public class NonPoolableType
	{
	}

	public struct PoolableStruct : Poolable
	{
		public void Reset()
		{
			
		}
	}

	public class PoolableWithConstructor : Poolable
	{
		public PoolableWithConstructor(int value)
		{
			
		}

		public void Reset()
		{
			
		}
	}
}
