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
		[SerializeField] private BetterButton _upgradeButton;
		[SerializeField] private BetterButton _patchButton;
		[SerializeField] private TextMeshProUGUI _characteristicsLabel;
		[SerializeField] private RectTransform _sizeIndicator;
		[SerializeField] private TextMeshProUGUI _sizeIndicatorLabel;
		[SerializeField] private TextMeshProUGUI _description;

        private CompositeDisposable _disposable;

		public void Show(Program program, Game game, ReactiveProperty<IOperationResult> pendingAction)
		{
            _disposable = new CompositeDisposable();

            _programView.Show(game, program);
			_programView.AddTo(_disposable);

			_description.text = program.CurrentVersion.Value.Description;

			program.CurrentVersion.Subscribe(_ => _upgradeButton.Label.text = "v" + (program.GetCurrentVersionIndex() + 2));

			_upgradeButton.Button.OnClickAsObservable().Subscribe(_ => program.Upgrade(game.GameProgress)).AddTo(_disposable);
			_patchButton.Button.OnClickAsObservable().Subscribe(_ => program.Patch(game.GameProgress)).AddTo(_disposable);

			program.CanUpgrade(game.GameProgress).Subscribe(upgradeResult =>
			{
				_upgradeButton.SetInteractable(upgradeResult.Error == null);
				_upgradeButton.gameObject.SetActive(!(upgradeResult.Error is Program.FinalVersionReachedError));
			}).AddTo(_disposable);

			program.CanPatch(game.GameProgress).Subscribe(patchResult =>
			{
				_patchButton.SetInteractable(patchResult.Error == null);
				_patchButton.gameObject.SetActive(!(patchResult.Error is Program.FinalVersionReachedError));
			}).AddTo(_disposable);

            Observable.CombineLatest
                (program.LeakBytesPerSecond, program.ProduceBytesPerSecond, pendingAction,
                (leak, produce, pending)
                    => pending is Program.IProgramChangeOperation programChange
                    && programChange.Program == program
                        ? (produce: programChange.Produce, leak: programChange.Leak)
                        : (produce: produce, leak: leak))
				.Subscribe(x => _characteristicsLabel.text = $"<color=#FBDF6A>produce</color> {x.produce} byte/s    <color=#BD306C>leak</color> {x.leak} byte/s")
                .AddTo(_disposable);

            Observable.CombineLatest
                (_programView.GetComponent<HoverTrigger>().Hovered, pendingAction,
                (hovered, pending)
                    => pending is Program.UpgradeResult changeOperation && changeOperation.Program == program ? changeOperation.Version.Description
                    : hovered ? program.CurrentVersion.Value.Description
                    : program.Template.Versions[0].Description)
                .Subscribe(description => _description.text = description)
                .AddTo(_disposable);

			program.MemorySize.Subscribe(size =>
			{
				_sizeIndicator.sizeDelta = new Vector2(Mathf.FloorToInt(size * game.Template.MemoryIndicationScale), _sizeIndicator.sizeDelta.y);
				_sizeIndicatorLabel.text = $"  {size / 1024}kb   ";
			}).AddTo(_disposable);

			_upgradeButton.gameObject.GetComponent<HoverTrigger>().Hovered
				.Subscribe(hovered => pendingAction.Value = hovered ? program.Upgrade(game.GameProgress, simulate: true).Value : null)
				.AddTo(_disposable);

			_patchButton.gameObject.GetComponent<HoverTrigger>().Hovered
				.Subscribe(hovered => pendingAction.Value = hovered ? program.Patch(game.GameProgress, simulate: true).Value : null)
				.AddTo(_disposable);
		}

		public void Dispose() => _disposable.Dispose();
	}
}
