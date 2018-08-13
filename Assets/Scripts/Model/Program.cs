using System;
using System.Linq;
using Data;
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

        public readonly ReactiveProperty<int> LeakedBytes = new ReactiveProperty<int>();
        public readonly ReactiveProperty<int> ProducedBytes = new ReactiveProperty<int>();

        public readonly IReadOnlyReactiveProperty<int> MemorySize;

        public readonly IReadOnlyReactiveProperty<string> Name;

        public readonly IReadOnlyReactiveProperty<int> LeakBytesPerSecond;
        public readonly IReadOnlyReactiveProperty<int> ProduceBytesPerSecond;

        public Program(ProgramTemplate template)
        {
            Template = template;
            CurrentVersion = new ReactiveProperty<ProgramVersion>(template.Versions[0]);
            InstalledPatches = new ReactiveCollection<PatchTemplate>();

            LeakedBytes = new ReactiveProperty<int>();

            MemorySize = CurrentVersion.CombineLatest(InstalledPatches.CountProperty(),
                (version, _) => version.MemorySize + InstalledPatches.Sum(patch => patch.SizeDelta)).ToReactiveProperty();

            Name = CurrentVersion.Select(_ => GetName(GetCurrentVersionIndex())).ToReactiveProperty();

            LeakBytesPerSecond = CurrentVersion.CombineLatest(InstalledPatches.CountProperty(),
                (version, _) => version.LeakBytesPerSecond + InstalledPatches.Sum(patch => patch.LeakDelta)).ToReactiveProperty();

            ProduceBytesPerSecond = CurrentVersion.Select(x => x.ProduceBytesPerSecond).ToReactiveProperty();
        }

        public Program(Program originalProgram)
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
        }

        public bool ExecuteOneSecond()
        {
            LeakedBytes.Value += LeakBytesPerSecond.Value;
            ProducedBytes.Value += ProduceBytesPerSecond.Value;
            return true;
        }

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

