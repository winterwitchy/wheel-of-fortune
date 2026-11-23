using System.Collections.Generic;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class DistinctRewardWheelGenerator : IWheelGenerator
    {
        private readonly IReadOnlyList<RewardItem> _rewards;
        private readonly int _bombIndex;

        public DistinctRewardWheelGenerator(IReadOnlyList<RewardItem> rewards, int bombIndex)
        {
            _rewards = rewards;
            _bombIndex = bombIndex;
        }

        public IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneKind zoneKind)
        {
            var slices = new List<WheelSliceEntry>();

            for (var i = 0; i < _rewards.Count; i++)
            {
                if (i == _bombIndex && zoneKind.HasBomb)
                {
                    slices.Add(WheelSliceEntry.CreateBomb());
                }
                else
                {
                    slices.Add(WheelSliceEntry.CreateReward(_rewards[i], i + 1));
                }
            }

            return slices;
        }
    }
}
