using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils
{
    public interface IFactory<T>
    {
        T Create();
        void Destroy(T t);
    }

    public class Instantiator<T> : IFactory<T>
        where T : Component
    {
        private T _template;

        public Instantiator(T template)
        {
            _template = template;
        }

        public T Create()
        {
            return Object.Instantiate(_template);
        }

        public void Destroy(T t)
        {
            Object.Destroy(t.gameObject);
        }
    }

    public class MonoBehaviourCollectionView<T, TView> : IDisposable
		where TView : MonoBehaviour, IDisposable
	{
		private readonly CollectionView<T, TView> _collectionView;

		public MonoBehaviourCollectionView(IReadOnlyReactiveCollection<T> collection, TView template, Transform parentTransform, Action<TView, T> initializer)
		{
			_collectionView = new CollectionView<T, TView>(collection, new Instantiator<TView>(template), (view, element) =>
			{
				view.transform.SetParent(parentTransform, worldPositionStays: false);
				initializer(view, element);
			});
		}

		public void Dispose()
		{
			_collectionView.Dispose();
		}
	}

	public class CollectionView<T, TView> : IDisposable
		where TView : Component, IDisposable
	{
		private readonly List<TView> _views = new List<TView>();
		private readonly Action<TView, T> _initializer;
		private readonly IFactory<TView> _factory;
		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		public CollectionView(IReadOnlyReactiveCollection<T> collection, IFactory<TView> factory, Action<TView, T> initializer)
		{
			_factory = factory;
			_initializer = initializer;

			foreach (var element in collection)
			{
                var view = factory.Create();
				_views.Add(view);
				_initializer(view, element);
			}

			collection.ObserveAdd().Subscribe(addEvent => Add(addEvent.Index, addEvent.Value)).AddTo(_disposable);
			collection.ObserveRemove().Subscribe(removeEvent => Remove(removeEvent.Index)).AddTo(_disposable);
		}

		private void Add(int index, T element)
		{
            var view = _factory.Create();
			_views.Insert(index, view);
			_initializer(view, element);
		}

		private void Remove(int index)
		{
			var view = _views[index];
			view.Dispose();
            _factory.Destroy(view);
			_views.RemoveAt(index);
		}

		public void Dispose()
		{
			foreach (var view in _views)
			{
				view.Dispose();
                _factory.Destroy(view);
			}

			_views.Clear();
		}
	}

	public static class CollectionViewExt
	{
		public static MonoBehaviourCollectionView<T, TView> CreateView<T, TView>(this IReadOnlyReactiveCollection<T> collection, TView template, Transform parentTransform, Action<TView, T> initializer)
			where TView : MonoBehaviour, IDisposable => new MonoBehaviourCollectionView<T, TView>(collection, template, parentTransform, initializer);
	}
}
