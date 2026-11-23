using NUnit.Framework;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class RewardVaultTests
    {
        [Test]
        public void Deposit_CurrencyGoesToWalletAndItemGoesToInventory()
        {
            var wallet = new InMemoryWallet();
            var inventory = new InMemoryInventory();
            var vault = new RewardVault(wallet, inventory);
            var cash = TestRewards.Create("cash", true);
            var chest = TestRewards.Create("chest_small", false);

            vault.Deposit(cash, 300);
            vault.Deposit(chest, 2);

            Assert.AreEqual(300, wallet.GetBalance("cash"));
            Assert.AreEqual(0, inventory.GetCount("cash"));
            Assert.AreEqual(2, inventory.GetCount("chest_small"));
            Assert.AreEqual(0, wallet.GetBalance("chest_small"));
        }
    }
}
