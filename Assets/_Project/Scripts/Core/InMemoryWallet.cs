namespace WheelGame.Core
{
    public sealed class InMemoryWallet : IWallet
    {
        public InMemoryWallet(int startingGold)
        {
            Gold = startingGold;
        }

        public int Gold { get; private set; }

        public bool TrySpend(int amount)
        {
            if (amount > Gold)
                return false;

            Gold -= amount;
            return true;
        }

        public void Add(int amount)
        {
            Gold += amount;
        }
    }
}