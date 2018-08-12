using System;
using Model;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
	public class DashboardProgramView : MonoBehaviour, IDisposable
	{
		[SerializeField] private ProgramView _programView;
		[SerializeField] private Button _upgradeButton;
		[SerializeField] private Button _patchButton;
		[SerializeField] private TextMeshProUGUI _characteristicsLabel;
		[SerializeField] private RectTransform _sizeIndicator;
		[SerializeField] private Text _sizeIndicatorLabel;

		private readonly CompositeDisposable _disposable = new CompositeDisposable();

		public void Show(Program program)
		{
			_programView.Show(program);
			_programView.AddTo(_disposable);

			program.CurrentVersion.Subscribe(_ => _upgradeButton.GetComponentInChildren<Text>().text = "v" + (program.GetCurrentVersionIndex() + 2));

			_upgradeButton.OnClickAsObservable().Subscribe(_ => program.Upgrade()).AddTo(_disposable);
			_patchButton.OnClickAsObservable().Subscribe(_ => program.Patch()).AddTo(_disposable);

			program.CanUpgrade.Subscribe(canUpgrade => _upgradeButton.gameObject.SetActive(canUpgrade)).AddTo(_disposable);
			program.CanPatch.Subscribe(canPatch => _patchButton.gameObject.SetActive(canPatch)).AddTo(_disposable);

			program.LeakSpeed.CombineLatest(program.ProduceSpeed,
				(leak, produce) => $"<color=red>produce</color> {produce} byte/s    <color=purple>leak</color> {leak} byte/s")
				.Subscribe(x => _characteristicsLabel.text = x).AddTo(_disposable);

			program.Size.Subscribe(size =>
			{
				_sizeIndicator.sizeDelta = new Vector2(size, _sizeIndicator.sizeDelta.y);
				_sizeIndicatorLabel.text = $"  {size}kb  ";
			}).AddTo(_disposable);
		}

		public void Dispose() => _disposable.Dispose();
	}
}
