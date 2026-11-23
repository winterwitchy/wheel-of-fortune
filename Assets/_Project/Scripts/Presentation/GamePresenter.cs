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
        private readonly ZoneCatalog _zones;
        private readonly IRewardIconProvider _icons;
        private readonly GoldView _goldView;
        private readonly ZoneProgressBarView _zoneProgressBarView;
        private readonly WheelView _wheelView;
        private readonly WheelEffectsView _wheelEffectsView;
        private readonly RewardsPanelView _rewardsPanelView;
        private readonly RewardClaimView _rewardClaimView;
        private readonly BombPopupView _bombPopupView;
        private readonly CashOutPopupView _cashOutPopupView;

        private int _spinningSliceIndex;
        private RewardItem _claimedReward;
        private int _claimedTotal;

        public GamePresenter(
            WheelSession session, IWallet wallet, IRewardVault vault, string goldId, ZoneCatalog zones, IRewardIconProvider icons,
            GoldView goldView, ZoneProgressBarView zoneProgressBarView, WheelView wheelView, WheelEffectsView wheelEffectsView,
            RewardsPanelView rewardsPanelView, RewardClaimView rewardClaimView, BombPopupView bombPopupView, CashOutPopupView cashOutPopupView)
        {
            _session = session;
            _wallet = wallet;
            _vault = vault;
            _goldId = goldId;
            _zones = zones;
            _icons = icons;
            _goldView = goldView;
            _zoneProgressBarView = zoneProgressBarView;
            _wheelView = wheelView;
            _wheelEffectsView = wheelEffectsView;
            _rewardsPanelView = rewardsPanelView;
            _rewardClaimView = rewardClaimView;
            _bombPopupView = bombPopupView;
            _cashOutPopupView = cashOutPopupView;
        }

        public void Initialize()
        {
            _session.StateChanged += HandleStateChanged;
            _session.RunStarted += HandleRunStarted;
            _session.RewardCollected += HandleRewardCollected;
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

            _zoneProgressBarView.Initialize(GetZoneVisuals, _session.GetNextZoneOfKind);
            _wheelView.Initialize(_icons);
            _rewardsPanelView.Initialize(_icons);
            _cashOutPopupView.Initialize(_icons);
            _goldView.Show(_wallet.GetBalance(_goldId));
            _session.StartRun();
        }

        public void Dispose()
        {
            _session.StateChanged -= HandleStateChanged;
            _session.RunStarted -= HandleRunStarted;
            _session.RewardCollected -= HandleRewardCollected;
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
            _spinningSliceIndex = result.SliceIndex;
            _wheelView.PlaySpin(result.SliceIndex, _session.CompleteSpin);
        }

        private void HandleRunStarted()
        {
            _rewardsPanelView.Clear();
        }

        private void HandleRewardCollected(WheelSliceEntry slice)
        {
            var total = _session.CollectedRewards[slice.Reward];
            var target = _rewardsPanelView.PrepareClaimTarget(slice.Reward, total - slice.Amount);

            _claimedReward = slice.Reward;
            _claimedTotal = total;
            _rewardClaimView.Play(_icons.GetIcon(slice.Reward.Id), slice.Amount, _wheelView.GetSliceIconPosition(_spinningSliceIndex), target, HandleClaimArrived);
        }

        private void HandleClaimArrived()
        {
            _rewardsPanelView.ShowClaimedAmount(_claimedReward, _claimedTotal);
            _claimedReward = null;
            _session.FinishCollecting();
        }

        private void HandleZoneStarted(int zone, ZoneKind zoneKind, IReadOnlyList<WheelSliceEntry> wheel)
        {
            var visuals = _zones.GetVisuals(zoneKind.Id);
            _zoneProgressBarView.Show(zone);
            _wheelView.Show(visuals, wheel);
            _wheelEffectsView.SetZoneVisuals(visuals);
        }

        private ZoneVisuals GetZoneVisuals(int zone)
        {
            return _zones.GetVisuals(_session.GetZoneKind(zone).Id);
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

        private void HandleCashedOut(IReadOnlyDictionary<RewardItem, int> rewards)
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