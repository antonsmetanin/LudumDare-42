using Data;
using System;
using UniRx;
using UniRx.Diagnostics;
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

        public IObservable<Result<BuyResult>> CanBuyProgram(ProgramTemplate programTemplate)
            => GameProgress.DataCollected.Select(_ => BuyProgram(programTemplate, simulate: true));

        public Result<BuyResult> BuyProgram(ProgramTemplate programTemplate, bool simulate = false)
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

        public IObservable<Result<BuyResult>> CanBuyRobot()
            => GameProgress.DataCollected.Select(_ => BuyRobot(simulate: true));

        public Result<BuyResult> BuyRobot(bool simulate = false)
        {
            if (GameProgress.DataCollected.Value < Template.RobotPrice)
                return new Program.NotEnoughDataError();

            if (!simulate)
            {
                Robots.Add(new Robot(Template.RobotTemplate, this));
                GameProgress.DataCollected.Value -= Template.RobotPrice;
            }

            return new BuyResult(Template.RobotPrice);
        }
    }
}
