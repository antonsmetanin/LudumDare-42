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

        public void Show(Game game)
        {
            _disposable = new CompositeDisposable();

            _dataIndicator.Show(game, PendingAction);
            _dataIndicator.AddTo(_disposable);

            _palette.Show(game, PendingAction);
            _palette.AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}