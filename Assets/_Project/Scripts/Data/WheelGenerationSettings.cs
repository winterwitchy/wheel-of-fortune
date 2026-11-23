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
        [SerializeField] private AmountFormula _cashFormula = new AmountFormula(GrowthCurve.Quadratic, 10, 1, 25);

        [Header("Gold")]
        [SerializeField] private RewardItemData _goldReward;
        [SerializeField] private AmountFormula _goldFormula = new AmountFormula(GrowthCurve.Linear, 1, 0, 1);

        [Header("Points And Consumables")]
        [SerializeField] private AmountFormula _pointFormula = new AmountFormula(GrowthCurve.Linear, 2, 1, 5);
        [SerializeField] private AmountFormula _consumableFormula = new AmountFormula(GrowthCurve.SquareRoot, 1, 0, 2);

        [Header("Zone Bands")]
        [SerializeField] private List<ZoneBand> _bands = new List<ZoneBand>();

        [Header("Safe Zone")]
        [SerializeField] private List<RewardItemData> _safeSkinRewards = new List<RewardItemData>();
        [SerializeField, Min(1)] private int _safeMultiplier = 2;

        [Header("Super Zone")]
        [SerializeField] private SliceComposition _superSlices = new SliceComposition(1, 1, 1, 1, 2, 2);
        [SerializeField] private List<RewardItemData> _superSkinRewards = new List<RewardItemData>();
        [SerializeField] private RewardItemData _superChestReward;
        [SerializeField, Min(1)] private int _superMultiplier = 5;

        [Header("Chests")]
        [SerializeField] private ChestProgression _chestProgression = new ChestProgression();

        public WheelGenerationSettings()
        {
        }

        public WheelGenerationSettings(
            RewardItemData cashReward, AmountFormula cashFormula,
            RewardItemData goldReward, AmountFormula goldFormula,
            AmountFormula pointFormula, AmountFormula consumableFormula,
            List<ZoneBand> bands,
            List<RewardItemData> safeSkinRewards, int safeMultiplier,
            SliceComposition superSlices, List<RewardItemData> superSkinRewards, RewardItemData superChestReward, int superMultiplier,
            ChestProgression chestProgression)
        {
            _cashReward = cashReward;
            _cashFormula = cashFormula;
            _goldReward = goldReward;
            _goldFormula = goldFormula;
            _pointFormula = pointFormula;
            _consumableFormula = consumableFormula;
            _bands = bands;
            _safeSkinRewards = safeSkinRewards;
            _safeMultiplier = safeMultiplier;
            _superSlices = superSlices;
            _superSkinRewards = superSkinRewards;
            _superChestReward = superChestReward;
            _superMultiplier = superMultiplier;
            _chestProgression = chestProgression;
        }

        public RewardItemData CashReward => _cashReward;
        public AmountFormula CashFormula => _cashFormula;
        public RewardItemData GoldReward => _goldReward;
        public AmountFormula GoldFormula => _goldFormula;
        public AmountFormula PointFormula => _pointFormula;
        public AmountFormula ConsumableFormula => _consumableFormula;
        public IReadOnlyList<RewardItemData> SafeSkinRewards => _safeSkinRewards;
        public int SafeMultiplier => _safeMultiplier;
        public SliceComposition SuperSlices => _superSlices;
        public IReadOnlyList<RewardItemData> SuperSkinRewards => _superSkinRewards;
        public RewardItemData SuperChestReward => _superChestReward;
        public int SuperMultiplier => _superMultiplier;
        public ChestProgression ChestProgression => _chestProgression;

        public ZoneBand GetBand(int zone)
        {
            ZoneBand result = null;

            foreach (var band in _bands)
            {
                if (band.FirstZone > zone)
                {
                    break;
                }

                result = band;
            }

            if (result == null && _bands.Count > 0)
            {
                result = _bands[0];
            }

            return result;
        }

        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (!_cashReward)
            {
                errors.Add("Cash reward is not assigned.");
            }

            if (!_goldReward)
            {
                errors.Add("Gold reward is not assigned.");
            }

            if (!_superChestReward)
            {
                errors.Add("Super chest reward is not assigned.");
            }

            if (_safeSkinRewards.Count == 0)
            {
                errors.Add("Safe skin reward pool is empty.");
            }

            if (_superSlices.Skins > 0 && _superSkinRewards.Count == 0)
            {
                errors.Add("Super zone has skin slices but the super skin pool is empty.");
            }

            if (_superSlices.Total != WheelLayout.SliceCount)
            {
                errors.Add($"Super zone defines {_superSlices.Total} slices; it needs {WheelLayout.SliceCount}.");
            }

            if (_chestProgression.Sequence.Count == 0)
            {
                errors.Add("Chest progression is empty.");
            }

            CollectBandErrors(errors);
            return errors;
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

            for (var i = 0; i < _bands.Count; i++)
            {
                if (i > 0 && _bands[i].FirstZone <= _bands[i - 1].FirstZone)
                {
                    errors.Add($"Zone bands must be sorted by first zone; band {i} starts at zone {_bands[i].FirstZone}.");
                }

                _bands[i].CollectValidationErrors(errors);
            }
        }
    }
}
