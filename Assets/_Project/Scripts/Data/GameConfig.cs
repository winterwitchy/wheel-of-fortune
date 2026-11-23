using System.Collections.Generic;
using UnityEngine;
using WheelGame.Core;

namespace WheelGame.Data
{
    [CreateAssetMenu(fileName = "game_config", menuName = "WheelGame/Game Config")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Zones")]
        [SerializeField, Min(1)] private int _safeInterval = 5;
        [SerializeField, Min(1)] private int _superInterval = 30;

        [Header("Rules")]
        [SerializeField] private LeaveRule _leaveRule = LeaveRule.AfterFirstReward;

        [Header("Economy")]
        [SerializeField, Min(0)] private int _startingGold = 100;
        [SerializeField, Min(1)] private int _reviveBaseCost = 25;
        [SerializeField, Min(1f)] private float _reviveCostMultiplier = 2f;

        [Header("Wheel Generation")]
        [SerializeField] private WheelGenerationSettings _generation = new WheelGenerationSettings();

        [Header("Overrides")]
        [SerializeField] private List<SliceOverride> _sliceOverrides = new List<SliceOverride>();

        public int SafeInterval => _safeInterval;
        public int SuperInterval => _superInterval;
        public LeaveRule LeaveRule => _leaveRule;
        public int StartingGold => _startingGold;
        public int ReviveBaseCost => _reviveBaseCost;
        public float ReviveCostMultiplier => _reviveCostMultiplier;
        public WheelGenerationSettings Generation => _generation;
        public IReadOnlyList<SliceOverride> SliceOverrides => _sliceOverrides;

        private void OnValidate()
        {
            foreach (var sliceOverride in _sliceOverrides)
                sliceOverride.RefreshLabel();

            foreach (var error in _generation.GetValidationErrors())
                Debug.LogWarning($"{name}: {error}", this);

            if (_generation.GoldReward && !_generation.GoldReward.IsCurrency)
            {
                Debug.LogWarning($"{name}: the gold reward is used for revives and must be marked as a currency.", this);
            }

            ValidateOverrides();
        }

        private void ValidateOverrides()
        {
            var zoneRules = new ZoneRules(_safeInterval, _superInterval);
            var usedSlots = new HashSet<(int, int)>();
            var appliedByZone = new Dictionary<int, List<SliceOverride>>();

            foreach (var sliceOverride in _sliceOverrides)
            {
                if (!sliceOverride.IsValid)
                {
                    Debug.LogWarning($"{name}: zone {sliceOverride.Zone} slice {sliceOverride.SliceIndex} has no reward or a zero amount; it is ignored.", this);
                    continue;
                }

                var zoneType = zoneRules.GetZoneType(sliceOverride.Zone);

                if (sliceOverride.IsBomb && WheelBombRule.GetRequiredBombCount(zoneType) == 0)
                {
                    Debug.LogWarning($"{name}: bomb override on zone {sliceOverride.Zone} is ignored because the zone is risk-free.", this);
                    continue;
                }

                if (!usedSlots.Add((sliceOverride.Zone, sliceOverride.SliceIndex)))
                {
                    Debug.LogWarning($"{name}: zone {sliceOverride.Zone} slice {sliceOverride.SliceIndex} has more than one override; the last one wins.", this);
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
                if (zoneRules.GetZoneType(pair.Key) == ZoneType.Normal)
                {
                    ValidateBombRule(pair.Key, pair.Value);
                }
            }
        }

        private void ValidateBombRule(int zone, List<SliceOverride> zoneOverrides)
        {
            var lastOrderBySlice = new Dictionary<int, int>();
            var lastKindBySlice = new Dictionary<int, bool>();
            var bombOverrideCount = 0;
            var lastBombSlice = -1;

            for (var order = 0; order < zoneOverrides.Count; order++)
            {
                var sliceOverride = zoneOverrides[order];
                lastOrderBySlice[sliceOverride.SliceIndex] = order;
                lastKindBySlice[sliceOverride.SliceIndex] = sliceOverride.IsBomb;

                if (sliceOverride.IsBomb)
                {
                    bombOverrideCount++;
                    lastBombSlice = sliceOverride.SliceIndex;
                }
            }

            if (bombOverrideCount > 1)
            {
                Debug.LogWarning($"{name}: zone {zone} has {bombOverrideCount} bomb overrides; only the last one (slice {lastBombSlice}) stays a bomb.", this);
            }

            if (lastOrderBySlice.Count < WheelLayout.SliceCount || lastKindBySlice.ContainsValue(true))
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

            Debug.LogWarning($"{name}: zone {zone} overrides every slice with a reward; slice {oldestSlice}, the least recently set one, turns back into the bomb.", this);
        }
    }
}