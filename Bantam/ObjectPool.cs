using System;
using System.Collections;
using System.Collections.Generic;

namespace Bantam
{
	public class ObjectPool
	{
		private Dictionary<Type, Queue<Poolable>> instances = new Dictionary<Type, Queue<Poolable>>();

		public T Allocate<T>() where T : class, Poolable, new()
		{
			EnsurePoolExists<T>();
			return GetInstance<T>();
		}

		public Poolable Allocate(Type type)
		{
			ValidateType(type);
			EnsurePoolExists(type);
			return GetInstance(type);
		}

		public void Free<T>(T instance) where T : Poolable
		{
			if (null == instance)
				throw new NullInstanceException();
			EnsurePoolExists<T>();
			instances[typeof(T)].Enqueue(instance);
		}

		public void Free(Type type, Poolable instance)
		{
			if (null == instance)
				throw new NullInstanceException();
			ValidateType(type);
			if (!type.IsInstanceOfType(instance))
				throw new MismatchedTypeException();
			EnsurePoolExists(type);
			instances[type].Enqueue(instance);
		}

		private void EnsurePoolExists<T>()
		{
			EnsurePoolExists(typeof(T));
		}

		private void EnsurePoolExists(Type type)
		{
			if (!instances.ContainsKey(type))
				instances[type] = new Queue<Poolable>();
		}

		private void ValidateType(Type type)
		{
			if (!type.IsClass)
				throw new InvalidTypeException();
			if (null == type.GetConstructor(Type.EmptyTypes))
				throw new InvalidTypeException();
			if (!typeof(Poolable).IsAssignableFrom(type))
				throw new InvalidTypeException();
		}

		private T GetInstance<T>() where T : class, Poolable, new()
		{
			T instance;
			var type = typeof(T);
			if (0 < instances[type].Count)
				instance = instances[type].Dequeue() as T;
			else
				instance = new T();
			instance.Reset();
			return instance;
		}

		private Poolable GetInstance(Type type)
		{
			Poolable instance;
			if (0 < instances[type].Count)
				instance = instances[type].Dequeue();
			else
				instance = Activator.CreateInstance(type) as Poolable;
			instance.Reset();
			return instance;
		}
	}

	public class ObjectPoolException : Exception {}
	public class InvalidTypeException : ObjectPoolException {}
	public class NullInstanceException : ObjectPoolException {}
	public class MismatchedTypeException : ObjectPoolException {}
}
