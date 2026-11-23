namespace WheelGame.Core
{
    public interface IRewardSource
    {
        void AddTo(WheelDraft draft, int zone, int multiplier, IRandomProvider random);
    }
}
