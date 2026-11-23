using System;
using System.Collections.Generic;
using UnityEngine;
using WheelGame.Core;

namespace WheelGame.Data
{
    [CreateAssetMenu(fileName = "game_config", menuName = "WheelGame/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        #region Serialized Fields

        [Header("Zones")]
        [SerializeField] private List<ZoneDefinition> _zones = new List<ZoneDefinition>();
        [SerializeField] private List<ZoneBandSettings> _bands = new List<ZoneBandSettings>();

        [Header("Rules")]
        [SerializeField] private LeaveRule _leaveRule = LeaveRule.AfterFirstReward;

        [Header("Economy")]
        [SerializeField] private RewardItemData _reviveCurrency;
        [SerializeField, Min(0)] private int _startingGold = 100;
        [SerializeField, Min(1)] private int _reviveBaseCost = 25;
        [SerializeField, Min(1f)] private float _reviveCostMultiplier = 2f;

        [Header("Presentation")]
        [SerializeField, Min(-1)] private int _targetFrameRate = 60;
        [SerializeField] private WheelSpinSettings _wheelSpin = new WheelSpinSettings();

        [Header("Overrides")]
        [SerializeField] private List<SliceOverride> _sliceOverrides = new List<SliceOverride>();

        #endregion

        #region Properties

        public IReadOnlyList<ZoneDefinition> Zones
        {
            get { return _zones; }
        }

        public IReadOnlyList<ZoneBandSettings> Bands
        {
            get { return _bands; }
        }

        public LeaveRule LeaveRule
        {
            get { return _leaveRule; }
        }

        public RewardItemData ReviveCurrency
        {
            get { return _reviveCurrency; }
        }

        public int StartingGold
        {
            get { return _startingGold; }
        }

        public int ReviveBaseCost
        {
            get { return _reviveBaseCost; }
        }

        public float ReviveCostMultiplier
        {
            get { return _reviveCostMultiplier; }
        }

        public int TargetFrameRate
        {
            get { return _targetFrameRate; }
        }

        public WheelSpinSettings WheelSpin
        {
            get { return _wheelSpin; }
        }

        public IReadOnlyList<SliceOverride> SliceOverrides
        {
            get { return _sliceOverrides; }
        }

        #endregion

        #region Validation

        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            CollectZoneErrors(errors);
            CollectBandErrors(errors);
            CollectPoolErrors(errors);
            CollectEconomyErrors(errors);

            if (errors.Count == 0)
            {
                CollectOverrideErrors(errors);
            }

            return errors;
        }

        private void OnValidate()
        {
            foreach (var sliceOverride in _sliceOverrides)
            {
                sliceOverride.RefreshLabel();
            }

            foreach (var error in GetValidationErrors())
            {
                Debug.LogWarning($"{name}: {error}", this);
            }
        }

        private void CollectZoneErrors(List<string> errors)
        {
            if (_zones.Count == 0)
            {
                errors.Add("No zone definitions are assigned.");
                return;
            }

            var ids = new HashSet<string>();
            var hasDefaultZone = false;

            foreach (var zone in _zones)
            {
                if (!zone)
                {
                    errors.Add("The zone list has an empty entry.");
                    continue;
                }

                if (string.IsNullOrEmpty(zone.Id))
                {
                    errors.Add($"{zone.name} has no id.");
                }
                else if (!ids.Add(zone.Id))
                {
                    errors.Add($"More than one zone uses the id '{zone.Id}'.");
                }

                if (zone.Interval == 1)
                {
                    hasDefaultZone = true;
                }

                CollectZoneSliceErrors(zone, errors);
            }

            if (!hasDefaultZone)
            {
                errors.Add("One zone definition needs an interval of 1 so every zone has a definition.");
            }
        }

        private void CollectZoneSliceErrors(ZoneDefinition zone, List<string> errors)
        {
            var reservedSlices = 0;

            if (zone.HasBomb || zone.BombSlotPool)
            {
                reservedSlices = 1;
            }

            if (zone.HasOwnSlices)
            {
                var total = CountSlices(zone.Slices) + reservedSlices;

                if (total != WheelLayout.SLICE_COUNT)
                {
                    errors.Add($"{zone.name} fills {total} slices; a wheel has {WheelLayout.SLICE_COUNT}.");
                }

                return;
            }

            foreach (var band in _bands)
            {
                var total = CountSlices(band.Slices) + reservedSlices;

                if (total != WheelLayout.SLICE_COUNT)
                {
                    errors.Add($"{zone.name} fills {total} slices with the band from zone {band.FirstZone}; a wheel has {WheelLayout.SLICE_COUNT}.");
                }
            }
        }

        private void CollectBandErrors(List<string> errors)
        {
            if (_bands.Count == 0)
            {
                errors.Add("No zone bands are defined.");
                return;
            }

            if (_bands[0].FirstZone != 1)
            {
                errors.Add("The first zone band must start at zone 1.");
            }

            for (var i = 1; i < _bands.Count; i++)
            {
                if (_bands[i].FirstZone <= _bands[i - 1].FirstZone)
                {
                    errors.Add($"Zone bands must be sorted by first zone; band {i} starts at zone {_bands[i].FirstZone}.");
                }
            }
        }

        private void CollectPoolErrors(List<string> errors)
        {
            var pools = new HashSet<RewardPoolDefinition>();

            foreach (var band in _bands)
            {
                CollectPools(band.Slices, pools, errors);
            }

            foreach (var zone in _zones)
            {
                if (!zone)
                {
                    continue;
                }

                CollectPools(zone.Slices, pools, errors);

                if (zone.BombSlotPool)
                {
                    pools.Add(zone.BombSlotPool);
                }
            }

            foreach (var pool in pools)
            {
                pool.CollectValidationErrors(errors);
            }
        }

        private void CollectEconomyErrors(List<string> errors)
        {
            if (!_reviveCurrency)
            {
                errors.Add("The revive currency is not assigned.");
                return;
            }

            if (!_reviveCurrency.IsCurrency)
            {
                errors.Add($"{_reviveCurrency.name} is used for revives but is not marked as a currency.");
            }
        }

        private void CollectOverrideErrors(List<string> errors)
        {
            ZoneRules zoneRules;

            try
            {
                zoneRules = new ZoneRules(new CoreModelFactory().CreateZoneKinds(_zones));
            }
            catch (ArgumentException exception)
            {
                errors.Add(exception.Message);
                return;
            }

            var usedSlots = new HashSet<(int, int)>();
            var appliedByZone = new Dictionary<int, List<SliceOverride>>();

            foreach (var sliceOverride in _sliceOverrides)
            {
                if (!sliceOverride.IsValid)
                {
                    errors.Add($"Zone {sliceOverride.Zone} slice {sliceOverride.SliceIndex} has no reward or a zero amount; it is ignored.");
                    continue;
                }

                var zoneKind = zoneRules.GetZoneKind(sliceOverride.Zone);

                if (sliceOverride.IsBomb && !zoneKind.HasBomb)
                {
                    errors.Add($"Bomb override on zone {sliceOverride.Zone} is ignored because the zone is risk-free.");
                    continue;
                }

                if (!usedSlots.Add((sliceOverride.Zone, sliceOverride.SliceIndex)))
                {
                    errors.Add($"Zone {sliceOverride.Zone} slice {sliceOverride.SliceIndex} has more than one override; the last one wins.");
                }

                if (!appliedByZone.TryGetValue(sliceOverride.Zone, out var zoneOverrides))
                {
                    zoneOverrides = new List<SliceOverride>();
                    appliedByZone.Add(sliceOverride.Zone, zoneOverrides);
                }

                zoneOverrides.Add(sliceOverride);
            }

            foreach (var pair in appliedByZone)
            {
                if (zoneRules.GetZoneKind(pair.Key).HasBomb)
                {
                    CollectBombRuleErrors(pair.Key, pair.Value, errors);
                }
            }
        }

        private static void CollectBombRuleErrors(int zone, List<SliceOverride> zoneOverrides, List<string> errors)
        {
            var lastOrderBySlice = new Dictionary<int, int>();
            var hasBombBySlice = new Dictionary<int, bool>();
            var bombOverrideCount = 0;
            var lastBombSlice = -1;

            for (var order = 0; order < zoneOverrides.Count; order++)
            {
                var sliceOverride = zoneOverrides[order];
                lastOrderBySlice[sliceOverride.SliceIndex] = order;
                hasBombBySlice[sliceOverride.SliceIndex] = sliceOverride.IsBomb;

                if (sliceOverride.IsBomb)
                {
                    bombOverrideCount++;
                    lastBombSlice = sliceOverride.SliceIndex;
                }
            }

            if (bombOverrideCount > 1)
            {
                errors.Add($"Zone {zone} has {bombOverrideCount} bomb overrides; only the last one (slice {lastBombSlice}) stays a bomb.");
            }

            if (lastOrderBySlice.Count < WheelLayout.SLICE_COUNT || hasBombBySlice.ContainsValue(true))
            {
                return;
            }

            var oldestSlice = -1;
            var oldestOrder = int.MaxValue;

            foreach (var pair in lastOrderBySlice)
            {
                if (pair.Value < oldestOrder)
                {
                    oldestOrder = pair.Value;
                    oldestSlice = pair.Key;
                }
            }

            errors.Add($"Zone {zone} overrides every slice with a reward; slice {oldestSlice}, the least recently set one, turns back into the bomb.");
        }

        private static int CountSlices(IReadOnlyList<SliceRuleSettings> slices)
        {
            var total = 0;

            foreach (var slice in slices)
            {
                total += slice.Count;
            }

            return total;
        }

        private static void CollectPools(IReadOnlyList<SliceRuleSettings> slices, HashSet<RewardPoolDefinition> pools, List<string> errors)
        {
            foreach (var slice in slices)
            {
                if (!slice.Pool)
                {
                    errors.Add("A slice rule has no reward pool.");
                    continue;
                }

                pools.Add(slice.Pool);
            }
        }

        #endregion
    }
}
