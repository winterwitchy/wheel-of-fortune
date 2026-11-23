using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class ZoneBand
    {
        public ZoneBand(int firstZone, IReadOnlyList<SliceRule> composition)
        {
            if (firstZone < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(firstZone), firstZone, "A band must start at zone 1 or later.");
            }

            FirstZone = firstZone;
            Composition = composition;
        }

        public int FirstZone { get; }

        public IReadOnlyList<SliceRule> Composition { get; }
    }
}
