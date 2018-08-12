using Model;
using System;
using UniRx;
using UnityEngine;

namespace View
{
    public class Dashboard : MonoBehaviour, IDisposable
    {
        [SerializeField] private FillBar _dataCollected;
//        [SerializeField] private FillBar _wood;

        [SerializeField] private ProgramPalette _palette;

        private CompositeDisposable _disposable;

        public void Show(GameProgress gameProgress)
        {
            _disposable = new CompositeDisposable();

            gameProgress.DataCollected
                .Subscribe(dataCollected => _dataCollected.SetValue(dataCollected, 100f))
                .AddTo(_disposable);

//            gameProgress.Wood
//                .Subscribe(wood => _wood.SetValue(wood, 100f))
//                .AddTo(_disposable);

            _palette.Show(gameProgress);
            _palette.AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}