using System;

namespace WheelGame.Core
{
    public sealed class SliceOverrideRule
    {
        #region Constructors

        private SliceOverrideRule(int zone, int sliceIndex, bool isBomb, RewardItem reward, int amount)
        {
            Zone = zone;
            SliceIndex = sliceIndex;
            IsBomb = isBomb;
            Reward = reward;
            Amount = amount;
        }

        #endregion

        #region Properties

        public int Zone { get; }

        public int SliceIndex { get; }

        public bool IsBomb { get; }

        public RewardItem Reward { get; }

        public int Amount { get; }

        #endregion

        #region Public Methods

        public static SliceOverrideRule CreateBomb(int zone, int sliceIndex)
        {
            return new SliceOverrideRule(zone, sliceIndex, true, null, 0);
        }

        public static SliceOverrideRule CreateReward(int zone, int sliceIndex, RewardItem reward, int amount)
        {
            if (reward == null)
            {
                throw new ArgumentNullException(nameof(reward));
            }

            if (amount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Override amount must be at least 1.");
            }

            return new SliceOverrideRule(zone, sliceIndex, false, reward, amount);
        }

        public WheelSliceEntry ToSliceEntry()
        {
            if (IsBomb)
            {
                return WheelSliceEntry.CreateBomb();
            }

            return WheelSliceEntry.CreateReward(Reward, Amount);
        }

        #endregion
    }
}
