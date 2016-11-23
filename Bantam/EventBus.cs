using System;
using System.Collections;
using System.Collections.Generic;

namespace Bantam
{
	public delegate void EventListener<T>(T evt) where T : Event;
	public delegate void EventInitializer<T>(T evt) where T : Event;

	public class EventBus
	{
		private Dictionary<Type, ArrayList> listeners = new Dictionary<Type, ArrayList>();
		private Dictionary<Type, ArrayList> onceListeners = new Dictionary<Type, ArrayList>();
		private ArrayList allListeners = new ArrayList();
		private ArrayList toRemove = new ArrayList();
		private List<Type> keyList = new List<Type>();
		private ObjectPool pool;
		private bool isDispatching;

		public EventBus(ObjectPool pool)
		{
			this.pool = pool;
			isDispatching = false;
		}

		public void AddListener<T>(EventListener<T> listener) where T : class, Event, new()
		{
			EnsureKeyExists<T>();
			listeners[typeof(T)].Add(listener);
		}

		public void AddOnce<T>(EventListener<T> listener) where T : class, Event, new()
		{
			EnsureKeyExists<T>();
			onceListeners[typeof(T)].Add(listener);
		}

		public void AddListenerForAll(EventListener<Event> listener)
		{
			allListeners.Add(listener);
		}

		public void RemoveListener<T>(EventListener<T> listener) where T : class, Event
		{
			EnsureKeyExists<T>();
			var type = typeof(T);
			if (isDispatching)
				toRemove.Add(listener);
			else
				RemoveListener(listener, type);
		}

		public void Dispatch<T>(EventInitializer<T> initializer = null) where T : class, Event, new()
		{
			EnsureKeyExists<T>();
			var ev = pool.Allocate<T>();
			if (null != initializer)
				initializer(ev);
			
			DispatchEvent<T>(ev);

			pool.Free<T>(ev);
		}

		private void EnsureKeyExists<T>() where T : Event
		{
			var type = typeof(T);

			if (!keyList.Contains(type))
			{
				keyList.Add(type);
				listeners[type] = new ArrayList();
				onceListeners[type] = new ArrayList();
			}
		}

		private void DispatchEvent<T>(T ev) where T : Event
		{
			DispatchEventToListeners<T>(ev, listeners[typeof(T)]);

			var onceEventListeners = onceListeners[typeof(T)];
			DispatchEventToListeners<T>(ev, onceEventListeners);
			onceEventListeners.Clear();

			DispatchEventToListeners<Event>(ev, allListeners);
		}

		private void DispatchEventToListeners<T>(T ev, ArrayList eventListeners) where T : Event
		{
			isDispatching = true;
			var numListeners = eventListeners.Count;
			for (var i = 0; i < numListeners; i++)
				(eventListeners[i] as EventListener<T>)(ev);
			isDispatching = false;

			ProcessRemovals();
		}

		private void ProcessRemovals()
		{
			var numToRemove = toRemove.Count;
			var numKeys = keyList.Count;
			for (var toRemoveIndex = 0; toRemoveIndex < numToRemove; toRemoveIndex++)
				for (var keyIndex = 0; keyIndex < numKeys; keyIndex++)
					RemoveListener(toRemove[toRemoveIndex], keyList[keyIndex]);
		}

		private void RemoveListener(object listener, Type type)
		{
			listeners[type].Remove(listener);
			onceListeners[type].Remove(listener);
			allListeners.Remove(listener);
		}
	}
}
