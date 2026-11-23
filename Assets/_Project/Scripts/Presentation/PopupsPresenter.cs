using WheelGame.Core;
using WheelGame.Core.Events;
using WheelGame.Views;

namespace WheelGame.Presentation
{
    public sealed class PopupsPresenter : IPresenter
    {
        private readonly IEventBus _bus;
        private readonly WheelSession _session;
        private readonly IWallet _wallet;
        private readonly IRewardVault _vault;
        private readonly IBombPopupView _bombPopup;
        private readonly ICashOutPopupView _cashOutPopup;

        public PopupsPresenter(IEventBus bus, WheelSession session, IWallet wallet, IRewardVault vault, IBombPopupView bombPopup, ICashOutPopupView cashOutPopup)
        {
            _bus = bus;
            _session = session;
            _wallet = wallet;
            _vault = vault;
            _bombPopup = bombPopup;
            _cashOutPopup = cashOutPopup;
        }

        public void Initialize()
        {
            _bus.Subscribe<BombEffectFinishedEvent>(HandleBombEffectFinished);
            _bus.Subscribe<ReviveRequestedEvent>(HandleReviveRequested);
            _bus.Subscribe<GiveUpRequestedEvent>(HandleGiveUpRequested);
            _bus.Subscribe<LeaveRequestedEvent>(HandleLeaveRequested);
            _bus.Subscribe<CashedOutEvent>(HandleCashedOut);
            _bus.Subscribe<ContinueRequestedEvent>(HandleContinueRequested);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<BombEffectFinishedEvent>(HandleBombEffectFinished);
            _bus.Unsubscribe<ReviveRequestedEvent>(HandleReviveRequested);
            _bus.Unsubscribe<GiveUpRequestedEvent>(HandleGiveUpRequested);
            _bus.Unsubscribe<LeaveRequestedEvent>(HandleLeaveRequested);
            _bus.Unsubscribe<CashedOutEvent>(HandleCashedOut);
            _bus.Unsubscribe<ContinueRequestedEvent>(HandleContinueRequested);
        }

        private void HandleBombEffectFinished(BombEffectFinishedEvent message)
        {
            _bombPopup.Show(_session.ReviveCost, _session.CanRevive(_wallet));
        }

        private void HandleReviveRequested(ReviveRequestedEvent message)
        {
            if (_session.State != SessionState.BombHit)
            {
                return;
            }

            if (_session.TryRevive(_wallet))
            {
                _bombPopup.Close();
            }
        }

        private void HandleGiveUpRequested(GiveUpRequestedEvent message)
        {
            _bombPopup.Close();
            _session.StartRun();
        }

        private void HandleLeaveRequested(LeaveRequestedEvent message)
        {
            if (_session.CanLeave)
            {
                _session.Leave(_vault);
            }
        }

        private void HandleCashedOut(CashedOutEvent message)
        {
            _cashOutPopup.Show(message.Rewards);
        }

        private void HandleContinueRequested(ContinueRequestedEvent message)
        {
            _cashOutPopup.Close();
            _session.StartRun();
        }
    }
}
