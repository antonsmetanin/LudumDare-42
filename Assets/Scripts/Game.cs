using System.Linq;
using Data;
using Model;
using UniRx;
using UnityEngine;
using View;

public class Game : MonoBehaviour
{
	private readonly CompositeDisposable _disposable = new CompositeDisposable();

	[SerializeField] private RobotView _robotViewTemplate;
	[SerializeField] private RectTransform _memoryViewParent;

    [SerializeField] private Dashboard _dashboard;
    [SerializeField] private Transform _cube;

	[SerializeField] private GameProgressTemplate _defaultGameProgress;
	[SerializeField] private ProgramTemplate[] _defaultProgramTemplates;

	private void Start()
	{
		var memory = new Robot
		{
			Size = new ReactiveProperty<int>(404),
			Programs = _defaultProgramTemplates.Select(template => new Program(template)).ToReactiveCollection()
		};

        var gameProgress = new GameProgress(_defaultGameProgress);

		var memoryView = Instantiate(_robotViewTemplate);
		memoryView.transform.SetParent(_memoryViewParent, worldPositionStays: false);
		memoryView.Show(memory, _cube, Camera.main);
		memoryView.AddTo(_disposable);

        _dashboard.Show(gameProgress);
        _dashboard.AddTo(_disposable);
	}

    //private void OnDestroy() => _disposable.Dispose();
}