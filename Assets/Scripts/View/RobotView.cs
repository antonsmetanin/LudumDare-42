using System;
using Model;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace View
{
	public class RobotView : MonoBehaviour, IDisposable
	{
		[SerializeField] private ProgramView _programViewTemplate;
        [SerializeField] private RectTransform _memoryTransform;

        [SerializeField] private Image _memoryBorder;

        private CompositeDisposable _disposable;

        public Robot Robot;

		public void Show(Robot robot, Transform robotTransform, Camera mainCamera)
		{
            _disposable = new CompositeDisposable();

            gameObject.SetActive(true);

            Robot = robot;

			Observable.EveryUpdate()
				.Subscribe(_ => transform.position = mainCamera.WorldToScreenPoint(robotTransform.position) + new Vector3(0f, 100f))
				.AddTo(_disposable);

			robot.MemorySize
				.Subscribe(x => _memoryTransform.sizeDelta = new Vector2(x, ((RectTransform)transform).sizeDelta.y))
				.AddTo(_disposable);

			robot.Programs
				.CreateView(_programViewTemplate, _memoryTransform, (view, program) => view.Show(program))
				.AddTo(_disposable);
		}

        public void Dispose()
		{
            gameObject.SetActive(false);
			_disposable.Dispose();
		}

        public void OnAccept(ProgramView programView, bool simulate = false)
        {
            var result = Robot.InstallProgram(programView.Program, simulate);

            _memoryBorder.color = result.Error != null ? Color.red : Color.white;
        }
    }
}
