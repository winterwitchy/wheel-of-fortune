using WheelGame.Data;

namespace WheelGame.Core
{
    public interface IRewardVault
    {
        void Deposit(RewardItemData reward, int amount);
    }
}
