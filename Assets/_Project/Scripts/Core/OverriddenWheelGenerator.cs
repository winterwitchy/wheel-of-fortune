using System.Collections.Generic;
using WheelGame.Data;

namespace WheelGame.Core
{
    public sealed class OverriddenWheelGenerator : IWheelGenerator
    {
        private readonly IWheelGenerator _inner;
        private readonly IReadOnlyList<SliceOverride> _overrides;
        private readonly WheelBombRule _bombRule;

        public OverriddenWheelGenerator(IWheelGenerator inner, IReadOnlyList<SliceOverride> overrides, WheelBombRule bombRule)
        {
            _inner = inner;
            _overrides = overrides;
            _bombRule = bombRule;
        }

        public IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneType zoneType)
        {
            var original = _inner.Generate(zone, zoneType);
            var slices = new List<WheelSliceEntry>(original);
            var alterationOrder = CreateUnalteredOrder(slices.Count);

            ApplyOverrides(zone, zoneType, slices, alterationOrder);
            _bombRule.Enforce(slices, original, alterationOrder, zoneType);

            return slices;
        }

        private void ApplyOverrides(int zone, ZoneType zoneType, List<WheelSliceEntry> slices, int[] alterationOrder)
        {
            var order = 0;

            foreach (var sliceOverride in _overrides)
            {
                if (!IsApplicable(sliceOverride, zone, zoneType, slices.Count))
                {
                    continue;
                }

                slices[sliceOverride.SliceIndex] = sliceOverride.ToSliceEntry();
                alterationOrder[sliceOverride.SliceIndex] = order;
                order++;
            }
        }

        private static bool IsApplicable(SliceOverride sliceOverride, int zone, ZoneType zoneType, int sliceCount)
        {
            if (sliceOverride.Zone != zone)
            {
                return false;
            }

            if (sliceOverride.SliceIndex < 0 || sliceOverride.SliceIndex >= sliceCount)
            {
                return false;
            }

            if (!sliceOverride.IsValid)
            {
                return false;
            }

            if (sliceOverride.IsBomb && WheelBombRule.GetRequiredBombCount(zoneType) == 0)
            {
                return false;
            }

            return true;
        }

        private static int[] CreateUnalteredOrder(int sliceCount)
        {
            var result = new int[sliceCount];

            for (var i = 0; i < sliceCount; i++)
            {
                result[i] = WheelBombRule.UNALTERED;
            }

            return result;
        }
    }
}
