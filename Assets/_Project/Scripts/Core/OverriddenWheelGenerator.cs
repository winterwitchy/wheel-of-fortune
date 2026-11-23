using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class OverriddenWheelGenerator : IWheelGenerator
    {
        #region Fields

        private readonly IWheelGenerator _inner;
        private readonly IReadOnlyList<SliceOverrideRule> _overrides;
        private readonly WheelBombRule _bombRule;

        #endregion

        #region Constructors

        public OverriddenWheelGenerator(IWheelGenerator inner, IReadOnlyList<SliceOverrideRule> overrides, WheelBombRule bombRule)
        {
            _inner = inner;
            _overrides = overrides;
            _bombRule = bombRule;
        }

        #endregion

        #region Public Methods

        public IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneKind zoneKind)
        {
            var original = _inner.Generate(zone, zoneKind);
            var slices = new List<WheelSliceEntry>(original);
            var alterationOrder = CreateUnalteredOrder(slices.Count);

            ApplyOverrides(zone, zoneKind, slices, alterationOrder);
            _bombRule.Enforce(slices, original, alterationOrder, zoneKind);

            return slices;
        }

        #endregion

        #region Private Methods

        private void ApplyOverrides(int zone, ZoneKind zoneKind, List<WheelSliceEntry> slices, int[] alterationOrder)
        {
            var order = 0;

            foreach (var sliceOverride in _overrides)
            {
                if (!IsApplicable(sliceOverride, zone, zoneKind, slices.Count))
                {
                    continue;
                }

                slices[sliceOverride.SliceIndex] = sliceOverride.ToSliceEntry();
                alterationOrder[sliceOverride.SliceIndex] = order;
                order++;
            }
        }

        private static bool IsApplicable(SliceOverrideRule sliceOverride, int zone, ZoneKind zoneKind, int sliceCount)
        {
            if (sliceOverride.Zone != zone)
            {
                return false;
            }

            if (sliceOverride.SliceIndex < 0 || sliceOverride.SliceIndex >= sliceCount)
            {
                return false;
            }

            if (sliceOverride.IsBomb && WheelBombRule.GetRequiredBombCount(zoneKind) == 0)
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

        #endregion
    }
}
