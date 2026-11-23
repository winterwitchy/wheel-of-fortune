using System;
using NUnit.Framework;
using WheelGame.Core;
using WheelGame.Core.Events;

namespace WheelGame.Tests.EditMode
{
    public sealed class InMemoryWalletTests
    {
        private const string GOLD = "gold";
        private const string CASH = "cash";

        [Test]
        public void Add_KeepsEveryCurrencySeparate()
        {
            var wallet = new InMemoryWallet(new EventBus());

            wallet.Add(GOLD, 10);
            wallet.Add(CASH, 500);
            wallet.Add(GOLD, 5);

            Assert.AreEqual(15, wallet.GetBalance(GOLD));
            Assert.AreEqual(500, wallet.GetBalance(CASH));
            Assert.AreEqual(0, wallet.GetBalance("gems"));
        }

        [Test]
        public void TrySpend_WithEnoughBalance_DeductsAndReturnsTrue()
        {
            var wallet = new InMemoryWallet(new EventBus());
            wallet.Add(GOLD, 100);

            Assert.IsTrue(wallet.TrySpend(GOLD, 25));
            Assert.AreEqual(75, wallet.GetBalance(GOLD));
        }

        [Test]
        public void TrySpend_WithoutEnoughBalance_ReturnsFalseAndKeepsBalance()
        {
            var wallet = new InMemoryWallet(new EventBus());
            wallet.Add(GOLD, 10);
            wallet.Add(CASH, 1000);

            Assert.IsFalse(wallet.TrySpend(GOLD, 25));
            Assert.AreEqual(10, wallet.GetBalance(GOLD));
        }

        [Test]
        public void Add_RaisesBalanceChangedWithTheNewBalance()
        {
            var bus = new EventBus();
            var wallet = new InMemoryWallet(bus);
            wallet.Add(GOLD, 10);
            string changedId = null;
            var changedBalance = 0;
            bus.Subscribe<BalanceChangedEvent>(message =>
            {
                changedId = message.CurrencyId;
                changedBalance = message.Balance;
            });

            wallet.Add(GOLD, 40);

            Assert.AreEqual(GOLD, changedId);
            Assert.AreEqual(50, changedBalance);
        }

        [Test]
        public void Add_NegativeAmount_Throws()
        {
            var wallet = new InMemoryWallet(new EventBus());

            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Add(GOLD, -1));
        }
    }
}
