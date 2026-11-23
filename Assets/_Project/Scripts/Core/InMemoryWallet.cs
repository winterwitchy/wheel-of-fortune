using System;
using System.Collections.Generic;
using WheelGame.Core.Events;

namespace WheelGame.Core
{
    public sealed class InMemoryWallet : IWallet
    {
        #region Fields

        private readonly Dictionary<string, int> _balances = new Dictionary<string, int>();
        private readonly IEventBus _bus;

        #endregion

        #region Constructors

        public InMemoryWallet(IEventBus bus)
        {
            _bus = bus;
        }

        #endregion

        #region Properties

        public IReadOnlyDictionary<string, int> Balances
        {
            get { return _balances; }
        }

        #endregion

        #region Public Methods

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

        #endregion

        #region Private Methods

        private void SetBalance(string currencyId, int balance)
        {
            _balances[currencyId] = balance;

            _bus.Publish(new BalanceChangedEvent(currencyId, balance));
        }

        #endregion
    }
}
