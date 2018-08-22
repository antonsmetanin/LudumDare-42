using System;
using JetBrains.Annotations;
using Model;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace View
{
	public class ProgramView : MonoBehaviour, IDisposable, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[SerializeField] private TextMeshProUGUI _nameLabel;
		[SerializeField] private DraggedProgramView _draggedProgramTemplate;
        [SerializeField] private Button _button;
        [SerializeField] private Image _image;

        private CompositeDisposable _disposable;

		private DraggedProgramView _draggedProgramView;
        private Game _game;
		public Program Program;

		public void Show(Game game, Program program)
		{
            _disposable = new CompositeDisposable();

            _game = game;
            Program = program;

            _image.color = program.Template.Color;
			_nameLabel.faceColor = program.Template.TextColor;

			program.MemorySize
                .Subscribe(size => ((RectTransform)transform).sizeDelta = new Vector2(Mathf.FloorToInt(size * game.Template.MemoryIndicationScale), ((RectTransform)transform).sizeDelta.y))
                .AddTo(_disposable);

			program.Name
                .Subscribe(x => _nameLabel.text = x)
                .AddTo(_disposable);

            if (_button != null)
                _button.OnClickAsObservable()
                    .Subscribe(_ => program.Uninstall())
                    .AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();

		public void OnDrag([NotNull] PointerEventData eventData)
		{
			_draggedProgramView.Drag(eventData);

            eventData.pointerEnter?.GetComponent<IDropAccepter<ProgramView>>()?.Accept(this, simulate: true);
		}

		public void OnBeginDrag([NotNull] PointerEventData eventData)
		{
			_draggedProgramView = Instantiate(_draggedProgramTemplate);
			_draggedProgramView.transform.SetParent(GetComponentInParent<Canvas>().transform, worldPositionStays: false);
			_draggedProgramView.transform.position = transform.position;
			_draggedProgramView.Show(_game, Program);
		}

		public void OnEndDrag([NotNull] PointerEventData eventData)
		{
            eventData.pointerEnter?.GetComponent<IDropAccepter<ProgramView>>()?.Accept(this);

			if (eventData.pointerEnter == null)
				Program.Uninstall();

            _draggedProgramView.Dispose();
		}
	}
}