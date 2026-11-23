using System;

namespace WheelGame.Core
{
    public sealed class RevivePricing
    {
        #region Fields

        private readonly int _baseCost;
        private readonly float _costMultiplier;

        #endregion

        #region Constructors

        public RevivePricing(string currencyId, int baseCost, float costMultiplier)
        {
            if (string.IsNullOrEmpty(currencyId))
            {
                throw new ArgumentException("Revive currency id is missing.", nameof(currencyId));
            }

            if (baseCost < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(baseCost), baseCost, "Revive base cost must be at least 1.");
            }

            if (costMultiplier < 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(costMultiplier), costMultiplier, "Revive cost multiplier must be at least 1.");
            }

            CurrencyId = currencyId;
            _baseCost = baseCost;
            _costMultiplier = costMultiplier;
        }

        #endregion

        #region Properties

        public string CurrencyId { get; }

        #endregion

        #region Public Methods

        public int GetCost(int revivesUsed)
        {
            var cost = _baseCost * Math.Pow(_costMultiplier, revivesUsed);

            if (cost >= int.MaxValue)
            {
                return int.MaxValue;
            }

            return (int)Math.Round(cost, MidpointRounding.AwayFromZero);
        }

        #endregion
    }
}
