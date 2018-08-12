using System.Linq;
using Data;
using Model;
using UniRx;
using UnityEngine;
using View;

public class GameController : MonoBehaviour
{
    private CompositeDisposable _disposable;

	[SerializeField] private RobotView _robotView;

    [SerializeField] private Dashboard _dashboard;

	[SerializeField] private GameProgressTemplate _defaultGameProgress;
	[SerializeField] private ProgramTemplate[] _defaultProgramTemplates;

	[SerializeField] private RobotController _robotControllerTemplate;
	[SerializeField] private Transform _robotSpawnPosition;

	private void Start()
	{
        _disposable = new CompositeDisposable();

		var game = new Game(_defaultGameProgress, _defaultProgramTemplates);

		game.SelectedRobot.Subscribe(robot =>
		{
			if (robot != null && !_robotView.gameObject.activeSelf)
				_robotView.Show(game.Robots[0], robot.Transform, Camera.main);
			else if (robot == null && _robotView.gameObject.activeSelf)
				_robotView.Dispose();
		}).AddTo(_disposable);

        _dashboard.Show(game.GameProgress);
        _dashboard.AddTo(_disposable);

		game.Robots.ObserveAdd().Subscribe(addRobot =>
		{
			var robotController = Instantiate(_robotControllerTemplate);
			robotController.RobotModel = addRobot.Value;
			robotController.transform.position = _robotSpawnPosition.position;
		}).AddTo(_disposable);

		game.Robots.Add(new Robot
		{
			MemorySize = new ReactiveProperty<int>(404),
			Programs = _defaultProgramTemplates.Select(template => new Program(template)).ToReactiveCollection()
		});

		Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.A)).Subscribe(_ => game.GameProgress.DataCollected.Value++);

		Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Alpha1)).Subscribe(_ => game.SelectedRobot.Value = game.Robots[0]);
		Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Alpha2)).Subscribe(_ => game.SelectedRobot.Value = null);
	}

    //private void OnDestroy() => _disposable.Dispose();
}