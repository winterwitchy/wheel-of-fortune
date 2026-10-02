using System;
using System.Collections.Generic;
using WheelGame.Data;

namespace WheelGame.Core
{
    public sealed class WheelSession
    {
        private readonly ZoneRules _zoneRules;
        private readonly IWheelGenerator _wheelGenerator;
        private readonly LeaveRule _leaveRule;
        private readonly IRandomProvider _random;
        private readonly Dictionary<RewardItemData, int> _collectedRewards = new Dictionary<RewardItemData, int>();

        private SpinResult _pendingResult;

        public WheelSession(ZoneRules zoneRules, IWheelGenerator wheelGenerator, LeaveRule leaveRule, IRandomProvider random)
        {
            _zoneRules = zoneRules;
            _wheelGenerator = wheelGenerator;
            _leaveRule = leaveRule;
            _random = random;
        }

        public event Action StateChanged;
        public event Action<int, ZoneType, IReadOnlyList<WheelSliceEntry>> ZoneStarted;
        public event Action<SpinResult> SpinStarted;
        public event Action<WheelSliceEntry> RewardCollected;
        public event Action BombHit;
        public event Action<IReadOnlyDictionary<RewardItemData, int>> CashedOut;

        public SessionState State { get; private set; }
        public int CurrentZone { get; private set; }
        public ZoneType CurrentZoneType { get; private set; }
        public IReadOnlyList<WheelSliceEntry> CurrentWheel { get; private set; }
        public IReadOnlyDictionary<RewardItemData, int> CollectedRewards => _collectedRewards;

        public bool CanSpin => State == SessionState.ReadyToSpin;
        public bool CanLeave => State == SessionState.ReadyToSpin && IsLeaveAllowedInCurrentZone();

        public void StartRun()
        {
            _collectedRewards.Clear();
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
            RewardCollected?.Invoke(slice);
            EnterZone(CurrentZone + 1);
        }

        public void Revive()
        {
            if (State != SessionState.BombHit)
                throw new InvalidOperationException($"Cannot revive while {State}.");

            SetState(SessionState.ReadyToSpin);
        }

        public void Leave()
        {
            if (!CanLeave)
                throw new InvalidOperationException($"Cannot leave while {State} at zone {CurrentZone}.");

            SetState(SessionState.CashedOut);
            CashedOut?.Invoke(new Dictionary<RewardItemData, int>(_collectedRewards));
        }

        private bool IsLeaveAllowedInCurrentZone()
        {
            return _leaveRule switch
            {
                LeaveRule.SafeZonesOnly => CurrentZoneType != ZoneType.Normal,
                _ => true
            };
        }

        private void EnterZone(int zone)
        {
            CurrentZone = zone;
            CurrentZoneType = _zoneRules.GetZoneType(zone);
            CurrentWheel = _wheelGenerator.Generate(zone, CurrentZoneType);

            SetState(SessionState.ReadyToSpin);
            ZoneStarted?.Invoke(CurrentZone, CurrentZoneType, CurrentWheel);
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