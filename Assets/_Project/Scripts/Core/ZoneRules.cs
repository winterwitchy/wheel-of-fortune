using System;

namespace WheelGame.Core
{
    public sealed class ZoneRules
    {
        private readonly int _safeInterval;
        private readonly int _superInterval;

        public ZoneRules(int safeInterval, int superInterval)
        {
            if (safeInterval <= 0)
                throw new ArgumentOutOfRangeException(nameof(safeInterval));

            if (superInterval <= 0)
                throw new ArgumentOutOfRangeException(nameof(superInterval));

            _safeInterval = safeInterval;
            _superInterval = superInterval;
        }

        public ZoneType GetZoneType(int zone)
        {
            if (zone % _superInterval == 0)
                return ZoneType.Super;

            if (zone % _safeInterval == 0)
                return ZoneType.Safe;

            return ZoneType.Normal;
        }
    }
}