using System;
using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class ZoneRules
    {
        #region Fields

        private readonly List<ZoneKind> _kinds;

        #endregion

        #region Constructors

        public ZoneRules(IReadOnlyList<ZoneKind> kinds)
        {
            if (kinds == null || kinds.Count == 0)
            {
                throw new ArgumentException("At least one zone kind is required.", nameof(kinds));
            }

            _kinds = new List<ZoneKind>(kinds);
            _kinds.Sort(CompareByPriorityDescending);

            if (!HasDefaultKind())
            {
                throw new ArgumentException("One zone kind must have an interval of 1 so every zone has a kind.", nameof(kinds));
            }
        }

        #endregion

        #region Properties

        public IReadOnlyList<ZoneKind> Kinds
        {
            get { return _kinds; }
        }

        #endregion

        #region Public Methods

        public ZoneKind GetZoneKind(int zone)
        {
            foreach (var kind in _kinds)
            {
                if (kind.Matches(zone))
                {
                    return kind;
                }
            }

            return _kinds[_kinds.Count - 1];
        }

        public int GetNextZoneOfKind(int zone, string kindId)
        {
            var next = zone + 1;
            var limit = zone + GetLargestInterval() * _kinds.Count + 1;

            while (next <= limit)
            {
                if (GetZoneKind(next).Id == kindId)
                {
                    return next;
                }

                next++;
            }

            return -1;
        }

        #endregion

        #region Private Methods

        private bool HasDefaultKind()
        {
            foreach (var kind in _kinds)
            {
                if (kind.Interval == 1)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetLargestInterval()
        {
            var largest = 1;

            foreach (var kind in _kinds)
            {
                largest = Math.Max(largest, kind.Interval);
            }

            return largest;
        }

        private static int CompareByPriorityDescending(ZoneKind left, ZoneKind right)
        {
            return right.Priority.CompareTo(left.Priority);
        }

        #endregion
    }
}
