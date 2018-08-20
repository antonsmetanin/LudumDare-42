using Model;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class RobotIcon : MonoBehaviour, IDisposable
    {
        private CompositeDisposable _disposable;

        [SerializeField] private Image _outOfMemoryIcon;

        public void Show(Game game, Robot robot, Camera mainCamera)
        {
            _disposable = new CompositeDisposable();

            var outOfMemory = robot.Status
                .Select(x => x == Robot.RobotStatus.OutOfMemory)
                .DistinctUntilChanged();

            outOfMemory
                .Subscribe(x => _outOfMemoryIcon.gameObject.SetActive(x))
                .AddTo(_disposable);

            outOfMemory
                .Select(x => x ? Observable.EveryUpdate().Merge(Observable.Return(0L)) : Observable.Empty<long>())
                .Switch()
                .Subscribe(_ => transform.position = mainCamera.WorldToScreenPoint(robot.Transform.position + new Vector3(0f, 3.5f)))
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}