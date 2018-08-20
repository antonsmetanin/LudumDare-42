using System;
using System.Linq;
using Data;
using UniRx;
using UnityEngine;
using Utils;

namespace Model
{
    public class Robot
    {
        public readonly ReactiveCollection<Program> Programs;

        public readonly ReactiveProperty<int> MemoryUpgrades = new ReactiveProperty<int>();

        public Transform Transform;
        private readonly Game _game;
        private readonly RobotTemplate _template;

        public readonly ReactiveProperty<bool> UploadIsRunning = new ReactiveProperty<bool>();

        public readonly ReactiveProperty<int> LeakedBytes = new ReactiveProperty<int>();
        public readonly ReactiveProperty<int> ProducedBytes = new ReactiveProperty<int>();

        public readonly IReadOnlyReactiveProperty<int> ProgramBytes;
        public readonly IReadOnlyReactiveProperty<int> TotalUsedBytes;
        public readonly IReadOnlyReactiveProperty<int> FreeSpace;
        
        public readonly IReadOnlyReactiveProperty<int> MemorySize;

        public enum RobotStatus
        {
            Ok,
            BootError,
            OutOfMemory
        }

        public readonly IReadOnlyReactiveProperty<RobotStatus> Status;
        public readonly IReadOnlyReactiveProperty<bool> HasSyncProgram;

        public Robot(RobotTemplate template, Game game)
        {
            _template = template;
            _game = game;

            MemorySize = MemoryUpgrades
                .Select(upgradesCount => template.InitialMemorySize + upgradesCount * template.MemoryUpgradeSize)
                .ToReactiveProperty();

            Programs = new ReactiveCollection<Program>();

            ProgramBytes = Programs.ObserveCountChanged(true)
                .SelectMany(_ => Programs
                    .Select(x => x.MemorySize)
                    .CombineLatest()
                    .Select(y => y.Sum()))
                .ToReactiveProperty(initialValue: 0);

            TotalUsedBytes = ProgramBytes.CombineLatest(LeakedBytes, ProducedBytes,
                (programs, leaked, produced) => programs + leaked + produced)
                .ToReactiveProperty();

            FreeSpace = TotalUsedBytes.CombineLatest(MemorySize,
                (used, memory) => memory - used)
                .ToReactiveProperty();

            Status = Programs.ObserveCountChanged(true).CombineLatest(FreeSpace,
                (programCount, freeSpace) => freeSpace <= 0 ? RobotStatus.OutOfMemory
                                           : programCount == 0 ? RobotStatus.BootError
                                           : RobotStatus.Ok)
                .ToReactiveProperty();

            HasSyncProgram = Programs.ObserveCountChanged(true)
                .Select(_ => Programs.Any(x => x.Template.Type == ProgramType.Sync))
                .ToReactiveProperty();

            //TODO: stop sync on game over
            Observable.CombineLatest(HasSyncProgram, Status.Select(x => x == RobotStatus.OutOfMemory),
                (hasSync, outOfMemory) => hasSync && !outOfMemory)
                .DistinctUntilChanged()
                .Select(shouldSync => shouldSync ? Observable.Interval(TimeSpan.FromSeconds(10)) : Observable.Empty<long>())
                .Switch()
                .Subscribe(_ =>
                {
                    _game.GameProgress.DataCollected.Value = ProducedBytes.Value;
                    ProducedBytes.Value = 0;
                });
        }

        public class UploadDataResult : IOperationResult { }
        public class NothingToCollectError : Error { }
        public class UploadIsAlreadyRunningError : Error { }

        public IObservable<Result<UploadDataResult>> CanUploadData()
            => Observable.CombineLatest(ProducedBytes.Select(x => x > 0).DistinctUntilChanged(), UploadIsRunning,
                (_, __) => CollectData(simulate: true));

        public Result<UploadDataResult> CollectData(bool simulate = false)
        {
            if (ProducedBytes.Value <= 0)
                return new NothingToCollectError();
            
            if (UploadIsRunning.Value)
                return new UploadIsAlreadyRunningError();

            if (!simulate)
            {
                UploadIsRunning.Value = true;

                //TODO: stop uploading on game over
                Observable.Interval(TimeSpan.FromSeconds(1))
                    .TakeUntil(ProducedBytes.Where(x => x <= 0))
                    .Subscribe(_ =>
                    {
                        var uploadBytes = Mathf.Min(_template.UploadSpeedBytesPerSecond, ProducedBytes.Value);
                        ProducedBytes.Value -= uploadBytes;
                        _game.GameProgress.DataCollected.Value += uploadBytes;
                    },
                    () => UploadIsRunning.Value = false);
            }
            

            return new UploadDataResult();
        }

        public class InstallProgramResult : IOperationResult { }

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
