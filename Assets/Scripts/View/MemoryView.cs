using System;
using Model;
using UniRx;
using UnityEngine;
using Utils;

namespace View
{
	public class MemoryView : MonoBehaviour, IDisposable
	{
		[SerializeField] private ProgramView _programViewTemplate;

		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		public void Show(Memory memory, Transform robotTransform, Camera mainCamera)
		{
			Observable.EveryUpdate()
				.Subscribe(_ => transform.position = mainCamera.WorldToScreenPoint(robotTransform.position) + new Vector3(0f, 100f))
				.AddTo(_disposable);

			memory.Size
				.Subscribe(x => ((RectTransform)transform).sizeDelta = new Vector2(x, ((RectTransform)transform).sizeDelta.y))
				.AddTo(_disposable);

			memory.Programs
				.CreateView(_programViewTemplate, transform, (view, program) => view.Show(program))
				.AddTo(_disposable);
		}

		public void Dispose()
		{
			_disposable.Dispose();
		}
	}
}
