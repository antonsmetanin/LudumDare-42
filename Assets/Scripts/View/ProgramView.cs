using System;
using Model;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
	public class ProgramView : MonoBehaviour, IDisposable
	{
		[SerializeField] private TextMeshProUGUI _nameLabel;

		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		public void Show(Program program)
		{
			program.Size
				.Subscribe(x => ((RectTransform)transform).sizeDelta = new Vector2(x, ((RectTransform)transform).sizeDelta.y))
				.AddTo(_disposable);

			GetComponent<Image>().color = program.Template.Color;
			_nameLabel.color = program.Template.TextColor;
			_nameLabel.text = program.Template.Name;
		}

		public void Dispose()
		{
			_disposable.Dispose();
		}
	}
}