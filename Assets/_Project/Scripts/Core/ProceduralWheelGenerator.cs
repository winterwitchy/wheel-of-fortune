using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class ProceduralWheelGenerator : IWheelGenerator
    {
        private readonly List<ZoneBand> _bands;
        private readonly IRandomProvider _random;

        public ProceduralWheelGenerator(IReadOnlyList<ZoneBand> bands, IRandomProvider random)
        {
            if (bands == null || bands.Count == 0)
            {
                throw new ArgumentException("At least one zone band is required.", nameof(bands));
            }

            _bands = new List<ZoneBand>(bands);
            _bands.Sort(CompareByFirstZone);
            _random = random;
        }

        public IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneKind zoneKind)
        {
            var draft = new WheelDraft();

            if (zoneKind.HasBomb)
            {
                draft.AddBomb();
            }
            else if (zoneKind.BombSlotSource != null)
            {
                zoneKind.BombSlotSource.AddTo(draft, zone, zoneKind.Multiplier, _random);
            }

            AddComposition(draft, GetComposition(zone, zoneKind), zone, zoneKind.Multiplier);
            Shuffle(draft.Slices);
            return draft.Slices;
        }

        public ZoneBand GetBand(int zone)
        {
            var result = _bands[0];

            foreach (var band in _bands)
            {
                if (band.FirstZone > zone)
                {
                    break;
                }

                result = band;
            }

            return result;
        }

        private IReadOnlyList<SliceRule> GetComposition(int zone, ZoneKind zoneKind)
        {
            if (zoneKind.HasOwnComposition)
            {
                return zoneKind.Composition;
            }

            return GetBand(zone).Composition;
        }

        private void AddComposition(WheelDraft draft, IReadOnlyList<SliceRule> composition, int zone, int multiplier)
        {
            foreach (var rule in composition)
            {
                for (var i = 0; i < rule.Count; i++)
                {
                    rule.Source.AddTo(draft, zone, multiplier, _random);
                }
            }
        }

        private void Shuffle(List<WheelSliceEntry> slices)
        {
            for (var i = slices.Count - 1; i > 0; i--)
            {
                var j = _random.Range(0, i + 1);
                var temp = slices[i];
                slices[i] = slices[j];
                slices[j] = temp;
            }
        }

        private static int CompareByFirstZone(ZoneBand left, ZoneBand right)
        {
            return left.FirstZone.CompareTo(right.FirstZone);
        }
    }
}
