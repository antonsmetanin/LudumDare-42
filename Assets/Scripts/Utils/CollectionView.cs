using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils
{
	public class MonoBehaviourCollectionView<T, TView> : IDisposable
		where TView : MonoBehaviour, IDisposable
	{
		private readonly CollectionView<T, TView> _collectionView;

		public MonoBehaviourCollectionView(IReadOnlyReactiveCollection<T> collection, TView template, Transform parentTransform, Action<TView, T> initializer)
		{
			_collectionView = new CollectionView<T, TView>(collection, template, (view, element) =>
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
		private readonly TView _template;
		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		public CollectionView(IReadOnlyReactiveCollection<T> collection, TView template, Action<TView, T> initializer)
		{
			_template = template;
			_initializer = initializer;

			foreach (var element in collection)
			{
				var view = Object.Instantiate(_template);
				_views.Add(view);
				_initializer(view, element);
			}

			collection.ObserveAdd().Subscribe(addEvent => Add(addEvent.Index, addEvent.Value)).AddTo(_disposable);
			collection.ObserveRemove().Subscribe(removeEvent => Remove(removeEvent.Index)).AddTo(_disposable);
		}

		private void Add(int index, T element)
		{
			var view = Object.Instantiate(_template);
			_views.Insert(index, view);
			_initializer(view, element);
		}

		private void Remove(int index)
		{
			var view = _views[index];
			view.Dispose();
			Object.Destroy(view.gameObject);
			_views.RemoveAt(index);
		}

		public void Dispose()
		{
			foreach (var view in _views)
			{
				view.Dispose();
				Object.Destroy(view);
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
