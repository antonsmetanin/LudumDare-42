using System;
using Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace View
{
	public class DraggedProgramView : MonoBehaviour, IDisposable
	{
		public void Show(Program program)
		{
			((RectTransform)transform).sizeDelta = new Vector2(program.CurrentVersion.Value.Size, ((RectTransform)transform).sizeDelta.y);
            GetComponentInChildren<TMPro.TextMeshProUGUI>().text = program.Template.Name;
		}

        public void Dispose() => Destroy(gameObject);

        public void Drag(PointerEventData eventData)
		{
			transform.localPosition += (Vector3)eventData.delta;

            Debug.LogWarning("dragging over " +
                (eventData.pointerEnter != null
                ? UnityEditor.AnimationUtility.CalculateTransformPath(eventData.pointerEnter.transform, null)
                : "null"));
		}
	}
}
