using NUnit.Framework;
using WheelGame.Core;
using WheelGame.Core.Events;

namespace WheelGame.Tests.EditMode
{
    public sealed class InMemoryInventoryTests
    {
        [Test]
        public void Add_AccumulatesCountsPerItem()
        {
            var inventory = new InMemoryInventory(new EventBus());

            inventory.Add("chest_small", 1);
            inventory.Add("molotov", 4);
            inventory.Add("chest_small", 2);

            Assert.AreEqual(3, inventory.GetCount("chest_small"));
            Assert.AreEqual(4, inventory.GetCount("molotov"));
            Assert.AreEqual(0, inventory.GetCount("shotgun_t1"));
        }

        [Test]
        public void Add_RaisesItemChanged()
        {
            var bus = new EventBus();
            var inventory = new InMemoryInventory(bus);
            var changedCount = 0;
            bus.Subscribe<InventoryChangedEvent>(message => changedCount = message.Count);

            inventory.Add("molotov", 4);

            Assert.AreEqual(4, changedCount);
        }
    }
}
