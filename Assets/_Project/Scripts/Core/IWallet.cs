using System.Collections.Generic;

namespace WheelGame.Core
{
    public interface IWallet
    {
        IReadOnlyDictionary<string, int> Balances { get; }

        int GetBalance(string currencyId);

        void Add(string currencyId, int amount);

        bool TrySpend(string currencyId, int amount);
    }
}
