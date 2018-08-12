using System.Collections.Generic;
using System.Linq;
using Data;
using UniRx;

namespace Model
{
	public class Game
	{
		public readonly GameProgress GameProgress;
		public readonly ReactiveCollection<Robot> Robots = new ReactiveCollection<Robot>();
		public readonly ReactiveProperty<Robot> SelectedRobot = new ReactiveProperty<Robot>();

		public Game(GameProgressTemplate gameProgressTemplate, IEnumerable<ProgramTemplate> defaultProgramTemplates)
		{
			GameProgress = new GameProgress(gameProgressTemplate);
		}
	}
}
