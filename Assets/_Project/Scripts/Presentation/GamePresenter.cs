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
        private readonly IRewardVault _vault;
        private readonly string _goldId;
        private readonly GoldView _goldView;
        private readonly ZoneProgressBarView _zoneProgressBarView;
        private readonly WheelView _wheelView;
        private readonly WheelEffectsView _wheelEffectsView;
        private readonly RewardsPanelView _rewardsPanelView;
        private readonly BombPopupView _bombPopupView;
        private readonly CashOutPopupView _cashOutPopupView;

        public GamePresenter(
            WheelSession session, IWallet wallet, IRewardVault vault, string goldId,
            GoldView goldView, ZoneProgressBarView zoneProgressBarView, WheelView wheelView, WheelEffectsView wheelEffectsView,
            RewardsPanelView rewardsPanelView, BombPopupView bombPopupView, CashOutPopupView cashOutPopupView)
        {
            _session = session;
            _wallet = wallet;
            _vault = vault;
            _goldId = goldId;
            _goldView = goldView;
            _zoneProgressBarView = zoneProgressBarView;
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
            _wallet.BalanceChanged += HandleBalanceChanged;

            _zoneProgressBarView.Initialize(_session.GetZoneType);
            _goldView.Show(_wallet.GetBalance(_goldId));
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
            _wallet.BalanceChanged -= HandleBalanceChanged;
        }

        private void HandleSpinClicked()
        {
            if (_session.CanSpin)
                _session.Spin();
        }

        private void HandleLeaveClicked()
        {
            if (_session.CanLeave)
            {
                _session.Leave(_vault);
            }
        }

        private void HandleSpinStarted(SpinResult result)
        {
            _wheelView.PlaySpin(result.SliceIndex, _session.CompleteSpin);
        }

        private void HandleZoneStarted(int zone, ZoneType zoneType, IReadOnlyList<WheelSliceEntry> wheel)
        {
            _zoneProgressBarView.Show(zone, _session.GetNextZoneOfType(ZoneType.Safe), _session.GetNextZoneOfType(ZoneType.Super));
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
            _bombPopupView.Show(_session.ReviveCost, _session.CanRevive(_wallet));
        }

        private void HandleGiveUpClicked()
        {
            _bombPopupView.Close();
            _session.StartRun();
        }

        private void HandleReviveClicked()
        {
            if (!_session.TryRevive(_wallet))
            {
                return;
            }

            _bombPopupView.Close();
        }

        private void HandleCashedOut(IReadOnlyDictionary<RewardItemData, int> rewards)
        {
            _cashOutPopupView.Show(rewards);
        }

        private void HandleBalanceChanged(string currencyId, int balance)
        {
            if (currencyId == _goldId)
            {
                _goldView.Show(balance);
            }
        }

        private void HandleContinueClicked()
        {
            _cashOutPopupView.Close();
            _session.StartRun();
        }
    }
}