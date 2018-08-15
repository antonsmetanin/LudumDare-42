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

        [SerializeField] private PurchasableProgramView _unboughtProgramTemplate;
        [SerializeField] private RectTransform _unboughtProgramsParent;

        private CompositeDisposable _disposable;

        public void Show(Game game, ReactiveProperty<IOperationResult> pendingAction)
        {
            _disposable = new CompositeDisposable();

            game.GameProgress.AvailablePrograms
                    .CreateView(_programViewTemplate, _programsParent, (view, program) => view.Show(program, game, pendingAction))
                    .AddTo(_disposable);

            game.Template.AllPrograms
                .CreateView(_unboughtProgramTemplate, _unboughtProgramsParent, (view, programTemplate) => view.Show(game, programTemplate, pendingAction))
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}
