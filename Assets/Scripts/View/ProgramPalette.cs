using System;
using Model;
using UniRx;
using UnityEngine;
using Utils;

namespace View
{
    public class ProgramPalette : MonoBehaviour, IDisposable
    {
        [SerializeField] private DashboardProgramView _programViewTemplate;
        [SerializeField] private RectTransform _programsParent;

        private CompositeDisposable _disposable;

        public void Show(GameProgress gameProgress, ReactiveProperty<IOperationResult> pendingAction)
        {
            _disposable = new CompositeDisposable();

            gameProgress.AvailablePrograms
                    .CreateView(_programViewTemplate, _programsParent, (view, program) => view.Show(program, gameProgress, pendingAction))
                    .AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}
