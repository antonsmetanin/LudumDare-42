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

        public readonly ReactiveProperty<int> LeakedBytes = new ReactiveProperty<int>();
        public readonly ReactiveProperty<int> ProducedBytes = new ReactiveProperty<int>();

        public readonly IReadOnlyReactiveProperty<int> ProgramBytes;
        public readonly IReadOnlyReactiveProperty<int> TotalUsedBytes;
        public readonly IReadOnlyReactiveProperty<int> FreeSpace;
        public readonly IReadOnlyReactiveProperty<bool> Broken;

        public Robot(RobotTemplate template, GameProgress gameProgress)
        {
            MemorySize = new ReactiveProperty<int>(template.InitialMemorySize);
            Programs = new ReactiveCollection<Program>();

            ProgramBytes = Programs.CountProperty()
                .SelectMany(_ => Programs
                    .Select(x => x.MemorySize)
                    .CombineLatest()
                    .Select(y => y.Sum()))
                .ToReactiveProperty();

            TotalUsedBytes = ProgramBytes.CombineLatest(LeakedBytes, ProducedBytes,
                (programs, leaked, produced) => programs + leaked + produced)
                .ToReactiveProperty();

            FreeSpace = TotalUsedBytes.CombineLatest(MemorySize,
                (used, memory) => memory - used)
                .ToReactiveProperty();

            Broken = FreeSpace.Select(x => x <= 0)
                .ToReactiveProperty();

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
