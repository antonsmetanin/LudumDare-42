using Model;
using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
    public class DashboardRobotView : MonoBehaviour, IDisposable
    {
        [SerializeField] private Button _selectButton;
        [SerializeField] private MemoryUpgradeView _memoryUpgradeViewTemplate;
        [SerializeField] private RectTransform _memoryUpgradeParent;

        private CompositeDisposable _disposable;

        public void Show(Game game, Robot robot, ReactiveProperty<IOperationResult> pendingAction)
        {
            _disposable = new CompositeDisposable();

            _selectButton.OnClickAsObservable()
                .Subscribe(_ => game.SelectedRobot.Value = robot)
                .AddTo(_disposable);

            Enumerable.Range(0, 2).CreateView(_memoryUpgradeViewTemplate, _memoryUpgradeParent, (view, index) => view.Show(robot, index, pendingAction))
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}