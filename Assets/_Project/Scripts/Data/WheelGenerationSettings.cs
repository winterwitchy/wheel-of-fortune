using System;
using System.Collections.Generic;
using UnityEngine;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class WheelGenerationSettings
    {
        [Header("Cash")]
        [SerializeField] private RewardItemData _cashReward;
        [SerializeField] private AmountFormula _cashFormula = new AmountFormula(2, 10, 1, 25);

        [Header("Gold")]
        [SerializeField] private RewardItemData _goldReward;
        [SerializeField] private AmountFormula _goldFormula = new AmountFormula(1, 1, 0, 1);

        [Header("Points")]
        [SerializeField] private List<RewardItemData> _pointRewards = new List<RewardItemData>();
        [SerializeField] private AmountFormula _pointFormula = new AmountFormula(1, 2, 1, 5);

        [Header("Items")]
        [SerializeField] private List<RewardItemData> _itemRewards = new List<RewardItemData>();
        [SerializeField] private List<RewardItemData> _superItemRewards = new List<RewardItemData>();

        [Header("Chests")]
        [SerializeField] private ChestProgression _chestProgression = new ChestProgression();
        [SerializeField] private RewardItemData _superChestReward;

        [Header("Multipliers")]
        [SerializeField, Min(1)] private int _safeMultiplier = 1;
        [SerializeField, Min(1)] private int _superMultiplier = 5;

        public WheelGenerationSettings() { }

        public WheelGenerationSettings(
            RewardItemData cashReward, AmountFormula cashFormula,
            RewardItemData goldReward, AmountFormula goldFormula,
            List<RewardItemData> pointRewards, AmountFormula pointFormula,
            List<RewardItemData> itemRewards, List<RewardItemData> superItemRewards,
            ChestProgression chestProgression, RewardItemData superChestReward,
            int safeMultiplier, int superMultiplier)
        {
            _cashReward = cashReward;
            _cashFormula = cashFormula;
            _goldReward = goldReward;
            _goldFormula = goldFormula;
            _pointRewards = pointRewards;
            _pointFormula = pointFormula;
            _itemRewards = itemRewards;
            _superItemRewards = superItemRewards;
            _chestProgression = chestProgression;
            _superChestReward = superChestReward;
            _safeMultiplier = safeMultiplier;
            _superMultiplier = superMultiplier;
        }

        public RewardItemData CashReward => _cashReward;
        public AmountFormula CashFormula => _cashFormula;
        public RewardItemData GoldReward => _goldReward;
        public AmountFormula GoldFormula => _goldFormula;
        public IReadOnlyList<RewardItemData> PointRewards => _pointRewards;
        public AmountFormula PointFormula => _pointFormula;
        public IReadOnlyList<RewardItemData> ItemRewards => _itemRewards;
        public IReadOnlyList<RewardItemData> SuperItemRewards => _superItemRewards;
        public ChestProgression ChestProgression => _chestProgression;
        public RewardItemData SuperChestReward => _superChestReward;
        public int SafeMultiplier => _safeMultiplier;
        public int SuperMultiplier => _superMultiplier;

        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (_cashReward == null)
                errors.Add("Cash reward is not assigned.");

            if (_goldReward == null)
                errors.Add("Gold reward is not assigned.");

            if (_superChestReward == null)
                errors.Add("Super chest reward is not assigned.");

            if (_pointRewards.Count == 0)
                errors.Add("Point reward pool is empty.");

            if (_itemRewards.Count == 0)
                errors.Add("Item reward pool is empty.");

            if (_superItemRewards.Count == 0)
                errors.Add("Super item reward pool is empty.");

            if (_chestProgression.Sequence.Count == 0)
                errors.Add("Chest progression is empty.");

            return errors;
        }
    }
}