using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class InMemoryInventory : IInventory
    {
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>();

        public event Action<string, int> ItemChanged;

        public IReadOnlyDictionary<string, int> Items
        {
            get { return _items; }
        }

        public int GetCount(string itemId)
        {
            _items.TryGetValue(itemId, out var count);
            return count;
        }

        public void Add(string itemId, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Inventory amounts cannot be negative.");
            }

            var count = GetCount(itemId) + amount;
            _items[itemId] = count;

            if (ItemChanged != null)
            {
                ItemChanged.Invoke(itemId, count);
            }
        }
    }
}
