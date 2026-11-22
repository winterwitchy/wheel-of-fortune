using System;

namespace WheelGame.Core
{
    public sealed class RevivePricing
    {
        private readonly int _baseCost;
        private readonly float _costMultiplier;

        public RevivePricing(int baseCost, float costMultiplier)
        {
            if (baseCost < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(baseCost), baseCost, "Revive base cost must be at least 1.");
            }

            if (costMultiplier < 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(costMultiplier), costMultiplier, "Revive cost multiplier must be at least 1.");
            }

            _baseCost = baseCost;
            _costMultiplier = costMultiplier;
        }

        public int GetCost(int revivesUsed)
        {
            var cost = _baseCost * Math.Pow(_costMultiplier, revivesUsed);

            if (cost >= int.MaxValue)
            {
                return int.MaxValue;
            }

            return (int)Math.Round(cost, MidpointRounding.AwayFromZero);
        }
    }
}
