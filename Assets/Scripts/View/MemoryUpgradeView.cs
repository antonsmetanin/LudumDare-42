using System;
using UniRx;
using UnityEngine;

namespace View
{
    public class MemoryUpgradeView : MonoBehaviour, IDisposable
    {
        private CompositeDisposable _disposable;

        public void Show()
        {
            _disposable = new CompositeDisposable();
        }

        public void Dispose() => _disposable.Dispose();
    }
}