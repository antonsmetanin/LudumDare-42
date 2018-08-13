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

        public int GetLeakedBytes() => Programs.Sum(x => x.LeakedBytes.Value);
        public int GetProducedBytes() => Programs.Sum(x => x.ProducedBytes.Value);

        public int GetTotalBytes() => Programs.Sum(x => x.MemorySize.Value) + GetLeakedBytes() + GetProducedBytes();
        public int GetFreeSpace() => MemorySize.Value - GetTotalBytes();

        public Robot(RobotTemplate template, GameProgress gameProgress)
        {
            MemorySize = new ReactiveProperty<int>(template.InitialMemorySize);
            Programs = new ReactiveCollection<Program>();

            _gameProgress = gameProgress;

//            LeakedBytes = Programs.CountProperty()
//                .SelectMany(_ => Programs.Select(x => x.LeakedBytes).CombineLatest().Select(y => y.Sum()))
//                .ToReactiveProperty();
//
//            ProducedBytes = Programs.CountProperty()
//                .SelectMany(_ => Programs.Select(x => x.ProducedBytes).CombineLatest().Select(y => y.Sum()))
//                .ToReactiveProperty();
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
            foreach (var program in Programs)
                program.LeakedBytes.Value = 0;
        }

        public void UploadData()
        {
            _gameProgress.DataCollected.Value += GetProducedBytes();

            foreach (var program in Programs)
                program.ProducedBytes.Value = 0;
        }
    }
}
