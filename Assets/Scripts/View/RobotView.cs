using System;
using Model;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
	public class RobotView : MonoBehaviour, IDisposable, IDropAccepter<ProgramView>
	{
		[SerializeField] private ProgramView _programViewTemplate;
        [SerializeField] private RectTransform _memoryTransform;
		[SerializeField] private TextMeshProUGUI _dropModulesLabel;

        [SerializeField] private Image _memoryBorder;

		[SerializeField] private DataFileView _leakedDataView;
		[SerializeField] private DataFileView _producedDataView;

        private CompositeDisposable _disposable;

        public Robot Robot;

		public void Show(Robot robot, Transform robotTransform, Camera mainCamera)
		{
            _disposable = new CompositeDisposable();

            gameObject.SetActive(true);

            Robot = robot;

			Observable.EveryUpdate().Merge(Observable.Return(0l))
				.Subscribe(_ => transform.position = mainCamera.WorldToScreenPoint(robotTransform.position) + new Vector3(0f, 100f))
				.AddTo(_disposable);

			robot.MemorySize
				.Subscribe(x => ((RectTransform)transform).sizeDelta = new Vector2(x + 4, ((RectTransform)transform).sizeDelta.y))
				.AddTo(_disposable);

			robot.Programs
				.CreateView(_programViewTemplate, _memoryTransform, (view, program) => view.Show(program))
				.AddTo(_disposable);

			_leakedDataView.Show(robot, DataFileView.DataFileType.Leak, robot.GetLeakedBytes);
			_leakedDataView.AddTo(_disposable);

			_producedDataView.Show(robot, DataFileView.DataFileType.Produce, robot.GetProducedBytes);
			_producedDataView.AddTo(_disposable);

			robot.Programs.CountProperty()
				.Subscribe(programsCount => _dropModulesLabel.gameObject.SetActive(programsCount == 0))
				.AddTo(_disposable);
		}

        public void Dispose()
		{
            gameObject.SetActive(false);
			_disposable.Dispose();
		}

        public void Accept(ProgramView programView, bool simulate = false)
        {
            var result = Robot.InstallProgram(programView.Program, simulate);

            _memoryBorder.color = result.Error != null ? Color.red : Color.white;
        }
    }
}
