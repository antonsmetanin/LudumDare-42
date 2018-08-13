using System.Linq;
using Data;
using UniRx;
using Utils;

namespace Model
{
    public class Robot
    {
        public ReactiveProperty<int> MemorySize;
        public ReactiveCollection<Program> Programs;

        public UnityEngine.Transform Transform;
        private GameProgress _gameProgress;

//        public IObservable<int> LeakedBytes;
//        public IObservable<int> ProducedBytes;

        public readonly ReactiveProperty<int> LeakedBytes = new ReactiveProperty<int>();
        public readonly ReactiveProperty<int> ProducedBytes = new ReactiveProperty<int>();

        public int GetTotalBytes() => Programs.Sum(x => x.MemorySize.Value) + LeakedBytes.Value + ProducedBytes.Value;
        public int GetFreeSpace() => MemorySize.Value - GetTotalBytes();

        private readonly IReadOnlyReactiveProperty<bool> Broken;

        public Robot(RobotTemplate template, GameProgress gameProgress)
        {
            MemorySize = new ReactiveProperty<int>(template.InitialMemorySize);
            Programs = new ReactiveCollection<Program>();



            _gameProgress = gameProgress;
        }

        public class InstallProgramResult { }

        public class NotEnoughMemoryError : Error { }

        public Result<InstallProgramResult> InstallProgram(Program program, bool simulate = false)
        {
            if (Programs.Sum(x => x.MemorySize.Value) + program.MemorySize.Value > MemorySize.Value)
                return new NotEnoughMemoryError();

            if (!simulate)
                Programs.Add(new Program(program, this));

            return new InstallProgramResult();
        }

        public void ClearLeaks()
        {
            LeakedBytes.Value = 0;
        }

        public void UploadData()
        {
            _gameProgress.DataCollected.Value += ProducedBytes.Value;

            ProducedBytes.Value = 0;
        }
    }
}
