using Model;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class DashboardRobotView : MonoBehaviour, IDisposable
    {
        [SerializeField] private Button _selectButton;
        [SerializeField] private MemoryUpgradeView _memoryUpgradeViewTemplate;
        [SerializeField] private RectTransform _memoryUpgradeParent;

        private CompositeDisposable _disposable;

        public void Show(Game game, Robot robot)
        {
            _disposable = new CompositeDisposable();

            _selectButton.OnClickAsObservable()
                .Subscribe(_ => game.SelectedRobot.Value = robot)
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}