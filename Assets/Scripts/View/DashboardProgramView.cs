using System;
using Model;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

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

        private CompositeDisposable _disposable;

		public void Show(Program program, GameProgress gameProgress, ReactiveProperty<IOperationResult> pendingAction)
		{
            _disposable = new CompositeDisposable();

            _programView.Show(program);
			_programView.AddTo(_disposable);

			program.CurrentVersion.Subscribe(_ => _upgradeButton.GetComponentInChildren<Text>().text = "v" + (program.GetCurrentVersionIndex() + 2));

			_upgradeButton.OnClickAsObservable().Subscribe(_ => program.Upgrade(gameProgress)).AddTo(_disposable);
			_patchButton.OnClickAsObservable().Subscribe(_ => program.Patch(gameProgress)).AddTo(_disposable);

			program.CanUpgrade(gameProgress).Subscribe(upgradeResult =>
			{
				_upgradeButton.interactable = upgradeResult.Error == null;
				_upgradeButton.gameObject.SetActive(!(upgradeResult.Error is Program.FinalVersionReachedError));
			}).AddTo(_disposable);

			program.CanPatch(gameProgress).Subscribe(patchResult =>
			{
				_patchButton.interactable = patchResult.Error == null;
				_patchButton.gameObject.SetActive(!(patchResult.Error is Program.FinalVersionReachedError));
			}).AddTo(_disposable);

			program.LeakBytesPerSecond.CombineLatest(program.ProduceBytesPerSecond,
				(leak, produce) => $"<color=#FBDF6A>produce</color> {produce} byte/s    <color=#BD306C>leak</color> {leak} byte/s")
				.Subscribe(x => _characteristicsLabel.text = x).AddTo(_disposable);

			program.MemorySize.Subscribe(size =>
			{
				_sizeIndicator.sizeDelta = new Vector2(size, _sizeIndicator.sizeDelta.y);
				_sizeIndicatorLabel.text = $"  {size}kb  ";
			}).AddTo(_disposable);

			_upgradeButton.gameObject.GetComponent<HoverTrigger>().Hovered
				.Subscribe(hovered => pendingAction.Value = hovered ? program.Upgrade(gameProgress, simulate: true).Value : null)
				.AddTo(_disposable);

			_patchButton.gameObject.GetComponent<HoverTrigger>().Hovered
				.Subscribe(hovered => pendingAction.Value = hovered ? program.Patch(gameProgress, simulate: true).Value : null)
				.AddTo(_disposable);
		}

		public void Dispose() => _disposable.Dispose();
	}
}
