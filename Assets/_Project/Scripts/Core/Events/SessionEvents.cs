using System.Collections.Generic;

namespace WheelGame.Core.Events
{
    public readonly struct RunStartedEvent
    {
    }

    public readonly struct ZoneStartedEvent
    {
        public ZoneStartedEvent(int zone, ZoneKind zoneKind, IReadOnlyList<WheelSliceEntry> wheel)
        {
            Zone = zone;
            ZoneKind = zoneKind;
            Wheel = wheel;
        }

        public int Zone { get; }

        public ZoneKind ZoneKind { get; }

        public IReadOnlyList<WheelSliceEntry> Wheel { get; }
    }

    public readonly struct SessionStateChangedEvent
    {
        public SessionStateChangedEvent(SessionState state, bool canSpin, bool canLeave)
        {
            State = state;
            CanSpin = canSpin;
            CanLeave = canLeave;
        }

        public SessionState State { get; }

        public bool CanSpin { get; }

        public bool CanLeave { get; }
    }

    public readonly struct SpinStartedEvent
    {
        public SpinStartedEvent(SpinResult result)
        {
            Result = result;
        }

        public SpinResult Result { get; }
    }

    public readonly struct RewardCollectedEvent
    {
        public RewardCollectedEvent(int sliceIndex, WheelSliceEntry slice, int total)
        {
            SliceIndex = sliceIndex;
            Slice = slice;
            Total = total;
        }

        public int SliceIndex { get; }

        public WheelSliceEntry Slice { get; }

        public int Total { get; }
    }

    public readonly struct BombHitEvent
    {
    }

    public readonly struct CashedOutEvent
    {
        public CashedOutEvent(IReadOnlyDictionary<RewardItem, int> rewards)
        {
            Rewards = rewards;
        }

        public IReadOnlyDictionary<RewardItem, int> Rewards { get; }
    }
}
