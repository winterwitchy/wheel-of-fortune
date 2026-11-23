using WheelGame.Core;
using WheelGame.Core.Events;
using WheelGame.Views;

namespace WheelGame.Presentation
{
    public sealed class RewardsPresenter : IPresenter
    {
        private readonly IEventBus _bus;
        private readonly WheelSession _session;
        private readonly IRewardIconProvider _icons;
        private readonly IRewardsPanelView _panel;
        private readonly IRewardClaimView _claim;
        private readonly IWheelView _wheel;

        private RewardItem _claimedReward;
        private int _claimedTotal;

        public RewardsPresenter(IEventBus bus, WheelSession session, IRewardIconProvider icons, IRewardsPanelView panel, IRewardClaimView claim, IWheelView wheel)
        {
            _bus = bus;
            _session = session;
            _icons = icons;
            _panel = panel;
            _claim = claim;
            _wheel = wheel;
        }

        public void Initialize()
        {
            _bus.Subscribe<RunStartedEvent>(HandleRunStarted);
            _bus.Subscribe<SessionStateChangedEvent>(HandleStateChanged);
            _bus.Subscribe<RewardCollectedEvent>(HandleRewardCollected);
            _bus.Subscribe<ClaimAnimationFinishedEvent>(HandleClaimFinished);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<RunStartedEvent>(HandleRunStarted);
            _bus.Unsubscribe<SessionStateChangedEvent>(HandleStateChanged);
            _bus.Unsubscribe<RewardCollectedEvent>(HandleRewardCollected);
            _bus.Unsubscribe<ClaimAnimationFinishedEvent>(HandleClaimFinished);
        }

        private void HandleRunStarted(RunStartedEvent message)
        {
            _panel.Clear();
        }

        private void HandleStateChanged(SessionStateChangedEvent message)
        {
            _panel.SetLeaveInteractable(message.CanLeave);
        }

        private void HandleRewardCollected(RewardCollectedEvent message)
        {
            var reward = message.Slice.Reward;
            var target = _panel.PrepareClaimTarget(reward, message.Total - message.Slice.Amount);

            _claimedReward = reward;
            _claimedTotal = message.Total;
            _claim.Play(_icons.GetIcon(reward.Id), message.Slice.Amount, _wheel.GetSliceIconPosition(message.SliceIndex), target);
        }

        private void HandleClaimFinished(ClaimAnimationFinishedEvent message)
        {
            if (_claimedReward != null)
            {
                _panel.ShowClaimedAmount(_claimedReward, _claimedTotal);
                _claimedReward = null;
            }

            if (_session.State == SessionState.CollectingReward)
            {
                _session.FinishCollecting();
            }
        }
    }
}
