using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class WheelSession
    {
        private readonly ZoneRules _zoneRules;
        private readonly IWheelGenerator _wheelGenerator;
        private readonly LeaveRule _leaveRule;
        private readonly RevivePricing _revivePricing;
        private readonly IRandomProvider _random;
        private readonly Dictionary<RewardItem, int> _collectedRewards = new Dictionary<RewardItem, int>();

        private SpinResult _pendingResult;
        private int _revivesUsed;

        public WheelSession(ZoneRules zoneRules, IWheelGenerator wheelGenerator, LeaveRule leaveRule, RevivePricing revivePricing, IRandomProvider random)
        {
            _zoneRules = zoneRules;
            _wheelGenerator = wheelGenerator;
            _leaveRule = leaveRule;
            _revivePricing = revivePricing;
            _random = random;
        }

        public event Action StateChanged;
        public event Action RunStarted;
        public event Action<int, ZoneKind, IReadOnlyList<WheelSliceEntry>> ZoneStarted;
        public event Action<SpinResult> SpinStarted;
        public event Action<WheelSliceEntry> RewardCollected;
        public event Action BombHit;
        public event Action<IReadOnlyDictionary<RewardItem, int>> CashedOut;

        public SessionState State { get; private set; }
        public int CurrentZone { get; private set; }
        public ZoneKind CurrentZoneKind { get; private set; }
        public IReadOnlyList<WheelSliceEntry> CurrentWheel { get; private set; }
        public IReadOnlyDictionary<RewardItem, int> CollectedRewards => _collectedRewards;

        public bool CanSpin => State == SessionState.ReadyToSpin;
        public bool CanLeave
        {
            get
            {
                if (State != SessionState.ReadyToSpin)
                {
                    return false;
                }

                if (!HasCollectedRewards)
                {
                    return false;
                }

                return IsLeaveAllowedInCurrentZone();
            }
        }

        public bool HasCollectedRewards
        {
            get
            {
                return _collectedRewards.Count > 0;
            }
        }
        public int GetNextZoneOfKind(string kindId)
        {
            return _zoneRules.GetNextZoneOfKind(CurrentZone, kindId);
        }

        public int ReviveCost
        {
            get
            {
                return _revivePricing.GetCost(_revivesUsed);
            }
        }

        public ZoneKind GetZoneKind(int zone)
        {
            return _zoneRules.GetZoneKind(zone);
        }

        public void StartRun()
        {
            _collectedRewards.Clear();
            _revivesUsed = 0;

            if (RunStarted != null)
            {
                RunStarted.Invoke();
            }

            EnterZone(1);
        }

        public SpinResult Spin()
        {
            if (!CanSpin)
                throw new InvalidOperationException($"Cannot spin while {State}.");

            var sliceIndex = _random.Range(0, CurrentWheel.Count);
            _pendingResult = new SpinResult(sliceIndex, CurrentWheel[sliceIndex]);

            SetState(SessionState.Spinning);
            SpinStarted?.Invoke(_pendingResult);
            return _pendingResult;
        }

        public void CompleteSpin()
        {
            if (State != SessionState.Spinning)
                throw new InvalidOperationException($"Cannot complete a spin while {State}.");

            var slice = _pendingResult.Slice;

            if (slice.IsBomb)
            {
                SetState(SessionState.BombHit);
                BombHit?.Invoke();
                return;
            }

            AddReward(slice);
            SetState(SessionState.CollectingReward);

            if (RewardCollected != null)
            {
                RewardCollected.Invoke(slice);
            }
        }

        public void FinishCollecting()
        {
            if (State != SessionState.CollectingReward)
            {
                throw new InvalidOperationException($"Cannot finish collecting while {State}.");
            }

            EnterZone(CurrentZone + 1);
        }

        public bool CanRevive(IWallet wallet)
        {
            if (State != SessionState.BombHit)
            {
                return false;
            }

            return wallet.GetBalance(_revivePricing.CurrencyId) >= ReviveCost;
        }

        public bool TryRevive(IWallet wallet)
        {
            if (State != SessionState.BombHit)
            {
                throw new InvalidOperationException($"Cannot revive while {State}.");
            }

            if (!wallet.TrySpend(_revivePricing.CurrencyId, ReviveCost))
            {
                return false;
            }

            _revivesUsed++;
            SetState(SessionState.ReadyToSpin);
            return true;
        }

        public void Leave(IRewardVault vault)
        {
            if (!CanLeave)
            {
                throw new InvalidOperationException($"Cannot leave while {State} at zone {CurrentZone}.");
            }

            var cashedOutRewards = new Dictionary<RewardItem, int>(_collectedRewards);

            foreach (var pair in cashedOutRewards)
            {
                vault.Deposit(pair.Key, pair.Value);
            }

            SetState(SessionState.CashedOut);

            if (CashedOut != null)
            {
                CashedOut.Invoke(cashedOutRewards);
            }
        }

        private bool IsLeaveAllowedInCurrentZone()
        {
            if (_leaveRule == LeaveRule.SafeZonesOnly)
            {
                return !CurrentZoneKind.HasBomb;
            }

            return true;
        }

        private void EnterZone(int zone)
        {
            CurrentZone = zone;
            CurrentZoneKind = _zoneRules.GetZoneKind(zone);
            CurrentWheel = _wheelGenerator.Generate(zone, CurrentZoneKind);

            SetState(SessionState.ReadyToSpin);
            ZoneStarted?.Invoke(CurrentZone, CurrentZoneKind, CurrentWheel);
        }

        private void AddReward(WheelSliceEntry slice)
        {
            _collectedRewards.TryGetValue(slice.Reward, out var current);
            _collectedRewards[slice.Reward] = current + slice.Amount;
        }

        private void SetState(SessionState state)
        {
            State = state;
            StateChanged?.Invoke();
        }
    }
}