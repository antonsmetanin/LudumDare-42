using System;
using JetBrains.Annotations;
using Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace View
{
	public class DraggedProgramView : MonoBehaviour, IDisposable
	{
        public Program Program;

		public void Show(Game game, Program program)
		{
            Program = program;
			((RectTransform)transform).sizeDelta = new Vector2(Mathf.FloorToInt(program.CurrentVersion.Value.MemorySize * game.Template.MemoryIndicationScale), ((RectTransform)transform).sizeDelta.y);
            GetComponentInChildren<TMPro.TextMeshProUGUI>().text = program.Template.Name;
		}

        public void Dispose() => Destroy(gameObject);

        public void Drag([NotNull] PointerEventData eventData)
		{
			transform.localPosition += (Vector3)eventData.delta;
		}
	}
}
