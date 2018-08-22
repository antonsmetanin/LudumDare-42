using System;
using JetBrains.Annotations;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View
{
	public class DraggedDataFileView : MonoBehaviour
	{
		public Robot Robot;

        [SerializeField] private Color _leakColor;
        [SerializeField] private Color _dataColor;

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Image _image;

        public void Show(Game game, Robot robot, DataFileView.DataFileType type, Func<int> getBytes)
		{
			Robot = robot;
			((RectTransform)transform).sizeDelta = new Vector2((getBytes() * game.Template.MemoryIndicationScale), ((RectTransform)transform).sizeDelta.y);
			_label.text = type == DataFileView.DataFileType.Leak ? "leak" : "data";
            _image.color = type == DataFileView.DataFileType.Leak ? _leakColor : _dataColor;
        }

        public void Drag([NotNull] PointerEventData eventData) => transform.position = eventData.position;
        public void Dispose() => Destroy(gameObject);
    }
}
