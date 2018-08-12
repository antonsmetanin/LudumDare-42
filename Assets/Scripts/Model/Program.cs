using System;
using System.Linq;
using Data;
using UniRx;
using Utils;

namespace Model
{
    public class Program
    {
        public readonly ProgramTemplate Template;

        public readonly ReactiveProperty<ProgramVersion> CurrentVersion;
        public readonly ReactiveCollection<PatchTemplate> InstalledPatches;

        public readonly IReadOnlyReactiveProperty<int> Size;

        public readonly IReadOnlyReactiveProperty<string> Name;

        public readonly IReadOnlyReactiveProperty<int> LeakSpeed;
        public readonly IReadOnlyReactiveProperty<int> ProduceSpeed;

        public Program(ProgramTemplate template)
        {
            Template = template;
            CurrentVersion = new ReactiveProperty<ProgramVersion>(template.Versions[0]);
            InstalledPatches = new ReactiveCollection<PatchTemplate>();

            Size = CurrentVersion.CombineLatest(InstalledPatches.ObserveCountChanged().ToReactiveProperty(InstalledPatches.Count),
                (version, _) => version.Size + InstalledPatches.Sum(patch => patch.SizeDelta)).ToReactiveProperty();

            Name = CurrentVersion.Select(_ => GetName(GetCurrentVersionIndex())).ToReactiveProperty();

            LeakSpeed = CurrentVersion.CombineLatest(InstalledPatches.ObserveCountChanged().ToReactiveProperty(InstalledPatches.Count),
                (version, _) => version.LeakSpeed + InstalledPatches.Sum(patch => patch.LeakDelta)).ToReactiveProperty();

            ProduceSpeed = CurrentVersion.Select(x => x.ProduceSpeed).ToReactiveProperty();
        }

        public Program(Program originalProgram)
        {
            Template = originalProgram.Template;
            CurrentVersion = new ReactiveProperty<ProgramVersion>(originalProgram.CurrentVersion.Value);
            InstalledPatches = new ReactiveCollection<PatchTemplate>(originalProgram.InstalledPatches);

            Size = CurrentVersion.CombineLatest(InstalledPatches.ObserveCountChanged().ToReactiveProperty(InstalledPatches.Count),
                (version, _) => version.Size + InstalledPatches.Sum(patch => patch.SizeDelta)).ToReactiveProperty();

            Name = CurrentVersion.Select(_ => GetName(GetCurrentVersionIndex())).ToReactiveProperty();

            LeakSpeed = CurrentVersion.CombineLatest(InstalledPatches.ObserveCountChanged().ToReactiveProperty(InstalledPatches.Count),
                (version, _) => version.LeakSpeed + InstalledPatches.Sum(patch => patch.LeakDelta)).ToReactiveProperty();

            ProduceSpeed = CurrentVersion.Select(x => x.ProduceSpeed).ToReactiveProperty();
        }

        public int GetCurrentVersionIndex() => Array.IndexOf(Template.Versions, CurrentVersion.Value);
        private string GetName(int index) => index == 0 ? Template.Name : Template.Name + " v." + (index + 1);

        public class UpgradeResult { }
        public class NotEnoughDataError : Error { }
        public class FinalVersionReachedError : Error { }

        public IObservable<Result<UpgradeResult>> CanUpgrade(GameProgress gameProgress)
            => gameProgress.DataCollected.CombineLatest(CurrentVersion, (_, __) => Upgrade(gameProgress, simulate: true));

        public class PatchResult { }

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

            return new UpgradeResult();
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

            return new PatchResult();
        }
    }
}

