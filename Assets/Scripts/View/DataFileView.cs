using System;
using JetBrains.Annotations;
using Model;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace View
{
	public class DataFileView : MonoBehaviour, IDisposable, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		private CompositeDisposable _disposable;

		[SerializeField] private DraggedDataFileView _draggedDataFileTemplate;
        [SerializeField] private Button _button;

        private DraggedDataFileView _draggedDataFile;

		public DataFileType Type;
		public Robot Robot;
        private Game _game;
		private Func<int> _getBytes;

		public enum DataFileType
		{
			Leak,
			Produce
		}

		public void Show(Game game, Robot robot, DataFileType type, Func<int> getBytes)
		{
            _game = game;
			Robot = robot;
			Type = type;
			_getBytes = getBytes;
			_disposable = new CompositeDisposable();

			Observable.EveryUpdate().Merge(Observable.Return(0L))
				.Subscribe(_ => ((RectTransform)transform).sizeDelta = new Vector2(Mathf.FloorToInt(getBytes() * game.Template.MemoryIndicationScale), ((RectTransform)transform).sizeDelta.y))
				.AddTo(_disposable);

            if (_button != null && type == DataFileType.Leak)
                _button.OnClickAsObservable()
                    .Subscribe(_ => robot.ClearLeaks())
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
			_draggedDataFile.Show(_game, Robot, Type, _getBytes);
		}

		public void OnEndDrag([NotNull] PointerEventData eventData)
		{
			if (Type == DataFileType.Produce)
				eventData.pointerEnter?.GetComponent<IDropAccepter<DataFileView>>()?.Accept(this);

			if (eventData.pointerEnter == null && Type == DataFileType.Leak)
				Robot.ClearLeaks();

			_draggedDataFile.Dispose();
		}
	}
}
