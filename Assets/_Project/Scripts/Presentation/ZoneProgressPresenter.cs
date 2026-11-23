using WheelGame.Core;
using WheelGame.Core.Events;
using WheelGame.Data;
using WheelGame.Views;

namespace WheelGame.Presentation
{
    public sealed class ZoneProgressPresenter : IPresenter
    {
        #region Fields

        private readonly IEventBus _bus;
        private readonly WheelSession _session;
        private readonly ZoneCatalog _zones;
        private readonly IZoneProgressView _view;

        #endregion

        #region Constructors

        public ZoneProgressPresenter(IEventBus bus, WheelSession session, ZoneCatalog zones, IZoneProgressView view)
        {
            _bus = bus;
            _session = session;
            _zones = zones;
            _view = view;
        }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            _view.Initialize(GetVisuals, _session.GetNextZoneOfKind);
            _bus.Subscribe<ZoneStartedEvent>(HandleZoneStarted);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<ZoneStartedEvent>(HandleZoneStarted);
        }

        #endregion

        #region Private Methods

        private void HandleZoneStarted(ZoneStartedEvent message)
        {
            _view.Show(message.Zone);
        }

        private ZoneVisuals GetVisuals(int zone)
        {
            return _zones.GetVisuals(_session.GetZoneKind(zone).Id);
        }

        #endregion
    }
}
