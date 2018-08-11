using System.Linq;
using Data;
using Model;
using UniRx;
using UnityEngine;
using View;

public class Game : MonoBehaviour
{
	private readonly CompositeDisposable _disposable = new CompositeDisposable();

	[SerializeField] private MemoryView _memoryViewTemplate;
	[SerializeField] private RectTransform _memoryViewParent;


	[SerializeField] private ProgramPalette _paletteTemplate;
	[SerializeField] private RectTransform _paletteParent;

	[SerializeField] private Transform _cube;

	[SerializeField] private ProgramTemplate[] _defaultProgramTemplates;

	private void Start()
	{
		var memory = new Memory
		{
			Size = new IntReactiveProperty(404),
			Programs = _defaultProgramTemplates.Select(template => new Program
			{
				Size = new IntReactiveProperty(template.InitialSize),
				Template = template
			}).ToReactiveCollection()
		};

		Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.W)).Subscribe(_ => memory.Programs[0].Size.Value++).AddTo(_disposable);
		Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.S)).Subscribe(_ => memory.Programs[0].Size.Value--).AddTo(_disposable);
		Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Alpha1)).Subscribe(_ => memory.Programs.Add(new Program { Size = new IntReactiveProperty(20) })).AddTo(_disposable);
		Observable.EveryUpdate().Where(_ => Input.GetKeyDown(KeyCode.Alpha2)).Subscribe(_ => memory.Programs.RemoveAt(Random.Range(0, memory.Programs.Count))).AddTo(_disposable);

		var memoryView = Instantiate(_memoryViewTemplate);
		memoryView.transform.SetParent(_memoryViewParent, worldPositionStays: false);
		memoryView.Show(memory, _cube, Camera.main);
		memoryView.AddTo(_disposable);

		var palette = Instantiate(_paletteTemplate);
		palette.transform.SetParent(_paletteParent, worldPositionStays: false);
		palette.Show(memory.Programs);
		palette.AddTo(_disposable);
	}

	private void OnDestroy()
	{
		_disposable.Dispose();
	}
}