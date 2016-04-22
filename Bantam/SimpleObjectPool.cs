﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Bantam
{
	public class SimpleObjectPool
	{
		private Dictionary<Type, Queue> instances = new Dictionary<Type, Queue>();

		public T Allocate<T>(Action<T> initializer = null) where T : class, new()
		{
			EnsurePoolExists<T>();
			var instance = GetInstance<T>();
			if (null != initializer)
				initializer(instance);
			return instance;
		}

		public void Release<T>(T instance)
		{
			EnsurePoolExists<T>();
			instances[typeof(T)].Enqueue(instance);
		}

		private void EnsurePoolExists<T>()
		{
			var type = typeof(T);
			if (!instances.ContainsKey(type))
				instances[type] = new Queue();
		}

		private T GetInstance<T>() where T : class, new()
		{
			T instance;
			var type = typeof(T);
			if (0 < instances[type].Count)
				instance = instances[type].Dequeue() as T;
			else
				instance = new T();
			return instance;
		}
	}
}
