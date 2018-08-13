using Model;
using System;
using UniRx;
using UnityEngine;
using Utils;

namespace View
{
    public class Dashboard : MonoBehaviour, IDisposable
    {
        [SerializeField] private ProgramPalette _palette;
        [SerializeField] private DataIndicator _dataIndicator;

        private CompositeDisposable _disposable;

        public ReactiveProperty<IOperationResult> PendingAction = new ReactiveProperty<IOperationResult>();

        public void Show(GameProgress gameProgress)
        {
            _disposable = new CompositeDisposable();

            _dataIndicator.Show(gameProgress, PendingAction);
            _dataIndicator.AddTo(_disposable);

            _palette.Show(gameProgress, PendingAction);
            _palette.AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}