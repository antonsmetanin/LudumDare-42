using System;
using System.Linq;
using Data;
using Model;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
    public class PurchasableProgramView : MonoBehaviour, IDisposable
    {
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private Button _button;

        [SerializeField] private TextMeshProUGUI _description;

        private CompositeDisposable _disposable;

        public void Show(Game game, ProgramTemplate programTemplate, ReactiveProperty<IOperationResult> pendingAction)
        {
            _disposable = new CompositeDisposable();

            _nameLabel.text = programTemplate.Name;
            _description.text = programTemplate.Versions[0].Description;

            _button.OnClickAsObservable()
                .Subscribe(_ => game.BuyProgram(programTemplate))
                .AddTo(_disposable);

            game.CanBuyProgram(programTemplate)
                .Subscribe(canUpgrade => _button.interactable = canUpgrade.Error == null)
                .AddTo(_disposable);

            game.GameProgress.AvailablePrograms.ObserveCountChanged(true)
                .Select(_ => game.GameProgress.AvailablePrograms.Any(program => program.Template == programTemplate))
                .Subscribe(isAlreadyAvailable => gameObject.SetActive(!isAlreadyAvailable))
                .AddTo(_disposable);

            _button.gameObject.GetComponent<HoverTrigger>().Hovered
                .Subscribe(hovered => pendingAction.Value = hovered ? game.BuyProgram(programTemplate, simulate: true).Value : null)
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}