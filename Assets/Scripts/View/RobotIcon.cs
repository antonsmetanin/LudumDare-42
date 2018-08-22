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

        [SerializeField] private Button _outOfMemoryButton;

        public void Show(Game game, Robot robot, Camera mainCamera)
        {
            _disposable = new CompositeDisposable();

            var outOfMemory = Observable.CombineLatest(robot.Status, game.SelectedRobot,
                (status, selectedRobot) => status == Robot.RobotStatus.OutOfMemory && selectedRobot != robot)
                .DistinctUntilChanged();

            outOfMemory
                .Subscribe(x => _outOfMemoryButton.gameObject.SetActive(x))
                .AddTo(_disposable);

            outOfMemory
                .Select(x => x ? Observable.EveryUpdate().Merge(Observable.Return(0L)) : Observable.Empty<long>())
                .Switch()
                .Subscribe(_ => transform.position = mainCamera.WorldToScreenPoint(robot.Transform.position + new Vector3(0f, 3.5f)))
                .AddTo(_disposable);

            _outOfMemoryButton.OnClickAsObservable()
                .Subscribe(_ => game.SelectedRobot.Value = robot)
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}