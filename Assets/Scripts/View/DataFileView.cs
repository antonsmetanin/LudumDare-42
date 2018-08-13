using System;
using JetBrains.Annotations;
using Model;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace View
{
	public class DataFileView : MonoBehaviour, IDisposable, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		private CompositeDisposable _disposable;

		[SerializeField] private DraggedDataFileView _draggedDataFileTemplate;

		private DraggedDataFileView _draggedDataFile;

		private DataFileType _type;
		private Robot _robot;
		private Func<int> _getBytes;

		public enum DataFileType
		{
			Leak,
			Produce
		}

		public void Show(Robot robot, DataFileType type, Func<int> getBytes)
		{
			_robot = robot;
			_type = type;
			_getBytes = getBytes;
			_disposable = new CompositeDisposable();

			Observable.EveryUpdate().Merge(Observable.Return(0l))
				.Subscribe(_ => ((RectTransform)transform).sizeDelta = new Vector2(getBytes(), ((RectTransform)transform).sizeDelta.y))
				.AddTo(_disposable);
		}

		public void Dispose() => _disposable.Dispose();

		public void OnDrag([NotNull] PointerEventData eventData)
		{
			_draggedDataFile.Drag(eventData);

			eventData.pointerEnter?.GetComponent<IDropAccepter<DataFileView>>()?.Accept(this, simulate: true);
		}

		public void OnBeginDrag([NotNull] PointerEventData eventData)
		{
			_draggedDataFile = Instantiate(_draggedDataFileTemplate);
			_draggedDataFile.transform.SetParent(GetComponentInParent<Canvas>().transform, worldPositionStays: false);
			_draggedDataFile.transform.position = transform.position;
			_draggedDataFile.Show(_robot, _type, _getBytes);
		}

		public void OnEndDrag([NotNull] PointerEventData eventData)
		{
			eventData.pointerEnter?.GetComponent<IDropAccepter<DataFileView>>()?.Accept(this);

			_draggedDataFile.Dispose();
		}
	}
}
