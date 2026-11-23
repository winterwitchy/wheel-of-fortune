namespace WheelGame.Core
{
    public sealed class WheelSliceEntry
    {
        private WheelSliceEntry(SliceKind kind, RewardItem reward, int amount)
        {
            Kind = kind;
            Reward = reward;
            Amount = amount;
        }

        public SliceKind Kind { get; }

        public RewardItem Reward { get; }

        public int Amount { get; }

        public bool IsBomb
        {
            get { return Kind == SliceKind.Bomb; }
        }

        public static WheelSliceEntry CreateReward(RewardItem reward, int amount)
        {
            return new WheelSliceEntry(SliceKind.Reward, reward, amount);
        }

        public static WheelSliceEntry CreateBomb()
        {
            return new WheelSliceEntry(SliceKind.Bomb, null, 0);
        }
    }
}
