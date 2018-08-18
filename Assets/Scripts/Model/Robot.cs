using System;
using System.Linq;
using Data;
using UniRx;
using Utils;

namespace Model
{
    public class Robot
    {
        public ReactiveCollection<Program> Programs;

        public readonly ReactiveProperty<int> MemoryUpgrades = new ReactiveProperty<int>();

        public UnityEngine.Transform Transform;
        private Game _game;

        public readonly ReactiveProperty<int> LeakedBytes = new ReactiveProperty<int>();
        public readonly ReactiveProperty<int> ProducedBytes = new ReactiveProperty<int>();

        public readonly IReadOnlyReactiveProperty<int> ProgramBytes;
        public readonly IReadOnlyReactiveProperty<int> TotalUsedBytes;
        public readonly IReadOnlyReactiveProperty<int> FreeSpace;
        public readonly IReadOnlyReactiveProperty<bool> Broken;
        public readonly IReadOnlyReactiveProperty<int> MemorySize;

        public Robot(RobotTemplate template, Game game)
        {
            MemorySize = MemoryUpgrades
                .Select(upgradesCount => template.InitialMemorySize + upgradesCount * template.MemoryUpgradeSize)
                .ToReactiveProperty();

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

            _game = game;
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
            _game.GameProgress.DataCollected.Value += ProducedBytes.Value;

            ProducedBytes.Value = 0;
        }

        public class UpgradeMemoryResult : Program.IPricedOperation
        {
            public int Price { get; }
            public UpgradeMemoryResult(int price) => Price = price;
        }

        public class AlreadyUpgradedError : Error { }
        public class UpgradeEarlierIndexFirstError : Error { }
        public class MaxUpgradesReachedError : Error { }

        public IObservable<Result<UpgradeMemoryResult>> CanUpgradeMemory()
            => _game.GameProgress.DataCollected.CombineLatest(MemoryUpgrades, (_, __) => UpgradeMemory(simulate: true));

        public Result<UpgradeMemoryResult> UpgradeMemory(bool simulate = false)
        {
            if (MemoryUpgrades.Value >= 3)
                return new MaxUpgradesReachedError();

            var price = _game.Template.MemoryUpgradePrice;

            if (_game.GameProgress.DataCollected.Value < price)
                return new Program.NotEnoughDataError();

            if (!simulate)
            {
                _game.GameProgress.DataCollected.Value -= price;
                MemoryUpgrades.Value++;
            }

            return new UpgradeMemoryResult(price);
        }
    }
}
