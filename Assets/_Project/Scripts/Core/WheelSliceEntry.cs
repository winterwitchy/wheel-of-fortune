namespace WheelGame.Core
{
    public sealed class WheelSliceEntry
    {
        #region Constructors

        private WheelSliceEntry(SliceKind kind, RewardItem reward, int amount)
        {
            Kind = kind;
            Reward = reward;
            Amount = amount;
        }

        #endregion

        #region Properties

        public SliceKind Kind { get; }

        public RewardItem Reward { get; }

        public int Amount { get; }

        public bool IsBomb
        {
            get { return Kind == SliceKind.Bomb; }
        }

        #endregion

        #region Public Methods

        public static WheelSliceEntry CreateReward(RewardItem reward, int amount)
        {
            return new WheelSliceEntry(SliceKind.Reward, reward, amount);
        }

        public static WheelSliceEntry CreateBomb()
        {
            return new WheelSliceEntry(SliceKind.Bomb, null, 0);
        }

        #endregion
    }
}
