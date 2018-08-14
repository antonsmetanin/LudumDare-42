using Data;
using UniRx;

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
    }
}
