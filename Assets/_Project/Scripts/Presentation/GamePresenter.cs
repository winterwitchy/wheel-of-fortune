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
        private readonly IWallet _wallet;
        private readonly int _reviveCost;
        private readonly RewardItemData _goldReward;
        private readonly GoldView _goldView;
        private readonly ZoneInfoView _zoneInfoView;
        private readonly WheelView _wheelView;
        private readonly WheelEffectsView _wheelEffectsView;
        private readonly RewardsPanelView _rewardsPanelView;
        private readonly BombPopupView _bombPopupView;
        private readonly CashOutPopupView _cashOutPopupView;

        public GamePresenter(
            WheelSession session, IWallet wallet, int reviveCost, RewardItemData goldReward,
            GoldView goldView, ZoneInfoView zoneInfoView, WheelView wheelView, WheelEffectsView wheelEffectsView,
            RewardsPanelView rewardsPanelView, BombPopupView bombPopupView, CashOutPopupView cashOutPopupView)
        {
            _session = session;
            _wallet = wallet;
            _reviveCost = reviveCost;
            _goldReward = goldReward;
            _goldView = goldView;
            _zoneInfoView = zoneInfoView;
            _wheelView = wheelView;
            _wheelEffectsView = wheelEffectsView;
            _rewardsPanelView = rewardsPanelView;
            _bombPopupView = bombPopupView;
            _cashOutPopupView = cashOutPopupView;
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
            _bombPopupView.GiveUpClicked += HandleGiveUpClicked;
            _bombPopupView.ReviveClicked += HandleReviveClicked;
            _cashOutPopupView.ContinueClicked += HandleContinueClicked;

            _goldView.Show(_wallet.Gold);
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
            _bombPopupView.GiveUpClicked -= HandleGiveUpClicked;
            _bombPopupView.ReviveClicked -= HandleReviveClicked;
            _cashOutPopupView.ContinueClicked -= HandleContinueClicked;
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
            var nextRiskFreeZone = _session.NextRiskFreeZone;
            _zoneInfoView.Show(zone, zoneType, nextRiskFreeZone, _session.GetZoneType(nextRiskFreeZone));
            _wheelView.Show(zoneType, wheel);
            _wheelEffectsView.SetZoneType(zoneType);
            _rewardsPanelView.Show(_session.CollectedRewards);
        }

        private void HandleStateChanged()
        {
            _wheelView.SetSpinInteractable(_session.CanSpin);
            _rewardsPanelView.SetLeaveInteractable(_session.CanLeave);
        }

        private void HandleBombHit()
        {
            _wheelEffectsView.PlayBomb(ShowBombPopup);
        }

        private void ShowBombPopup()
        {
            _bombPopupView.Show(_reviveCost, _wallet.Gold >= _reviveCost);
        }

        private void HandleGiveUpClicked()
        {
            _bombPopupView.Close();
            _session.StartRun();
        }

        private void HandleReviveClicked()
        {
            if (!_wallet.TrySpend(_reviveCost))
                return;

            _goldView.Show(_wallet.Gold);
            _bombPopupView.Close();
            _session.Revive();
        }

        private void HandleCashedOut(IReadOnlyDictionary<RewardItemData, int> rewards)
        {
            if (rewards.TryGetValue(_goldReward, out var collectedGold))
            {
                _wallet.Add(collectedGold);
                _goldView.Show(_wallet.Gold);
            }

            _cashOutPopupView.Show(rewards);
        }

        private void HandleContinueClicked()
        {
            _cashOutPopupView.Close();
            _session.StartRun();
        }
    }
}