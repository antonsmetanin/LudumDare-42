using System;
using Model;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
	public class DataIndicator : MonoBehaviour, IDisposable, IDropAccepter<DataFileView>
	{
		[SerializeField] private Image _currentDataBar;
		[SerializeField] private Image _dataAfterSubstractionBar;

		[SerializeField] private TextMeshProUGUI _currentDataLabel;
		[SerializeField] private TextMeshProUGUI _dataAfterSubstractionLabel;

		private CompositeDisposable _disposable;

		public void Show(Game game, ReactiveProperty<IOperationResult> pendingAction)
		{
			_disposable = new CompositeDisposable();

			game.GameProgress.DataCollected
				.Subscribe(data =>
				{
                    _currentDataBar.fillAmount = (float)data / game.Template.MaxData;
					_currentDataLabel.text = $"{data} bytes";
				})
				.AddTo(_disposable);

			game.GameProgress.DataCollected.CombineLatest(pendingAction,
					(data, action) => pendingAction.Value is Program.IPricedOperation pricedOperation ? data - pricedOperation.Price : data)
				.Subscribe(data => _dataAfterSubstractionBar.fillAmount = (float)data / game.Template.MaxData)
				.AddTo(_disposable);

			pendingAction.Select(action => action is Program.IPricedOperation pricedOperation ? pricedOperation.Price : 0)
				.Subscribe(price => _dataAfterSubstractionLabel.text = price > 0 ? $"-{price}b" : "")
				.AddTo(_disposable);
		}

		public void Dispose() => _disposable.Dispose();

		public void Accept(DataFileView dataFileView, bool simulate)
		{
			if (simulate)
				return;

			if (dataFileView.Type == DataFileView.DataFileType.Produce)
				dataFileView.Robot.UploadData();
		}
	}
}