using Model;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
    public class DashboardRobotView : MonoBehaviour, IDisposable
    {
        [SerializeField] private Button _selectButton;
        [SerializeField] private BoughtMemoryUpgradeView _memoryUpgradeViewTemplate;
        [SerializeField] private RectTransform _memoryUpgradeParent;
        [SerializeField] private Button _buyUpgradeButton;

        private CompositeDisposable _disposable;

        public void Show(Game game, Robot robot, ReactiveProperty<IOperationResult> pendingAction)
        {
            _disposable = new CompositeDisposable();

            _selectButton.OnClickAsObservable()
                .Subscribe(_ => game.SelectedRobot.Value = robot)
                .AddTo(_disposable);

            new CollectionFromInt(robot.MemoryUpgrades)
                .CreateView(_memoryUpgradeViewTemplate, _memoryUpgradeParent, (view, index) => { })
                .AddTo(_disposable);

            robot.CanUpgradeMemory()
                .Subscribe(canUpgrade => {
                    _buyUpgradeButton.interactable = canUpgrade.Error == null;
                    _buyUpgradeButton.gameObject.SetActive(!(canUpgrade.Error is Robot.MaxUpgradesReachedError));
                })
                .AddTo(_disposable);

            _buyUpgradeButton.OnClickAsObservable()
                .Subscribe(_ => robot.UpgradeMemory())
                .AddTo(_disposable);

            _buyUpgradeButton.GetComponent<HoverTrigger>().Hovered
                .Subscribe(hovered => pendingAction.Value = hovered ? robot.UpgradeMemory(simulate: true).Value : null)
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}