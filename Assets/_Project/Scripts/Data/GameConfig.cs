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
        [SerializeField] private LeaveRule _leaveRule = LeaveRule.BeforeAnySpin;

        [Header("Revive")]
        [SerializeField, Min(0)] private int _startingGold = 100;
        [SerializeField, Min(1)] private int _reviveCost = 25;

        [Header("Wheel Generation")]
        [SerializeField] private WheelGenerationSettings _generation = new WheelGenerationSettings();

        [Header("Overrides")]
        [SerializeField] private List<SliceOverride> _sliceOverrides = new List<SliceOverride>();

        public int SafeInterval => _safeInterval;
        public int SuperInterval => _superInterval;
        public LeaveRule LeaveRule => _leaveRule;
        public int StartingGold => _startingGold;
        public int ReviveCost => _reviveCost;
        public WheelGenerationSettings Generation => _generation;
        public IReadOnlyList<SliceOverride> SliceOverrides => _sliceOverrides;

        private void OnValidate()
        {
            foreach (var sliceOverride in _sliceOverrides)
                sliceOverride.RefreshLabel();

            foreach (var error in _generation.GetValidationErrors())
                Debug.LogWarning($"{name}: {error}", this);

            ValidateOverrides();
        }

        private void ValidateOverrides()
        {
            var zoneRules = new ZoneRules(_safeInterval, _superInterval);
            var usedSlots = new HashSet<(int, int)>();

            foreach (var sliceOverride in _sliceOverrides)
            {
                if (sliceOverride.IsBomb && zoneRules.GetZoneType(sliceOverride.Zone) != ZoneType.Normal)
                    Debug.LogWarning($"{name}: bomb override on zone {sliceOverride.Zone} is ignored because the zone is risk-free.", this);

                if (!usedSlots.Add((sliceOverride.Zone, sliceOverride.SliceIndex)))
                    Debug.LogWarning($"{name}: zone {sliceOverride.Zone} slice {sliceOverride.SliceIndex} has more than one override; the last one wins.", this);
            }
        }
    }
}