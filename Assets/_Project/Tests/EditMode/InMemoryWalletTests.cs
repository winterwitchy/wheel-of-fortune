using NUnit.Framework;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class InMemoryWalletTests
    {
        [Test]
        public void TrySpend_WithEnoughGold_DeductsAndReturnsTrue()
        {
            var wallet = new InMemoryWallet(100);

            Assert.IsTrue(wallet.TrySpend(25));
            Assert.AreEqual(75, wallet.Gold);
        }

        [Test]
        public void TrySpend_WithoutEnoughGold_ReturnsFalseAndKeepsGold()
        {
            var wallet = new InMemoryWallet(10);

            Assert.IsFalse(wallet.TrySpend(25));
            Assert.AreEqual(10, wallet.Gold);
        }

        [Test]
        public void Add_IncreasesGold()
        {
            var wallet = new InMemoryWallet(10);

            wallet.Add(40);

            Assert.AreEqual(50, wallet.Gold);
        }
    }
}