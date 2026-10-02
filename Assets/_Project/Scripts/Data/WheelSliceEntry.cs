namespace WheelGame.Data
{
    public sealed class WheelSliceEntry
    {
        private WheelSliceEntry(SliceKind kind, RewardItemData reward, int amount)
        {
            Kind = kind;
            Reward = reward;
            Amount = amount;
        }

        public SliceKind Kind { get; }
        public RewardItemData Reward { get; }
        public int Amount { get; }
        public bool IsBomb => Kind == SliceKind.Bomb;

        public static WheelSliceEntry CreateReward(RewardItemData reward, int amount)
        {
            return new WheelSliceEntry(SliceKind.Reward, reward, amount);
        }

        public static WheelSliceEntry CreateBomb()
        {
            return new WheelSliceEntry(SliceKind.Bomb, null, 0);
        }
    }
}