using System;
using System.Collections.Generic;
using WheelGame.Core;
using WheelGame.Data;
using WheelGame.Views;

namespace WheelGame.Presentation
{
    public sealed class GamePresenter : IDisposable
    {
        private readonly WheelSession _session;
        private readonly WheelView _wheelView;

        public GamePresenter(WheelSession session, WheelView wheelView)
        {
            _session = session;
            _wheelView = wheelView;
        }

        public void Initialize()
        {
            _session.StateChanged += HandleStateChanged;
            _session.ZoneStarted += HandleZoneStarted;
            _session.SpinStarted += HandleSpinStarted;
            _session.BombHit += HandleBombHit;
            _wheelView.SpinClicked += HandleSpinClicked;

            _session.StartRun();
        }

        public void Dispose()
        {
            _session.StateChanged -= HandleStateChanged;
            _session.ZoneStarted -= HandleZoneStarted;
            _session.SpinStarted -= HandleSpinStarted;
            _session.BombHit -= HandleBombHit;
            _wheelView.SpinClicked -= HandleSpinClicked;
        }

        private void HandleSpinClicked()
        {
            if (_session.CanSpin)
                _session.Spin();
        }

        private void HandleSpinStarted(SpinResult result)
        {
            _wheelView.PlaySpin(result.SliceIndex, _session.CompleteSpin);
        }

        private void HandleZoneStarted(int zone, ZoneType zoneType, IReadOnlyList<WheelSliceEntry> wheel)
        {
            _wheelView.Show(zoneType, wheel);
        }

        private void HandleStateChanged()
        {
            _wheelView.SetSpinInteractable(_session.CanSpin);
        }

        private void HandleBombHit()
        {
            _session.StartRun();
        }
    }
}