using System;
using Model;
using UniRx;
using UnityEngine;

namespace View
{
	public class DataFileView : MonoBehaviour, IDisposable
	{
		private CompositeDisposable _disposable;

		public void Show(Robot robot, Func<int> getBytes)
		{
			_disposable = new CompositeDisposable();

			Observable.EveryUpdate()
				.Subscribe(_ => ((RectTransform)transform).sizeDelta = new Vector2(getBytes(), ((RectTransform)transform).sizeDelta.y))
				.AddTo(_disposable);
		}

		public void Dispose() => _disposable.Dispose();
	}
}
