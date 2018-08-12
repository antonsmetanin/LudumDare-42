using System;
using System.Linq;
using Data;
using UniRx;
using UnityEngine.Experimental.Rendering;

namespace Model
{
    public class Program
    {
        public readonly ProgramTemplate Template;

        public readonly ReactiveProperty<ProgramVersion> CurrentVersion;
        public readonly ReactiveCollection<PatchTemplate> InstalledPatches;

        public readonly IReadOnlyReactiveProperty<int> Size;
        public readonly IReadOnlyReactiveProperty<bool> CanUpgrade;
        public readonly IReadOnlyReactiveProperty<bool> CanPatch;

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

            CanUpgrade = CurrentVersion.Select(_ => Upgrade(simulate: true)).ToReactiveProperty();
            CanPatch = InstalledPatches.ObserveCountChanged().Select(_ => Patch(simulate: true)).ToReactiveProperty();
            Name = CurrentVersion.Select(_ => GetName(GetCurrentVersionIndex())).ToReactiveProperty();

            LeakSpeed = CurrentVersion.CombineLatest(InstalledPatches.ObserveCountChanged().ToReactiveProperty(InstalledPatches.Count),
                (version, _) => version.LeakSpeed + InstalledPatches.Sum(patch => patch.LeakDelta)).ToReactiveProperty();

            ProduceSpeed = CurrentVersion.Select(x => x.ProduceSpeed).ToReactiveProperty();
        }

        public int GetCurrentVersionIndex() => Array.IndexOf(Template.Versions, CurrentVersion.Value);
        private string GetName(int index) => index == 0 ? Template.Name : Template.Name + " v." + (index + 1);

        public bool Upgrade(bool simulate = false)
        {
            var currentVersionIndex = GetCurrentVersionIndex();

            if (currentVersionIndex == -1)
                return false;

            if (currentVersionIndex >= Template.Versions.Length - 1)
                return false;

            if (!simulate)
                CurrentVersion.Value = Template.Versions[currentVersionIndex + 1];

            return true;
        }

        public bool Patch(bool simulate = false)
        {
            var lastPatchIndex = InstalledPatches.Count > 0 ? Array.IndexOf(Template.AvailablePatches, InstalledPatches.Last()) : -1;

            if (lastPatchIndex >= Template.AvailablePatches.Length - 1)
                return false;

            if (!simulate)
                InstalledPatches.Add(Template.AvailablePatches[lastPatchIndex + 1]);

            return true;
        }
    }
}

