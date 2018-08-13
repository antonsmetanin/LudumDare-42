using System.Linq;
using Data;
using UniRx;

namespace Model
{
    public class GameProgress
    {
        public readonly GameProgressTemplate Template;

        public readonly ReactiveProperty<int> Wood;
        public readonly ReactiveProperty<int> DataCollected;
        public readonly ReactiveCollection<Program> AvailablePrograms;

        public GameProgress(GameProgressTemplate template)
        {
            Template = template;

            Wood = new ReactiveProperty<int>(template.Wood);
            DataCollected = new ReactiveProperty<int>(template.DataCollected);
            AvailablePrograms = template.AvailablePrograms.Select(x => new Program(x)).ToReactiveCollection();
        }
    }
}