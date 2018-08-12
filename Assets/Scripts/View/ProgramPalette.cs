﻿using System;
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

        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public void Show(IReadOnlyReactiveCollection<Program> programs)
        {
            programs.CreateView(_programViewTemplate, _programsParent, (view, program) => view.Show(program)).AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}
