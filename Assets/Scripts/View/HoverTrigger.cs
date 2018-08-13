using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace View
{
	public class HoverTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public ReactiveProperty<bool> Hovered = new ReactiveProperty<bool>();

		public void OnPointerEnter([NotNull] PointerEventData eventData) => Hovered.Value = true;
		public void OnPointerExit([NotNull] PointerEventData eventData) => Hovered.Value = false;
	}
}
