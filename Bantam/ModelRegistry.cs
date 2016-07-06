using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bantam
{
	public delegate void ModelInitializer<T>(T model) where T : Model;

	public class ModelRegistry
	{
		private Dictionary<Type, ModelListWrapper> modelLists;
		private EventBus eventBus;
		private ObjectPool pool;

		public ModelRegistry(ObjectPool pool, EventBus eventBus)
		{
			this.pool = pool;
			this.eventBus = eventBus;
			modelLists = new Dictionary<Type, ModelListWrapper>();
		}

		public void Create<T>(ModelInitializer<T> initializer = null) where T : class, Model, new()
		{
			EnsureTypeExists<T>();
			(modelLists[typeof(T)] as ModelListWrapper<T>).CreateModel(initializer);
		}

		public IEnumerable<T> Get<T>() where T : class, Model, new()
		{
			EnsureTypeExists<T>();
			return modelLists[typeof(T)].GetModels<T>();
		}

		public void Destroy<T>(T model) where T : class, Model, new()
		{
			EnsureTypeExists<T>();
			(modelLists[typeof(T)] as ModelListWrapper<T>).DestroyModel(model);
		}

		private void EnsureTypeExists<T>() where T : class, Model, new()
		{
			var type = typeof(T);
			if (!modelLists.ContainsKey(type))
				modelLists[type] = new ModelListWrapper<T>(pool, eventBus);
		}

		private interface ModelListWrapper
		{
			IEnumerable<T> GetModels<T>() where T : class, Model, new();
		}

		private class ModelListWrapper<T> : ModelListWrapper where T : class, Model, new()
		{
			private List<T> models;
			private ObjectPool pool;
			private EventBus eventBus;

			public ModelListWrapper(ObjectPool pool, EventBus eventBus)
			{
				this.pool = pool;
				this.eventBus = eventBus;
				models = new List<T>();
			}

			public IEnumerable<U> GetModels<U>() where U : class, Model, new()
			{
				return models as IEnumerable<U>;
			}

			public void CreateModel(ModelInitializer<T> initializer = null)
			{
				var instance = pool.Allocate<T>();
				if (null != initializer)
					initializer(instance);
				models.Add(instance);
				eventBus.Dispatch<ModelCreatedEvent>(evt => {
					evt.model = instance;
					evt.type = typeof(T);
				});
			}

			public void DestroyModel(T model)
			{
				if (null == model)
					throw new NullModelException();
				if (!models.Contains(model))
					return;
				models.Remove(model);
				eventBus.Dispatch<ModelDestroyedEvent>(evt => {
					evt.model = model;
					evt.type = typeof(T);
				});
				pool.Free(model);
			}
		}
	}

	public class ModelRegistryException : Exception {}
	public class NullModelException : ModelRegistryException {}
}
