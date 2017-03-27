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
		public void FreeAllowsObjectToBeUsedAgain()
		{
			var first = testObj.Allocate<DummyType>();
			testObj.Free(first);
			var second = testObj.Allocate<DummyType>();
			Assert.AreSame(first, second);
		}

		[Test]
		public void FreeThrowsExceptionIfInstanceIsNull()
		{
			Assert.Throws<NullInstanceException>(() => testObj.Free<DummyType>(null));
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
		public void FreeWithTypeAllowsObjectToBeUsedAgain()
		{
			var first = testObj.Allocate(typeof(DummyType)) as DummyType;
			testObj.Free(typeof(DummyType), first);
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
		public void FreeWithTypeThrowsExceptionForInvalidTypes()
		{
			Assert.Throws<InvalidTypeException>(() => testObj.Free(typeof(NonPoolableType), new DummyType()));
			Assert.Throws<InvalidTypeException>(() => testObj.Free(typeof(PoolableStruct), new PoolableStruct()));
			Assert.Throws<InvalidTypeException>(() => testObj.Free(typeof(PoolableWithConstructor), new PoolableWithConstructor(5)));
		}

		[Test]
		public void FreeWithTypeThrowsExceptionIfInstanceIsNull()
		{
			Assert.Throws<NullInstanceException>(() => testObj.Free(typeof(DummyType), null));
		}

		[Test]
		public void FreeWithTypeThrowsExceptionIfInstanceDoesNotMatchGivenType()
		{
			Assert.Throws<MismatchedTypeException>(() => testObj.Free(typeof(DummyType), new DummyEvent()));
		}

		[Test]
		public void LockOnAnInstancePreventsItFromBeingAllocated()
		{
			var key = new object();
			var first = testObj.Allocate<DummyType>();
			testObj.Lock(first, key);
			testObj.Free(first);
			var second = testObj.Allocate<DummyType>();
			Assert.AreNotSame(first, second);
		}

		[Test]
		public void LockAndUnlockWithSameKeyOnAnInstanceAllowsItToBeAllocated()
		{
			var key = new object();
			var first = testObj.Allocate<DummyType>();
			testObj.Lock(first, key);
			testObj.Free(first);
			testObj.Unlock(first, key);
			var second = testObj.Allocate<DummyType>();
			Assert.AreSame(first, second);
		}

		[Test]
		public void LockAndUnlockWithDifferentKeysOnAnInstanceDoesNotAllowItToBeAllocated()
		{
			var firstKey = new object();
			var secondKey = new object();
			var first = testObj.Allocate<DummyType>();
			testObj.Lock(first, firstKey);
			testObj.Free(first);
			testObj.Unlock(first, secondKey);
			var second = testObj.Allocate<DummyType>();
			Assert.AreNotSame(first, second);
		}

		[Test]
		public void UnlockingInstanceWithoutLockingItDoesNotAddItToPool()
		{
			var key = new object();
			var first = new DummyType();
			testObj.Unlock(first, key);
			var second = testObj.Allocate<DummyType>();
			Assert.AreNotSame(first, second);
		}

		[Test]
		public void LockOnAnInstancePreventsItFromBeingAllocatedWithType()
		{
			var key = new object();
			var first = testObj.Allocate(typeof(DummyType));
			testObj.Lock(first, key);
			testObj.Free(typeof(DummyType), first);
			var second = testObj.Allocate(typeof(DummyType));
			Assert.AreNotSame(first, second);
		}

		[Test]
		public void LockAndUnlockWithTypeAndSameKeyOnAnInstanceAllowsItToBeAllocated()
		{
			var key = new object();
			var first = testObj.Allocate(typeof(DummyType));
			testObj.Lock(first, key);
			testObj.Free(typeof(DummyType), first);
			testObj.Unlock(typeof(DummyType), first, key);
			var second = testObj.Allocate(typeof(DummyType));
			Assert.AreSame(first, second);
		}

		[Test]
		public void LockAndUnlockWithTypeAndDifferentKeysOnAnInstanceDoesNotAllowItToBeAllocated()
		{
			var firstKey = new object();
			var secondKey = new object();
			var first = testObj.Allocate(typeof(DummyType));
			testObj.Lock(first, firstKey);
			testObj.Free(typeof(DummyType), first);
			testObj.Unlock(typeof(DummyType), first, secondKey);
			var second = testObj.Allocate(typeof(DummyType));
			Assert.AreNotSame(first, second);
		}

		[Test]
		public void UnlockingInstanceWithTypeWithoutLockingItDoesNotAddItToPool()
		{
			var key = new object();
			var first = new DummyType();
			testObj.Unlock(typeof(DummyType), first, key);
			var second = testObj.Allocate(typeof(DummyType));
			Assert.AreNotSame(first, second);
		}

		[Test]
		public void LockingAndUnlockingAllocatedInstanceWithoutFreeingItDoesNotAllowItToBeAllocatedAgain()
		{
			var key = new object();
			var first = testObj.Allocate<DummyType>();
			testObj.Lock(first, key);
			testObj.Unlock(first, key);
			var second = testObj.Allocate<DummyType>();
			Assert.AreNotSame(first, second);
		}

		[Test]
		public void LockingAndUnlockingAllocatedWithTypeInstanceWithoutFreeingItDoesNotAllowItToBeAllocatedAgain()
		{
			var key = new object();
			var first = testObj.Allocate(typeof(DummyType));
			testObj.Lock(first, key);
			testObj.Unlock(first, key);
			var second = testObj.Allocate(typeof(DummyType));
			Assert.AreNotSame(first, second);
		}

		[Test]
		public void UniqueInstancesReflectsTheNumberOfAllocationsPoolHasDoneForEachType()
		{
			testObj.Allocate<DummyType>();
			testObj.Allocate<DummyType>();
			testObj.Allocate<DummyType>();
			testObj.Allocate<DummyEvent>();

			Assert.AreEqual(3, testObj.UniqueInstances[typeof(DummyType)]);
			Assert.AreEqual(1, testObj.UniqueInstances[typeof(DummyEvent)]);
			Assert.IsFalse(testObj.UniqueInstances.ContainsKey(typeof(DummyCommand)));
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
