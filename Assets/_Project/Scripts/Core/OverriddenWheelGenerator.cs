using System.Collections.Generic;
using WheelGame.Data;

namespace WheelGame.Core
{
    public sealed class OverriddenWheelGenerator : IWheelGenerator
    {
        private readonly IWheelGenerator _inner;
        private readonly IReadOnlyList<SliceOverride> _overrides;
        private readonly IRandomProvider _random;

        public OverriddenWheelGenerator(IWheelGenerator inner, IReadOnlyList<SliceOverride> overrides, IRandomProvider random)
        {
            _inner = inner;
            _overrides = overrides;
            _random = random;
        }

        public IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneType zoneType)
        {
            var generated = _inner.Generate(zone, zoneType);
            var zoneOverrides = CollectOverrides(zone, zoneType);

            if (zoneOverrides.Count == 0)
                return generated;

            var slices = new List<WheelSliceEntry>(generated);
            var overriddenIndices = new HashSet<int>();
            var rewardIndices = new HashSet<int>();

            foreach (var sliceOverride in zoneOverrides)
            {
                overriddenIndices.Add(sliceOverride.SliceIndex);

                if (!sliceOverride.IsBomb)
                    rewardIndices.Add(sliceOverride.SliceIndex);
            }

            RelocateDisplacedBomb(slices, rewardIndices, overriddenIndices);

            foreach (var sliceOverride in zoneOverrides)
                slices[sliceOverride.SliceIndex] = sliceOverride.ToSliceEntry();

            return slices;
        }

        private List<SliceOverride> CollectOverrides(int zone, ZoneType zoneType)
        {
            var result = new List<SliceOverride>();

            foreach (var sliceOverride in _overrides)
            {
                if (sliceOverride.Zone != zone)
                    continue;

                if (sliceOverride.IsBomb && zoneType != ZoneType.Normal)
                    continue;

                result.Add(sliceOverride);
            }

            return result;
        }

        private void RelocateDisplacedBomb(List<WheelSliceEntry> slices, HashSet<int> rewardIndices, HashSet<int> overriddenIndices)
        {
            var bombIndex = slices.FindIndex(slice => slice.IsBomb);
            if (bombIndex < 0 || !rewardIndices.Contains(bombIndex))
                return;

            var freeIndices = new List<int>();
            for (var i = 0; i < slices.Count; i++)
            {
                if (!overriddenIndices.Contains(i))
                    freeIndices.Add(i);
            }

            if (freeIndices.Count == 0)
                return;

            var target = freeIndices[_random.Range(0, freeIndices.Count)];
            (slices[bombIndex], slices[target]) = (slices[target], slices[bombIndex]);
        }
    }
}