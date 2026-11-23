using System.Collections.Generic;
using UnityEngine;

namespace WheelGame.Data
{
    [CreateAssetMenu(fileName = "zone_", menuName = "WheelGame/Zone Definition")]
    public sealed class ZoneDefinition : ScriptableObject
    {
        [Header("Rules")]
        [SerializeField] private string _id;
        [SerializeField, Min(1)] private int _interval = 1;
        [SerializeField] private int _priority;
        [SerializeField] private bool _hasBomb = true;
        [SerializeField] private RewardPoolDefinition _bombSlotPool;
        [SerializeField, Min(1)] private int _multiplier = 1;

        [Header("Slices")]
        [SerializeField] private bool _hasOwnSlices;
        [SerializeField] private List<SliceRuleSettings> _slices = new List<SliceRuleSettings>();

        [Header("Visuals")]
        [SerializeField] private ZoneVisuals _visuals = new ZoneVisuals();

        public string Id
        {
            get { return _id; }
        }

        public int Interval
        {
            get { return _interval; }
        }

        public int Priority
        {
            get { return _priority; }
        }

        public bool HasBomb
        {
            get { return _hasBomb; }
        }

        public RewardPoolDefinition BombSlotPool
        {
            get { return _bombSlotPool; }
        }

        public int Multiplier
        {
            get { return _multiplier; }
        }

        public bool HasOwnSlices
        {
            get { return _hasOwnSlices; }
        }

        public IReadOnlyList<SliceRuleSettings> Slices
        {
            get { return _slices; }
        }

        public ZoneVisuals Visuals
        {
            get { return _visuals; }
        }
    }
}
