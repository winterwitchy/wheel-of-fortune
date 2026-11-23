using System.Collections.Generic;

namespace WheelGame.Core
{
    public interface IInventory
    {
        IReadOnlyDictionary<string, int> Items { get; }

        int GetCount(string itemId);

        void Add(string itemId, int amount);
    }
}
