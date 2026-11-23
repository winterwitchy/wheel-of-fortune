using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class ZoneKind
    {
        #region Constructors

        public ZoneKind(string id, int interval, int priority, bool hasBomb, int multiplier, IRewardSource bombSlotSource, IReadOnlyList<SliceRule> composition)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Zone kind id is missing.", nameof(id));
            }

            if (interval < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(interval), interval, "Zone interval must be at least 1.");
            }

            if (multiplier < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, "Zone multiplier must be at least 1.");
            }

            Id = id;
            Interval = interval;
            Priority = priority;
            HasBomb = hasBomb;
            Multiplier = multiplier;
            BombSlotSource = bombSlotSource;
            Composition = composition;
        }

        #endregion

        #region Properties

        public string Id { get; }

        public int Interval { get; }

        public int Priority { get; }

        public bool HasBomb { get; }

        public int Multiplier { get; }

        public IRewardSource BombSlotSource { get; }

        public IReadOnlyList<SliceRule> Composition { get; }

        public bool HasOwnComposition
        {
            get { return Composition != null; }
        }

        #endregion

        #region Public Methods

        public bool Matches(int zone)
        {
            return zone % Interval == 0;
        }

        #endregion
    }
}
