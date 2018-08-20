using Data;
using Model;
using UniRx;
using UnityEngine;
using Utils;
using View;

public class GameController : MonoBehaviour
{
    private CompositeDisposable _disposable;

	[SerializeField] private RobotView _robotView;

    [SerializeField] private Dashboard _dashboard;

    [SerializeField] private RobotIcon _robotIconTemplate;
    [SerializeField] private RectTransform _robotIconsParent;

    [SerializeField] private GameProgressTemplate _defaultGameProgress;

	[SerializeField] private RobotController _robotControllerTemplate;
	[SerializeField] private Transform _robotSpawnPosition;
	[SerializeField] private Transform _robotTargetPosition;

    [SerializeField] private GameTemplate _gameTemplate;

	private void Start()
	{
        _disposable = new CompositeDisposable();

		var game = new Game(_gameTemplate, new GameProgress(_defaultGameProgress));

		MainApplication.Instance.Game = game;

		game.SelectedRobot.Subscribe(robot =>
		{
            if (_robotView.gameObject.activeSelf)
                _robotView.Dispose();

            if (robot != null)
				_robotView.Show(game, robot, Camera.main);
		}).AddTo(_disposable);

        _dashboard.Show(game);
        _dashboard.AddTo(_disposable);

		game.Robots.ObserveAdd().Subscribe(addRobot =>
		{
			var robotController = Instantiate(_robotControllerTemplate);
			robotController.RobotModel = addRobot.Value;
			robotController.Game = game;
			robotController.RobotModel.Transform = robotController.transform;
			robotController.transform.position = _robotSpawnPosition.position;
			robotController.StartCoroutine(robotController.CO_Spawn(_robotTargetPosition.position));
			BotsSurvivors.Survived = game.Robots.Count;
			
		}).AddTo(_disposable);

		var rob = new Robot(game.Template.RobotTemplate, game);
		game.Robots.Add(rob);

		rob.LeakedBytes.Value += 7000;
		rob.ProducedBytes.Value = 768;

        game.Robots
            .CreateView(_robotIconTemplate, _robotIconsParent, (robotIcon, robot) => robotIcon.Show(game, robot, Camera.main))
            .AddTo(_disposable);
	}

    //private void OnDestroy() => _disposable.Dispose();
}