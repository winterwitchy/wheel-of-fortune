using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class InMemoryWallet : IWallet
    {
        private readonly Dictionary<string, int> _balances = new Dictionary<string, int>();

        public event Action<string, int> BalanceChanged;

        public IReadOnlyDictionary<string, int> Balances
        {
            get { return _balances; }
        }

        public int GetBalance(string currencyId)
        {
            _balances.TryGetValue(currencyId, out var balance);
            return balance;
        }

        public void Add(string currencyId, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Use TrySpend to remove currency.");
            }

            SetBalance(currencyId, GetBalance(currencyId) + amount);
        }

        public bool TrySpend(string currencyId, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Cannot spend a negative amount.");
            }

            var balance = GetBalance(currencyId);

            if (amount > balance)
            {
                return false;
            }

            SetBalance(currencyId, balance - amount);
            return true;
        }

        private void SetBalance(string currencyId, int balance)
        {
            _balances[currencyId] = balance;

            if (BalanceChanged != null)
            {
                BalanceChanged.Invoke(currencyId, balance);
            }
        }
    }
}
