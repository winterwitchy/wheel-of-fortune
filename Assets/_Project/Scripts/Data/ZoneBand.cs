using System;
using System.Collections.Generic;
using UnityEngine;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class ZoneBand
    {
        [SerializeField, Min(1)] private int _firstZone = 1;
        [SerializeField] private SliceComposition _slices = new SliceComposition();
        [SerializeField] private List<RewardItemData> _pointRewards = new List<RewardItemData>();
        [SerializeField] private List<RewardItemData> _consumableRewards = new List<RewardItemData>();
        [SerializeField] private List<RewardItemData> _skinRewards = new List<RewardItemData>();

        public ZoneBand()
        {
        }

        public ZoneBand(int firstZone, SliceComposition slices, List<RewardItemData> pointRewards, List<RewardItemData> consumableRewards, List<RewardItemData> skinRewards)
        {
            _firstZone = firstZone;
            _slices = slices;
            _pointRewards = pointRewards;
            _consumableRewards = consumableRewards;
            _skinRewards = skinRewards;
        }

        public int FirstZone
        {
            get { return _firstZone; }
        }

        public SliceComposition Slices
        {
            get { return _slices; }
        }

        public IReadOnlyList<RewardItemData> PointRewards
        {
            get { return _pointRewards; }
        }

        public IReadOnlyList<RewardItemData> ConsumableRewards
        {
            get { return _consumableRewards; }
        }

        public IReadOnlyList<RewardItemData> SkinRewards
        {
            get { return _skinRewards; }
        }

        public void CollectValidationErrors(List<string> errors)
        {
            var expectedTotal = WheelLayout.SliceCount - 1;

            if (_slices.Total != expectedTotal)
            {
                errors.Add($"Band from zone {_firstZone} defines {_slices.Total} reward slices; it needs {expectedTotal} next to the bomb.");
            }

            if (_slices.Points > 0 && _pointRewards.Count == 0)
            {
                errors.Add($"Band from zone {_firstZone} has point slices but no point rewards.");
            }

            if (_slices.Consumables > 0 && _consumableRewards.Count == 0)
            {
                errors.Add($"Band from zone {_firstZone} has consumable slices but no consumable rewards.");
            }

            if (_slices.Skins > 0 && _skinRewards.Count == 0)
            {
                errors.Add($"Band from zone {_firstZone} has skin slices but no skin rewards.");
            }

            CollectMissingRewards(_pointRewards, "point", errors);
            CollectMissingRewards(_consumableRewards, "consumable", errors);
            CollectMissingRewards(_skinRewards, "skin", errors);
        }

        private void CollectMissingRewards(List<RewardItemData> pool, string poolName, List<string> errors)
        {
            foreach (var reward in pool)
            {
                if (!reward)
                {
                    errors.Add($"Band from zone {_firstZone} has an empty entry in its {poolName} rewards.");
                    return;
                }
            }
        }
    }
}
