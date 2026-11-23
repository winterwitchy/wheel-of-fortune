using WheelGame.Core;
using WheelGame.Core.Events;
using WheelGame.Data;
using WheelGame.Views;

namespace WheelGame.Presentation
{
    public sealed class WheelPresenter : IPresenter
    {
        private readonly IEventBus _bus;
        private readonly WheelSession _session;
        private readonly ZoneCatalog _zones;
        private readonly IWheelView _wheel;
        private readonly IWheelEffectsView _effects;

        public WheelPresenter(IEventBus bus, WheelSession session, ZoneCatalog zones, IWheelView wheel, IWheelEffectsView effects)
        {
            _bus = bus;
            _session = session;
            _zones = zones;
            _wheel = wheel;
            _effects = effects;
        }

        public void Initialize()
        {
            _bus.Subscribe<SessionStateChangedEvent>(HandleStateChanged);
            _bus.Subscribe<ZoneStartedEvent>(HandleZoneStarted);
            _bus.Subscribe<SpinRequestedEvent>(HandleSpinRequested);
            _bus.Subscribe<SpinStartedEvent>(HandleSpinStarted);
            _bus.Subscribe<SpinAnimationFinishedEvent>(HandleSpinAnimationFinished);
            _bus.Subscribe<BombHitEvent>(HandleBombHit);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<SessionStateChangedEvent>(HandleStateChanged);
            _bus.Unsubscribe<ZoneStartedEvent>(HandleZoneStarted);
            _bus.Unsubscribe<SpinRequestedEvent>(HandleSpinRequested);
            _bus.Unsubscribe<SpinStartedEvent>(HandleSpinStarted);
            _bus.Unsubscribe<SpinAnimationFinishedEvent>(HandleSpinAnimationFinished);
            _bus.Unsubscribe<BombHitEvent>(HandleBombHit);
        }

        private void HandleStateChanged(SessionStateChangedEvent message)
        {
            _wheel.SetSpinInteractable(message.CanSpin);
        }

        private void HandleZoneStarted(ZoneStartedEvent message)
        {
            var visuals = _zones.GetVisuals(message.ZoneKind.Id);
            _wheel.Show(visuals, message.Wheel);
            _effects.SetZoneVisuals(visuals);
        }

        private void HandleSpinRequested(SpinRequestedEvent message)
        {
            if (_session.CanSpin)
            {
                _session.Spin();
            }
        }

        private void HandleSpinStarted(SpinStartedEvent message)
        {
            _wheel.PlaySpin(message.Result.SliceIndex);
        }

        private void HandleSpinAnimationFinished(SpinAnimationFinishedEvent message)
        {
            if (_session.State == SessionState.Spinning)
            {
                _session.CompleteSpin();
            }
        }

        private void HandleBombHit(BombHitEvent message)
        {
            _effects.PlayBomb();
        }
    }
}
