using System.Collections.Generic;

namespace WheelGame.Core
{
    public sealed class WheelDraft
    {
        private readonly Dictionary<RewardItem, int> _occurrences = new Dictionary<RewardItem, int>();
        private readonly List<WheelSliceEntry> _slices = new List<WheelSliceEntry>(WheelLayout.SLICE_COUNT);

        public List<WheelSliceEntry> Slices
        {
            get { return _slices; }
        }

        public bool HasUsed(RewardItem reward)
        {
            return _occurrences.ContainsKey(reward);
        }

        public void AddReward(RewardItem reward, int baseAmount)
        {
            _occurrences.TryGetValue(reward, out var occurrence);
            _occurrences[reward] = occurrence + 1;
            _slices.Add(WheelSliceEntry.CreateReward(reward, baseAmount * (occurrence + 1)));
        }

        public void AddBomb()
        {
            _slices.Add(WheelSliceEntry.CreateBomb());
        }

        public List<RewardItem> CollectUnused(IReadOnlyList<RewardItem> pool)
        {
            var result = new List<RewardItem>(pool.Count);

            foreach (var reward in pool)
            {
                if (!HasUsed(reward))
                {
                    result.Add(reward);
                }
            }

            return result;
        }
    }
}
