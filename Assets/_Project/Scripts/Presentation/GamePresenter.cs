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
        private readonly RewardsPanelView _rewardsPanelView;

        public GamePresenter(WheelSession session, WheelView wheelView, RewardsPanelView rewardsPanelView)
        {
            _session = session;
            _wheelView = wheelView;
            _rewardsPanelView = rewardsPanelView;
        }

        public void Initialize()
        {
            _session.StateChanged += HandleStateChanged;
            _session.ZoneStarted += HandleZoneStarted;
            _session.SpinStarted += HandleSpinStarted;
            _session.BombHit += HandleBombHit;
            _session.CashedOut += HandleCashedOut;
            _wheelView.SpinClicked += HandleSpinClicked;
            _rewardsPanelView.LeaveClicked += HandleLeaveClicked;

            _session.StartRun();
        }

        public void Dispose()
        {
            _session.StateChanged -= HandleStateChanged;
            _session.ZoneStarted -= HandleZoneStarted;
            _session.SpinStarted -= HandleSpinStarted;
            _session.BombHit -= HandleBombHit;
            _session.CashedOut -= HandleCashedOut;
            _wheelView.SpinClicked -= HandleSpinClicked;
            _rewardsPanelView.LeaveClicked -= HandleLeaveClicked;
        }

        private void HandleSpinClicked()
        {
            if (_session.CanSpin)
                _session.Spin();
        }

        private void HandleLeaveClicked()
        {
            if (_session.CanLeave)
                _session.Leave();
        }

        private void HandleSpinStarted(SpinResult result)
        {
            _wheelView.PlaySpin(result.SliceIndex, _session.CompleteSpin);
        }

        private void HandleZoneStarted(int zone, ZoneType zoneType, IReadOnlyList<WheelSliceEntry> wheel)
        {
            _wheelView.Show(zoneType, wheel);
            _rewardsPanelView.Show(_session.CollectedRewards);
        }

        private void HandleStateChanged()
        {
            _wheelView.SetSpinInteractable(_session.CanSpin);
            _rewardsPanelView.SetLeaveInteractable(_session.CanLeave);
        }

        private void HandleBombHit()
        {
            _session.StartRun();
        }

        private void HandleCashedOut(IReadOnlyDictionary<RewardItemData, int> rewards)
        {
            _session.StartRun();
        }
    }
}