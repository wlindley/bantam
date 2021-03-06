﻿using System;
using System.Collections.Generic;

namespace Bantam
{
	public class ObjectPool
	{
		public Dictionary<Type, int> UniqueInstances = new Dictionary<Type, int>();

		private readonly Dictionary<Type, Queue<Poolable>> instances = new Dictionary<Type, Queue<Poolable>>();
		private readonly Dictionary<Poolable, MultiLock> lockedInstances = new Dictionary<Poolable, MultiLock>();

		public T Allocate<T>() where T : class, Poolable, new()
		{
			EnsurePoolExists<T>();
			var instance = GetInstance<T>();
			Lock(instance, this);
			return instance;
		}

		public Poolable Allocate(Type type)
		{
			ValidateType(type);
			EnsurePoolExists(type);
			var instance = GetInstance(type);
			Lock(instance, this);
			return instance;
		}

		public void Free<T>(T instance) where T : Poolable
		{
			if (null == instance)
				throw new NullInstanceException();
			Unlock(instance, this);
		}

		public void Free(Type type, Poolable instance)
		{
			Unlock(type, instance, this);
		}

		public void Lock(Poolable instance, object key)
		{
			EnsureLockExists(instance);
			lockedInstances[instance].Lock(key);
		}

		public void Unlock<T>(T instance, object key) where T : Poolable
		{
			Unlock(typeof(T), instance, key);
		}

		public void Unlock(Type type, Poolable instance, object key)
		{
			Validate(type, instance);
			MultiLock instanceLock;
			lockedInstances.TryGetValue(instance, out instanceLock);
			if (null == instanceLock)
				return;
			instanceLock.Unlock(key);
			if (!instanceLock.IsLocked)
			{
				lockedInstances.Remove(instance);
				FreeInternalLock(instanceLock);
				InternalFree(type, instance);
			}
		}

		private void Validate(Type type, Poolable instance)
		{
			if (null == instance)
				throw new NullInstanceException();
			ValidateType(type);
			if (!type.IsInstanceOfType(instance))
				throw new MismatchedTypeException();
		}

		private void EnsurePoolExists<T>()
		{
			EnsurePoolExists(typeof(T));
		}

		private void EnsurePoolExists(Type type)
		{
			if (!instances.ContainsKey(type))
			{
				instances[type] = new Queue<Poolable>();
				UniqueInstances[type] = 0;
			}
		}

		private void EnsureLockExists(Poolable instance)
		{
			if (!lockedInstances.ContainsKey(instance))
				lockedInstances[instance] = AllocateInternalLock();
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
			if (0 >= instances[type].Count)
			{
				instance = new T();
				UniqueInstances[type]++;
			}
			else
			{
				var poolable = instances[type].Dequeue();
				if (null == poolable)
					throw new NullInPoolException1();
				if (!(poolable is T))
					throw new MismatchedTypeInPoolException1();
				instance = poolable as T;
			}
			instance.Reset();
			return instance;
		}

		private Poolable GetInstance(Type type)
		{
			Poolable instance;
			if (0 >= instances[type].Count)
			{
				instance = Activator.CreateInstance(type) as Poolable;
				UniqueInstances[type]++;
				if (null == instance)
					throw new NullInPoolException2();
				if (!type.IsInstanceOfType(instance))
					throw new MismatchedTypeInPoolException2();
			}
			else
			{
				instance = instances[type].Dequeue();
				if (null == instance)
					throw new NullInPoolException3();
				if (!type.IsInstanceOfType(instance))
					throw new MismatchedTypeInPoolException3();
			}
			instance.Reset();
			return instance;
		}

		private MultiLock AllocateInternalLock()
		{
			EnsurePoolExists<MultiLock>();
			return GetInstance<MultiLock>();
		}

		private void FreeInternalLock(MultiLock internalLock)
		{
			EnsurePoolExists<MultiLock>();
			instances[typeof(MultiLock)].Enqueue(internalLock);
		}

		private void InternalFree(Type type, Poolable instance)
		{
			EnsurePoolExists(type);
			instances[type].Enqueue(instance);
		}
	}

	public class ObjectPoolException : Exception {}
	public class InvalidTypeException : ObjectPoolException {}
	public class NullInstanceException : ObjectPoolException {}
	public class MismatchedTypeException : ObjectPoolException {}
	public class NullInPoolException1 : ObjectPoolException {}
	public class MismatchedTypeInPoolException1 : ObjectPoolException {}
	public class NullInPoolException2 : ObjectPoolException { }
	public class MismatchedTypeInPoolException2 : ObjectPoolException { }
	public class NullInPoolException3 : ObjectPoolException { }
	public class MismatchedTypeInPoolException3 : ObjectPoolException { }
}
