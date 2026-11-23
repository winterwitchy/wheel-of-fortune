using WheelGame.Core;
using WheelGame.Core.Events;
using WheelGame.Views;

namespace WheelGame.Presentation
{
    public sealed class CurrencyPresenter : IPresenter
    {
        private readonly IEventBus _bus;
        private readonly IWallet _wallet;
        private readonly string _currencyId;
        private readonly ICurrencyView _view;

        public CurrencyPresenter(IEventBus bus, IWallet wallet, string currencyId, ICurrencyView view)
        {
            _bus = bus;
            _wallet = wallet;
            _currencyId = currencyId;
            _view = view;
        }

        public void Initialize()
        {
            _bus.Subscribe<BalanceChangedEvent>(HandleBalanceChanged);
            _view.Show(_wallet.GetBalance(_currencyId));
        }

        public void Dispose()
        {
            _bus.Unsubscribe<BalanceChangedEvent>(HandleBalanceChanged);
        }

        private void HandleBalanceChanged(BalanceChangedEvent message)
        {
            if (message.CurrencyId == _currencyId)
            {
                _view.Show(message.Balance);
            }
        }
    }
}
