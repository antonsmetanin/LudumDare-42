using System;
using JetBrains.Annotations;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace View
{
	public class DraggedProgramView : MonoBehaviour, IDisposable
	{
        public Program Program;

        [SerializeField] private Image _colorPanel;
        [SerializeField] private TextMeshProUGUI _nameLabel;

		public void Show(Game game, Program program)
		{
            Program = program;
			((RectTransform)transform).sizeDelta = new Vector2(Mathf.FloorToInt(program.CurrentVersion.Value.MemorySize * game.Template.MemoryIndicationScale), ((RectTransform)transform).sizeDelta.y);
            _nameLabel.text = program.Template.Name;
            _colorPanel.color = program.Template.Color;
		}

        public void Dispose() => Destroy(gameObject);

        public void Drag([NotNull] PointerEventData eventData)
		{
			transform.localPosition += (Vector3)eventData.delta;
		}
	}
}
