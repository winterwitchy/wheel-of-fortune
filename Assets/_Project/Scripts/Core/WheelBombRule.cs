using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class WheelBombRule
    {
        #region Constants

        public const int UNALTERED = -1;

        #endregion

        #region Fields

        private readonly IRandomProvider _random;

        #endregion

        #region Constructors

        public WheelBombRule(IRandomProvider random)
        {
            _random = random;
        }

        #endregion

        #region Public Methods

        public static int GetRequiredBombCount(ZoneKind zoneKind)
        {
            if (zoneKind.HasBomb)
            {
                return 1;
            }

            return 0;
        }

        public void Enforce(List<WheelSliceEntry> slices, IReadOnlyList<WheelSliceEntry> original, IReadOnlyList<int> alterationOrder, ZoneKind zoneKind)
        {
            var bombIndices = FindBombIndices(slices);

            if (GetRequiredBombCount(zoneKind) == 0)
            {
                foreach (var bombIndex in bombIndices)
                {
                    RevertToReward(slices, bombIndex, UNALTERED, original);
                }

                return;
            }

            if (bombIndices.Count == 0)
            {
                PlaceBombOnLeastRecentlyAltered(slices, alterationOrder);
                return;
            }

            var keptBombIndex = FindMostRecentlyAltered(bombIndices, alterationOrder);

            foreach (var bombIndex in bombIndices)
            {
                if (bombIndex != keptBombIndex)
                {
                    RevertToReward(slices, bombIndex, keptBombIndex, original);
                }
            }
        }

        #endregion

        #region Private Methods

        private static List<int> FindBombIndices(IReadOnlyList<WheelSliceEntry> slices)
        {
            var result = new List<int>();

            for (var i = 0; i < slices.Count; i++)
            {
                if (slices[i].IsBomb)
                {
                    result.Add(i);
                }
            }

            return result;
        }

        private static int FindMostRecentlyAltered(IReadOnlyList<int> bombIndices, IReadOnlyList<int> alterationOrder)
        {
            var result = bombIndices[0];

            foreach (var bombIndex in bombIndices)
            {
                if (alterationOrder[bombIndex] > alterationOrder[result])
                {
                    result = bombIndex;
                }
            }

            return result;
        }

        private void PlaceBombOnLeastRecentlyAltered(List<WheelSliceEntry> slices, IReadOnlyList<int> alterationOrder)
        {
            var oldestOrder = int.MaxValue;
            var candidates = new List<int>();

            for (var i = 0; i < slices.Count; i++)
            {
                if (alterationOrder[i] < oldestOrder)
                {
                    oldestOrder = alterationOrder[i];
                    candidates.Clear();
                }

                if (alterationOrder[i] == oldestOrder)
                {
                    candidates.Add(i);
                }
            }

            if (candidates.Count == 0)
            {
                return;
            }

            var target = candidates[0];

            if (candidates.Count > 1)
            {
                target = candidates[_random.Range(0, candidates.Count)];
            }

            slices[target] = WheelSliceEntry.CreateBomb();
        }

        private static void RevertToReward(List<WheelSliceEntry> slices, int index, int keptBombIndex, IReadOnlyList<WheelSliceEntry> original)
        {
            var replacement = FindReplacement(index, keptBombIndex, original);

            if (replacement != null)
            {
                slices[index] = replacement;
            }
        }

        private static WheelSliceEntry FindReplacement(int index, int keptBombIndex, IReadOnlyList<WheelSliceEntry> original)
        {
            if (IsReward(original, index))
            {
                return original[index];
            }

            if (IsReward(original, keptBombIndex))
            {
                return original[keptBombIndex];
            }

            foreach (var entry in original)
            {
                if (!entry.IsBomb)
                {
                    return entry;
                }
            }

            return null;
        }

        private static bool IsReward(IReadOnlyList<WheelSliceEntry> slices, int index)
        {
            if (index < 0 || index >= slices.Count)
            {
                return false;
            }

            return !slices[index].IsBomb;
        }

        #endregion
    }
}
