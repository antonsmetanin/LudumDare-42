using Model;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace View
{
    public class Dashboard : MonoBehaviour, IDisposable
    {
        [SerializeField] private ProgramPalette _palette;
        [SerializeField] private DataIndicator _dataIndicator;
        [SerializeField] private Button _buyBotButton;

        [SerializeField] private DashboardRobotView _robotViewTemplate;
        [SerializeField] private RectTransform _robotViewParent;

        private CompositeDisposable _disposable;

        public ReactiveProperty<IOperationResult> PendingAction = new ReactiveProperty<IOperationResult>();

        public void Show(Game game)
        {
            _disposable = new CompositeDisposable();

            _dataIndicator.Show(game, PendingAction);
            _dataIndicator.AddTo(_disposable);

            _palette.Show(game, PendingAction);
            _palette.AddTo(_disposable);

            game.CanBuyRobot()
                .Subscribe(canBuy => _buyBotButton.interactable = canBuy.Error == null)
                .AddTo(_disposable);

            _buyBotButton.OnClickAsObservable()
                .Subscribe(_ => game.BuyRobot())
                .AddTo(_disposable);

            game.Robots.CreateView(_robotViewTemplate, _robotViewParent, (view, robot) => view.Show(game, robot))
                .AddTo(_disposable);

            _buyBotButton.gameObject.GetComponent<HoverTrigger>().Hovered
                .Subscribe(hovered => PendingAction.Value = hovered ? game.BuyRobot(simulate: true).Value : null)
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable.Dispose();
    }
}