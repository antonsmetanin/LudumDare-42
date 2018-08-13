﻿using System;
using Model;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
	public class DataIndicator : MonoBehaviour, IDisposable, IDropAccepter<Program>
	{
		[SerializeField] private Image _currentDataBar;
		[SerializeField] private Image _dataAfterSubstractionBar;

		[SerializeField] private TextMeshProUGUI _currentDataLabel;
		[SerializeField] private TextMeshProUGUI _dataAfterSubstractionLabel;

		private CompositeDisposable _disposable;

		public void Show(GameProgress gameProgress, ReactiveProperty<IOperationResult> pendingAction)
		{
			_disposable = new CompositeDisposable();

			gameProgress.DataCollected
				.Subscribe(data =>
				{
					_currentDataBar.fillAmount = (float)data / gameProgress.Template.MaxData;
					_currentDataLabel.text = $"{data} bytes";
				})
				.AddTo(_disposable);

			gameProgress.DataCollected.CombineLatest(pendingAction,
					(data, action) => pendingAction.Value is Program.IPricedOperation pricedOperation ? data - pricedOperation.Price : data)
				.Subscribe(data => _dataAfterSubstractionBar.fillAmount = (float)data / gameProgress.Template.MaxData)
				.AddTo(_disposable);

			pendingAction.Select(action => action is Program.IPricedOperation pricedOperation ? pricedOperation.Price : 0)
				.Subscribe(price => _dataAfterSubstractionLabel.text = price > 0 ? $"-{price}b" : "")
				.AddTo(_disposable);
		}

		public void Dispose() => _disposable.Dispose();

		public void Accept(Program program, bool simulate)
		{
		}
	}
}