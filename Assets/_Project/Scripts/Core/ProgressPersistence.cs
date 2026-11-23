using System;
using System.Collections.Generic;
using WheelGame.Core.Events;

namespace WheelGame.Core
{
    public sealed class ProgressPersistence : IDisposable
    {
        #region Fields

        private readonly IWallet _wallet;
        private readonly IInventory _inventory;
        private readonly IProgressStore _store;
        private readonly IEventBus _bus;
        private bool _isTracking;

        #endregion

        #region Constructors

        public ProgressPersistence(IWallet wallet, IInventory inventory, IProgressStore store, IEventBus bus)
        {
            _wallet = wallet;
            _inventory = inventory;
            _store = store;
            _bus = bus;
        }

        #endregion

        #region Public Methods

        public void Restore(string startingCurrencyId, int startingAmount)
        {
            if (_store.TryLoad(out var data))
            {
                Apply(data);
            }
            else
            {
                _wallet.Add(startingCurrencyId, startingAmount);
            }

            StartTracking();
        }

        public void Dispose()
        {
            if (!_isTracking)
            {
                return;
            }

            _bus.Unsubscribe<BalanceChangedEvent>(HandleBalanceChanged);
            _bus.Unsubscribe<InventoryChangedEvent>(HandleInventoryChanged);
            _isTracking = false;
        }

        #endregion

        #region Private Methods

        private void Apply(ProgressData data)
        {
            foreach (var entry in data.Currencies)
            {
                if (IsUsable(entry))
                {
                    _wallet.Add(entry.Id, entry.Amount);
                }
            }

            foreach (var entry in data.Items)
            {
                if (IsUsable(entry))
                {
                    _inventory.Add(entry.Id, entry.Amount);
                }
            }
        }

        private static bool IsUsable(ProgressEntry entry)
        {
            if (entry == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(entry.Id))
            {
                return false;
            }

            return entry.Amount >= 0;
        }

        private void StartTracking()
        {
            if (_isTracking)
            {
                return;
            }

            _bus.Subscribe<BalanceChangedEvent>(HandleBalanceChanged);
            _bus.Subscribe<InventoryChangedEvent>(HandleInventoryChanged);
            _isTracking = true;
        }

        private void HandleBalanceChanged(BalanceChangedEvent message)
        {
            _store.Save(CreateSnapshot());
        }

        private void HandleInventoryChanged(InventoryChangedEvent message)
        {
            _store.Save(CreateSnapshot());
        }

        private ProgressData CreateSnapshot()
        {
            var data = new ProgressData();
            CopyEntries(_wallet.Balances, data.Currencies);
            CopyEntries(_inventory.Items, data.Items);
            return data;
        }

        private static void CopyEntries(IReadOnlyDictionary<string, int> source, List<ProgressEntry> target)
        {
            foreach (var pair in source)
            {
                target.Add(new ProgressEntry(pair.Key, pair.Value));
            }
        }

        #endregion
    }
}
