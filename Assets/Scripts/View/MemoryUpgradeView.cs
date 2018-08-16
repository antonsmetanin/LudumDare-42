using Model;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
    public class MemoryUpgradeView : MonoBehaviour, IDisposable
    {
        [SerializeField] private Button _buyUpgradeButton;

        private CompositeDisposable _disposable;

        public void Show(Robot robot, int memoryUpgradeIndex, ReactiveProperty<IOperationResult> pendingAction)
        {
            _disposable = new CompositeDisposable();

            robot.CanUpgradeMemory(memoryUpgradeIndex)
                .Subscribe(canUpgrade => _buyUpgradeButton.interactable = canUpgrade.Error == null)
                .AddTo(_disposable);

            _buyUpgradeButton.OnClickAsObservable()
                .Subscribe(_ => robot.UpgradeMemory(memoryUpgradeIndex))
                .AddTo(_disposable);

            _buyUpgradeButton.GetComponent<HoverTrigger>().Hovered
                .Subscribe(hovered => pendingAction.Value = hovered ? robot.UpgradeMemory(memoryUpgradeIndex, simulate: true).Value : null)
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}