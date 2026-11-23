using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class ItemPoolSource : IRewardSource
    {
        #region Fields

        private readonly IReadOnlyList<RewardItem> _items;
        private readonly AmountFormula _amountFormula;
        private readonly bool _isUniquePerWheel;
        private IRewardSource _fallback;

        #endregion

        #region Constructors

        public ItemPoolSource(IReadOnlyList<RewardItem> items, AmountFormula amountFormula, bool isUniquePerWheel)
        {
            if (items == null || items.Count == 0)
            {
                throw new ArgumentException("A reward pool needs at least one item.", nameof(items));
            }

            _items = items;
            _amountFormula = amountFormula;
            _isUniquePerWheel = isUniquePerWheel;
        }

        #endregion

        #region Public Methods

        public void SetFallback(IRewardSource fallback)
        {
            _fallback = fallback;
        }

        public void AddTo(WheelDraft draft, int zone, int multiplier, IRandomProvider random)
        {
            var unused = draft.CollectUnused(_items);

            if (unused.Count > 0)
            {
                draft.AddReward(PickOne(unused, random), GetAmount(zone, multiplier));
                return;
            }

            if (_isUniquePerWheel && _fallback != null)
            {
                _fallback.AddTo(draft, zone, multiplier, random);
                return;
            }

            draft.AddReward(PickOne(_items, random), GetAmount(zone, multiplier));
        }

        #endregion

        #region Private Methods

        private int GetAmount(int zone, int multiplier)
        {
            if (_amountFormula == null)
            {
                return 1;
            }

            return _amountFormula.Evaluate(zone) * multiplier;
        }

        private static RewardItem PickOne(IReadOnlyList<RewardItem> items, IRandomProvider random)
        {
            if (items.Count == 1)
            {
                return items[0];
            }

            return items[random.Range(0, items.Count)];
        }

        #endregion
    }
}
