using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class WheelGenerationSettings
    {
        [Header("Cash")]
        [SerializeField] private RewardItemData _cashReward;
        [SerializeField] private AmountFormula _cashFormula = new AmountFormula(GrowthCurve.Quadratic, 10, 1, 25);

        [Header("Gold")]
        [SerializeField] private RewardItemData _goldReward;
        [SerializeField] private AmountFormula _goldFormula = new AmountFormula(GrowthCurve.Linear, 1, 0, 1);

        [Header("Points")]
        [SerializeField] private List<RewardItemData> _pointRewards = new List<RewardItemData>();
        [SerializeField] private AmountFormula _pointFormula = new AmountFormula(GrowthCurve.Linear, 2, 1, 5);

        [Header("Consumables")]
        [FormerlySerializedAs("_itemRewards")]
        [SerializeField] private List<RewardItemData> _consumableRewards = new List<RewardItemData>();
        [SerializeField] private AmountFormula _consumableFormula = new AmountFormula(GrowthCurve.SquareRoot, 1, 0, 2);

        [Header("Skins")]
        [SerializeField] private List<RewardItemData> _safeSkinRewards = new List<RewardItemData>();
        [FormerlySerializedAs("_superItemRewards")]
        [SerializeField] private List<RewardItemData> _superSkinRewards = new List<RewardItemData>();

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
            List<RewardItemData> consumableRewards, AmountFormula consumableFormula,
            List<RewardItemData> safeSkinRewards, List<RewardItemData> superSkinRewards,
            ChestProgression chestProgression, RewardItemData superChestReward,
            int safeMultiplier, int superMultiplier)
        {
            _cashReward = cashReward;
            _cashFormula = cashFormula;
            _goldReward = goldReward;
            _goldFormula = goldFormula;
            _pointRewards = pointRewards;
            _pointFormula = pointFormula;
            _consumableRewards = consumableRewards;
            _consumableFormula = consumableFormula;
            _safeSkinRewards = safeSkinRewards;
            _superSkinRewards = superSkinRewards;
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
        public IReadOnlyList<RewardItemData> ConsumableRewards => _consumableRewards;
        public AmountFormula ConsumableFormula => _consumableFormula;
        public IReadOnlyList<RewardItemData> SafeSkinRewards => _safeSkinRewards;
        public IReadOnlyList<RewardItemData> SuperSkinRewards => _superSkinRewards;
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

            if (_consumableRewards.Count == 0)
                errors.Add("Consumable reward pool is empty.");

            if (_safeSkinRewards.Count == 0)
                errors.Add("Safe skin reward pool is empty.");

            if (_superSkinRewards.Count == 0)
                errors.Add("Super skin reward pool is empty.");

            if (_chestProgression.Sequence.Count == 0)
                errors.Add("Chest progression is empty.");

            return errors;
        }
    }
}