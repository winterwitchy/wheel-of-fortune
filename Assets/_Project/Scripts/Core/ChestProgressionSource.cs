using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class ChestProgressionSource : IRewardSource
    {
        #region Fields

        private readonly IReadOnlyList<RewardItem> _sequence;
        private readonly int _windowSize;
        private readonly int _eraLength;
        private readonly int _shift;

        #endregion

        #region Constructors

        public ChestProgressionSource(IReadOnlyList<RewardItem> sequence, int windowSize, int eraLength, int shift)
        {
            if (sequence == null || sequence.Count == 0)
            {
                throw new ArgumentException("A chest progression needs at least one chest.", nameof(sequence));
            }

            if (windowSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(windowSize), windowSize, "Window size must be at least 1.");
            }

            if (eraLength < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(eraLength), eraLength, "Era length must be at least 1.");
            }

            _sequence = sequence;
            _windowSize = windowSize;
            _eraLength = eraLength;
            _shift = Math.Max(0, shift);
        }

        #endregion

        #region Public Methods

        public void AddTo(WheelDraft draft, int zone, int multiplier, IRandomProvider random)
        {
            var windowIndex = 0;

            if (_windowSize > 1)
            {
                windowIndex = random.Range(0, _windowSize);
            }

            draft.AddReward(GetOption(zone, windowIndex), 1);
        }

        public RewardItem GetOption(int zone, int windowIndex)
        {
            var era = (zone - 1) / _eraLength;
            var index = Math.Min(era + _shift + windowIndex, _sequence.Count - 1);
            return _sequence[index];
        }

        #endregion
    }
}
