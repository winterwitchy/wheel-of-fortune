using NUnit.Framework;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class ProgressPersistenceTests
    {
        private sealed class MemoryProgressStore : IProgressStore
        {
            public ProgressData Stored;
            public int SaveCount;

            public bool TryLoad(out ProgressData data)
            {
                data = Stored;
                return Stored != null;
            }

            public void Save(ProgressData data)
            {
                Stored = data;
                SaveCount++;
            }
        }

        [Test]
        public void Restore_WithoutSave_GivesStartingCurrency()
        {
            var wallet = new InMemoryWallet();
            var persistence = new ProgressPersistence(wallet, new InMemoryInventory(), new MemoryProgressStore());

            persistence.Restore("gold", 100);

            Assert.AreEqual(100, wallet.GetBalance("gold"));
        }

        [Test]
        public void Restore_WithSave_LoadsCurrenciesAndItemsInsteadOfStartingCurrency()
        {
            var store = new MemoryProgressStore { Stored = new ProgressData() };
            store.Stored.Currencies.Add(new ProgressEntry("gold", 40));
            store.Stored.Currencies.Add(new ProgressEntry("cash", 900));
            store.Stored.Items.Add(new ProgressEntry("chest_small", 2));
            var wallet = new InMemoryWallet();
            var inventory = new InMemoryInventory();
            var persistence = new ProgressPersistence(wallet, inventory, store);

            persistence.Restore("gold", 100);

            Assert.AreEqual(40, wallet.GetBalance("gold"));
            Assert.AreEqual(900, wallet.GetBalance("cash"));
            Assert.AreEqual(2, inventory.GetCount("chest_small"));
            Assert.AreEqual(0, store.SaveCount);
        }

        [Test]
        public void Changes_AfterRestore_AreSaved()
        {
            var store = new MemoryProgressStore();
            var wallet = new InMemoryWallet();
            var inventory = new InMemoryInventory();
            var persistence = new ProgressPersistence(wallet, inventory, store);
            persistence.Restore("gold", 100);

            wallet.Add("cash", 250);
            inventory.Add("molotov", 3);

            Assert.AreEqual(2, store.SaveCount);
            Assert.AreEqual(2, store.Stored.Currencies.Count);
            Assert.AreEqual("molotov", store.Stored.Items[0].Id);
            Assert.AreEqual(3, store.Stored.Items[0].Amount);
        }

        [Test]
        public void Dispose_StopsSaving()
        {
            var store = new MemoryProgressStore();
            var wallet = new InMemoryWallet();
            var persistence = new ProgressPersistence(wallet, new InMemoryInventory(), store);
            persistence.Restore("gold", 100);

            persistence.Dispose();
            wallet.Add("gold", 5);

            Assert.AreEqual(0, store.SaveCount);
        }

        [Test]
        public void TransientStore_NeverLoadsSavedProgress()
        {
            var store = new TransientProgressStore();
            store.Save(new ProgressData());

            Assert.IsFalse(store.TryLoad(out _));
        }
    }
}
