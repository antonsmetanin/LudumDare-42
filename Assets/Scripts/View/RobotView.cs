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
		[SerializeField] private TextMeshProUGUI _statusLabel;
        [SerializeField] private Button _uploadButton;

        [SerializeField] private Image _memoryBorder;

		[SerializeField] private DataFileView _leakedDataView;
		[SerializeField] private DataFileView _producedDataView;

        [SerializeField] private OffScreenIndicator _offScreenIndicator;

        private CompositeDisposable _disposable;

        public Robot Robot;

		public void Show(Game game, Robot robot, Camera mainCamera)
		{
            _disposable = new CompositeDisposable();

            gameObject.SetActive(true);

            Robot = robot;

			Observable.EveryUpdate().Merge(Observable.Return(0L))
				.Subscribe(_ => transform.position = mainCamera.WorldToScreenPoint(robot.Transform.position + new Vector3(0, 3.5f)))
				.AddTo(_disposable);

			robot.MemorySize
				.Subscribe(x => ((RectTransform)transform).sizeDelta = new Vector2(Mathf.FloorToInt(x * game.Template.MemoryIndicationScale) + 8, ((RectTransform)transform).sizeDelta.y))
				.AddTo(_disposable);

			robot.Programs
				.CreateView(_programViewTemplate, _memoryTransform, (view, program) => view.Show(game, program))
				.AddTo(_disposable);

			robot.Status
				.Subscribe(status => _statusLabel.text = status == Robot.RobotStatus.OutOfMemory ? "Out Of Memory Exception"
                                                       : status == Robot.RobotStatus.BootError ? "Boot Error"
                                                       : "OK")
				.AddTo(_disposable);

			_leakedDataView.Show(game, robot, DataFileView.DataFileType.Leak, () => robot.LeakedBytes.Value);
			_leakedDataView.AddTo(_disposable);

			_producedDataView.Show(game, robot, DataFileView.DataFileType.Produce, () => robot.ProducedBytes.Value);
			_producedDataView.AddTo(_disposable);

            _offScreenIndicator.Show(mainCamera, robot.Transform, Screen.safeArea);
            _offScreenIndicator.AddTo(_disposable);

            robot.CanUploadData()
                .Subscribe(canUpload =>
                {
                    _uploadButton.GetComponentInChildren<TextMeshProUGUI>().text = canUpload.Error is Robot.UploadIsAlreadyRunningError ? "Uploading..." : "Upload";
                    _uploadButton.interactable = canUpload.Error == null;
                })
                .AddTo(_disposable);

            _uploadButton.OnClickAsObservable()
                .Subscribe(_ => robot.CollectData())
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
