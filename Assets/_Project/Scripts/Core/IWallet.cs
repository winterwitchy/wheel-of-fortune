namespace WheelGame.Core
{
    public interface IWallet
    {
        int Gold { get; }
        bool TrySpend(int amount);
        void Add(int amount);
    }
}