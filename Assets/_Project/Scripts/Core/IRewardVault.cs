namespace WheelGame.Core
{
    public interface IRewardVault
    {
        void Deposit(RewardItem reward, int amount);
    }
}
