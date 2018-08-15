using Data;
using System;
using UniRx;
using Utils;

namespace Model
{
	public class Game
	{
        public readonly GameTemplate Template;
        public readonly GameProgress GameProgress;
		public readonly ReactiveCollection<Robot> Robots = new ReactiveCollection<Robot>();
		public readonly ReactiveProperty<Robot> SelectedRobot = new ReactiveProperty<Robot>();

        public Game(GameTemplate gameTemplate, GameProgress gameProgress)
        {
            Template = gameTemplate;
            GameProgress = gameProgress;
        }

        public class BuyResult : Program.IPricedOperation
        {
            public int Price { get; }

            public BuyResult(int price) => Price = price;
        }

        public IObservable<Result<BuyResult>> CanUpgrade(ProgramTemplate programTemplate)
            => GameProgress.DataCollected.Select(_ => Buy(programTemplate, simulate: true));

        public Result<BuyResult> Buy(ProgramTemplate programTemplate, bool simulate = false)
        {
            var price = programTemplate.Versions[0].Price;

            if (GameProgress.DataCollected.Value < price)
                return new Program.NotEnoughDataError();

            if (!simulate)
            {
                GameProgress.AvailablePrograms.Add(new Program(programTemplate));
                GameProgress.DataCollected.Value -= price;
            }
                
            return new BuyResult(price);
        }
    }
}
