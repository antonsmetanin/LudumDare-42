using System;
using JetBrains.Annotations;
using Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace View
{
	public class DraggedDataFileView : MonoBehaviour
	{
		public Robot Robot;

		public void Show(Game game, Robot robot, DataFileView.DataFileType type, Func<int> getBytes)
		{
			Robot = robot;
			((RectTransform)transform).sizeDelta = new Vector2((getBytes() * game.Template.MemoryIndicationScale), ((RectTransform)transform).sizeDelta.y);
			GetComponentInChildren<TMPro.TextMeshProUGUI>().text = type == DataFileView.DataFileType.Leak ? "Leak" : "Produce";
		}

		public void Dispose() => Destroy(gameObject);

		public void Drag([NotNull] PointerEventData eventData)
		{
            transform.position = eventData.position;
        }
	}
}
