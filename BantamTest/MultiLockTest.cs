using NUnit.Framework;

namespace Bantam.Test
{
	[TestFixture]
	public class MultiLockTest
	{
		private MultiLock testObj;
		private object key, secondKey;

		[SetUp]
		public void SetUp()
		{
			testObj = new MultiLock();
			key = new object();
			secondKey = new object();
		}

		[Test]
		public void IsLockedIsFalseByDefault()
		{
			Assert.IsFalse(testObj.IsLocked);
		}

		[Test]
		public void IsLockedIsTrueAfterLockIsCalled()
		{
			testObj.Lock(key);
			Assert.IsTrue(testObj.IsLocked);
		}

		[Test]
		public void IsLockedIsFalseAfterUnlockIsCalledWithKey()
		{
			testObj.Lock(key);
			testObj.Unlock(key);
			Assert.IsFalse(testObj.IsLocked);
		}

		[Test]
		public void IsLockedIsTrueAfterUnlockIsCalledWithIncorrectKey()
		{
			testObj.Lock(key);
			testObj.Unlock(secondKey);
			Assert.IsTrue(testObj.IsLocked);
		}

		[Test]
		public void IsLockedIsTrueIfLockedWithTwoKeysAndOnlyUnlockedWithFirstKey()
		{
			testObj.Lock(key);
			testObj.Lock(secondKey);
			testObj.Unlock(key);
			Assert.IsTrue(testObj.IsLocked);
		}

		[Test]
		public void IsLockedIsTrueIfLockedWithTwoKeysAndOnlyUnlockedWithSecondKey()
		{
			testObj.Lock(key);
			testObj.Lock(secondKey);
			testObj.Unlock(secondKey);
			Assert.IsTrue(testObj.IsLocked);
		}

		[Test]
		public void IsLockedIsFalseIfLockWithTwoKeysAndUnlockedWithBothKeys()
		{
			testObj.Lock(key);
			testObj.Lock(secondKey);
			testObj.Unlock(key);
			testObj.Unlock(secondKey);
			Assert.IsFalse(testObj.IsLocked);
		}

		[Test]
		public void ResetClearsAllKeys()
		{
			testObj.Lock(key);
			testObj.Lock(secondKey);
			testObj.Reset();
			Assert.IsFalse(testObj.IsLocked);
		}
	}
}
