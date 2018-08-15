using System;
using System.Linq;
using Data;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Utils;

namespace Model
{
    public class Program
    {
        public readonly ProgramTemplate Template;

        public readonly ReactiveProperty<ProgramVersion> CurrentVersion;
        public readonly ReactiveCollection<PatchTemplate> InstalledPatches;

        public readonly IReadOnlyReactiveProperty<int> MemorySize;
        public readonly IReadOnlyReactiveProperty<string> Name;
        public readonly IReadOnlyReactiveProperty<int> LeakBytesPerSecond;
        public readonly IReadOnlyReactiveProperty<int> ProduceBytesPerSecond;

        [CanBeNull] private readonly Robot _robot;

        public Program(ProgramTemplate template)
        {
            Template = template;
            CurrentVersion = new ReactiveProperty<ProgramVersion>(template.Versions[0]);
            InstalledPatches = new ReactiveCollection<PatchTemplate>();

            MemorySize = CurrentVersion.CombineLatest(InstalledPatches.CountProperty(),
                (version, _) => version.MemorySize + InstalledPatches.Sum(patch => patch.SizeDelta)).ToReactiveProperty();

            Name = CurrentVersion.Select(_ => GetName(GetCurrentVersionIndex())).ToReactiveProperty();

            LeakBytesPerSecond = CurrentVersion.CombineLatest(InstalledPatches.CountProperty(),
                (version, _) => version.LeakBytesPerSecond + InstalledPatches.Sum(patch => patch.LeakDelta)).ToReactiveProperty();

            ProduceBytesPerSecond = CurrentVersion.Select(x => x.ProduceBytesPerSecond).ToReactiveProperty();
        }

        public Program(Program originalProgram, Robot robot)
        {
            Template = originalProgram.Template;
            CurrentVersion = new ReactiveProperty<ProgramVersion>(originalProgram.CurrentVersion.Value);
            InstalledPatches = new ReactiveCollection<PatchTemplate>(originalProgram.InstalledPatches);

            MemorySize = CurrentVersion.CombineLatest(InstalledPatches.CountProperty(),
                (version, _) => version.MemorySize + InstalledPatches.Sum(patch => patch.SizeDelta)).ToReactiveProperty();

            Name = CurrentVersion.Select(_ => GetName(GetCurrentVersionIndex())).ToReactiveProperty();

            LeakBytesPerSecond = CurrentVersion.CombineLatest(InstalledPatches.CountProperty(),
                (version, _) => version.LeakBytesPerSecond + InstalledPatches.Sum(patch => patch.LeakDelta)).ToReactiveProperty();

            ProduceBytesPerSecond = CurrentVersion.Select(x => x.ProduceBytesPerSecond).ToReactiveProperty();

            _robot = robot;
        }

        public bool ExecuteOneSecond()
        {
            if (_robot == null)
                return false;

            _robot.LeakedBytes.Value += Mathf.Clamp(LeakBytesPerSecond.Value, 0, _robot.FreeSpace.Value);
            _robot.ProducedBytes.Value += Mathf.Clamp(ProduceBytesPerSecond.Value, 0, _robot.FreeSpace.Value);

            return true;
        }

        public void Uninstall() => _robot?.Programs.Remove(this);

        public int GetCurrentVersionIndex() => Array.IndexOf(Template.Versions, CurrentVersion.Value);
        private string GetName(int index) => index == 0 ? Template.Name : Template.Name + " v." + (index + 1);

        public interface IPricedOperation : IOperationResult
        {
            int Price { get; }
        }

        public class UpgradeResult : IPricedOperation
        {
            public int Price { get; }
            public UpgradeResult(int price) => Price = price;
        }

        public class NotEnoughDataError : Error { }
        public class FinalVersionReachedError : Error { }

        public IObservable<Result<UpgradeResult>> CanUpgrade(GameProgress gameProgress)
            => gameProgress.DataCollected.CombineLatest(CurrentVersion, (_, __) => Upgrade(gameProgress, simulate: true));

        public class PatchResult : IPricedOperation
        {
            public int Price { get; }
            public PatchResult(int price) => Price = price;
        }

        public IObservable<Result<PatchResult>> CanPatch(GameProgress gameProgress)
            => gameProgress.DataCollected.CombineLatest(InstalledPatches.ObserveCountChanged(), (_, __) => Patch(gameProgress, simulate: true));

        public Result<UpgradeResult> Upgrade(GameProgress gameProgress, bool simulate = false)
        {
            var currentVersionIndex = GetCurrentVersionIndex();

            if (currentVersionIndex == -1)
                throw new Exception("Cannot find current version");

            if (currentVersionIndex >= Template.Versions.Length - 1)
                return new FinalVersionReachedError();

            var nextVersion = Template.Versions[currentVersionIndex + 1];

            if (gameProgress.DataCollected.Value < nextVersion.Price)
                return new NotEnoughDataError();

            if (!simulate)
            {
                CurrentVersion.Value = nextVersion;
                gameProgress.DataCollected.Value -= nextVersion.Price;
            }

            return new UpgradeResult(nextVersion.Price);
        }

        public Result<PatchResult> Patch(GameProgress gameProgress, bool simulate = false)
        {
            var lastPatchIndex = InstalledPatches.Count > 0 ? Array.IndexOf(Template.AvailablePatches, InstalledPatches.Last()) : -1;

            if (lastPatchIndex >= Template.AvailablePatches.Length - 1)
                return new FinalVersionReachedError();

            var nextPatch = Template.AvailablePatches[lastPatchIndex + 1];

            if (gameProgress.DataCollected.Value < nextPatch.Price)
                return new NotEnoughDataError();

            if (!simulate)
            {
                InstalledPatches.Add(nextPatch);
                gameProgress.DataCollected.Value -= nextPatch.Price;
            }

            return new PatchResult(nextPatch.Price);
        }

        


    }
}

