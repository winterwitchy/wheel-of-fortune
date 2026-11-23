namespace WheelGame.Core.Events
{
    public readonly struct BalanceChangedEvent
    {
        public BalanceChangedEvent(string currencyId, int balance)
        {
            CurrencyId = currencyId;
            Balance = balance;
        }

        public string CurrencyId { get; }

        public int Balance { get; }
    }

    public readonly struct InventoryChangedEvent
    {
        public InventoryChangedEvent(string itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }

        public string ItemId { get; }

        public int Count { get; }
    }
}
