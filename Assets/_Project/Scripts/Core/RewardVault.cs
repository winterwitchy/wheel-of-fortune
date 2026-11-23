namespace WheelGame.Core
{
    public sealed class RewardVault : IRewardVault
    {
        #region Fields

        private readonly IWallet _wallet;
        private readonly IInventory _inventory;

        #endregion

        #region Constructors

        public RewardVault(IWallet wallet, IInventory inventory)
        {
            _wallet = wallet;
            _inventory = inventory;
        }

        #endregion

        #region Public Methods

        public void Deposit(RewardItem reward, int amount)
        {
            if (reward.IsCurrency)
            {
                _wallet.Add(reward.Id, amount);
                return;
            }

            _inventory.Add(reward.Id, amount);
        }

        #endregion
    }
}
