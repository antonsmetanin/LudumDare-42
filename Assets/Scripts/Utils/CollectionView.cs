using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils
{
    public class MonoBehaviourReactiveCollectionView<T, TView> : IDisposable
		where TView : MonoBehaviour, IDisposable
	{
		private readonly ReactiveCollectionView<T, TView> _collectionView;

		public MonoBehaviourReactiveCollectionView(IReadOnlyReactiveCollection<T> collection, TView template, Transform parentTransform, Action<TView, T> initializer)
		{
			_collectionView = new ReactiveCollectionView<T, TView>(collection, new Instantiator<TView>(template), (view, element) =>
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

    public class MonoBehaviourCollectionView<T, TView> : IDisposable
        where TView : MonoBehaviour, IDisposable
    {
        private readonly CollectionView<T, TView> _collectionView;

        public MonoBehaviourCollectionView(IEnumerable<T> collection, TView template, Transform parentTransform, Action<TView, T> initializer)
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
        where TView : IDisposable
    {
        protected readonly List<TView> Views = new List<TView>();
        protected readonly Action<TView, T> Initializer;
        protected readonly IFactory<TView> ViewFactory;
        protected readonly CompositeDisposable Disposable = new CompositeDisposable();

        public CollectionView(IEnumerable<T> collection, IFactory<TView> factory, Action<TView, T> initializer)
        {
            ViewFactory = factory;
            Initializer = initializer;

            foreach (var element in collection)
            {
                var view = factory.Create();
                Views.Add(view);
                Initializer(view, element);
            }
        }

        public void Dispose()
        {
            foreach (var view in Views)
            {
                view.Dispose();
                ViewFactory.Destroy(view);
            }

            Disposable.Dispose();

            Views.Clear();
        }
    }

	public class ReactiveCollectionView<T, TView> : CollectionView<T, TView>
		where TView : IDisposable
	{
		public ReactiveCollectionView(IReadOnlyReactiveCollection<T> collection, IFactory<TView> factory, Action<TView, T> initializer)
            : base(collection, factory, initializer)
		{
			collection.ObserveAdd().Subscribe(addEvent => Add(addEvent.Index, addEvent.Value)).AddTo(Disposable);
			collection.ObserveRemove().Subscribe(removeEvent => Remove(removeEvent.Index)).AddTo(Disposable);
		}

		private void Add(int index, T element)
		{
            var view = ViewFactory.Create();
			Views.Insert(index, view);
			Initializer(view, element);
		}

		private void Remove(int index)
		{
			var view = Views[index];
			view.Dispose();
            ViewFactory.Destroy(view);
			Views.RemoveAt(index);
		}
	}

	public static class CollectionViewExt
	{
		public static MonoBehaviourReactiveCollectionView<T, TView> CreateView<T, TView>(this IReadOnlyReactiveCollection<T> collection, TView template, Transform parentTransform, Action<TView, T> initializer)
			where TView : MonoBehaviour, IDisposable => new MonoBehaviourReactiveCollectionView<T, TView>(collection, template, parentTransform, initializer);

        public static MonoBehaviourCollectionView<T, TView> CreateView<T, TView>(this IEnumerable<T> collection, TView template, Transform parentTransform, Action<TView, T> initializer)
            where TView : MonoBehaviour, IDisposable => new MonoBehaviourCollectionView<T, TView>(collection, template, parentTransform, initializer);
    }
}
