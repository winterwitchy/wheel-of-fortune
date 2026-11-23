using System;

namespace WheelGame.Core
{
    public sealed class SliceRule
    {
        #region Constructors

        public SliceRule(IRewardSource source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Slice count cannot be negative.");
            }

            Source = source;
            Count = count;
        }

        #endregion

        #region Properties

        public IRewardSource Source { get; }

        public int Count { get; }

        #endregion
    }
}
