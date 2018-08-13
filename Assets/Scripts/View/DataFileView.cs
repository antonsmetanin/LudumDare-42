using System;
using Model;
using UniRx;
using UnityEngine;

namespace View
{
	public class DataFileView : MonoBehaviour, IDisposable
	{
		private CompositeDisposable _disposable;

		public void Show(Robot robot, IObservable<int> data)
		{
			_disposable = new CompositeDisposable();

			data.Subscribe(size => ((RectTransform)transform).sizeDelta = new Vector2(size, ((RectTransform)transform).sizeDelta.y))
				.AddTo(_disposable);
		}

		public void Dispose() => _disposable.Dispose();
	}
}
