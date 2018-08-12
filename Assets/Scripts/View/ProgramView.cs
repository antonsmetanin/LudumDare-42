using System;
using JetBrains.Annotations;
using Model;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View
{
	public class ProgramView : MonoBehaviour, IDisposable, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[SerializeField] private TextMeshProUGUI _nameLabel;
		[SerializeField] private DraggedProgramView _draggedProgramTemplate;

        private CompositeDisposable _disposable;

		private DraggedProgramView _draggedProgramView;
		public Program Program;

		public void Show(Program program)
		{
            _disposable = new CompositeDisposable();

            GetComponent<Image>().color = program.Template.Color;
			_nameLabel.faceColor = program.Template.TextColor;

			program.Size.Subscribe(size => ((RectTransform)transform).sizeDelta = new Vector2(size, ((RectTransform)transform).sizeDelta.y)).AddTo(_disposable);
			program.Name.Subscribe(x => _nameLabel.text = x).AddTo(_disposable);

			Program = program;
		}

        public void Dispose() => _disposable.Dispose();

		public void OnDrag([NotNull] PointerEventData eventData)
		{
			_draggedProgramView.Drag(eventData);

            eventData.pointerEnter?.GetComponent<RobotView>()?.OnAccept(this, simulate: true);
		}

		public void OnBeginDrag([NotNull] PointerEventData eventData)
		{
			_draggedProgramView = Instantiate(_draggedProgramTemplate);
			_draggedProgramView.transform.SetParent(GetComponentInParent<Canvas>().transform, worldPositionStays: false);
			_draggedProgramView.transform.position = transform.position;
			_draggedProgramView.Show(Program);
		}

		public void OnEndDrag([NotNull] PointerEventData eventData)
		{
            eventData.pointerEnter?.GetComponent<RobotView>()?.OnAccept(this);

            _draggedProgramView.Dispose();
		}
	}
}