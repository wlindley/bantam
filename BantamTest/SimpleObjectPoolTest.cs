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
	}
}
