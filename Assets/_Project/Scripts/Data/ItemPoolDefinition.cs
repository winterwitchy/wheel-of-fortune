using System.Collections.Generic;
using UnityEngine;
using WheelGame.Core;

namespace WheelGame.Data
{
    [CreateAssetMenu(fileName = "pool_", menuName = "WheelGame/Reward Pools/Item Pool")]
    public sealed class ItemPoolDefinition : RewardPoolDefinition
    {
        #region Serialized Fields

        [SerializeField] private List<RewardItemData> _items = new List<RewardItemData>();
        [SerializeField] private bool _hasAmountFormula = true;
        [SerializeField] private AmountFormulaSettings _amountFormula = new AmountFormulaSettings();
        [SerializeField] private bool _isUniquePerWheel;
        [SerializeField] private RewardPoolDefinition _fallbackPool;

        #endregion

        #region Public Methods

        public override IRewardSource CreateSource(CoreModelFactory factory)
        {
            AmountFormula formula = null;

            if (_hasAmountFormula)
            {
                formula = _amountFormula.ToFormula();
            }

            return new ItemPoolSource(factory.GetRewards(_items), formula, _isUniquePerWheel);
        }

        public override void LinkSource(IRewardSource source, CoreModelFactory factory)
        {
            if (!_fallbackPool)
            {
                return;
            }

            ((ItemPoolSource)source).SetFallback(factory.GetSource(_fallbackPool));
        }

        public override void CollectValidationErrors(List<string> errors)
        {
            CollectItemErrors(_items, errors);

            if (_isUniquePerWheel && !_fallbackPool)
            {
                errors.Add($"{name} is unique per wheel but has no fallback pool; it repeats rewards when it runs out.");
            }

            if (_fallbackPool == this)
            {
                errors.Add($"{name} uses itself as its fallback pool.");
            }
        }

        #endregion
    }
}
