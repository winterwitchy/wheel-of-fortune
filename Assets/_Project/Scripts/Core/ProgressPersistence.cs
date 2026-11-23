using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class ProgressPersistence : IDisposable
    {
        private readonly IWallet _wallet;
        private readonly IInventory _inventory;
        private readonly IProgressStore _store;

        private bool _isTracking;

        public ProgressPersistence(IWallet wallet, IInventory inventory, IProgressStore store)
        {
            _wallet = wallet;
            _inventory = inventory;
            _store = store;
        }

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

            _wallet.BalanceChanged -= HandleProgressChanged;
            _inventory.ItemChanged -= HandleProgressChanged;
            _isTracking = false;
        }

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

            _wallet.BalanceChanged += HandleProgressChanged;
            _inventory.ItemChanged += HandleProgressChanged;
            _isTracking = true;
        }

        private void HandleProgressChanged(string id, int amount)
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
    }
}
