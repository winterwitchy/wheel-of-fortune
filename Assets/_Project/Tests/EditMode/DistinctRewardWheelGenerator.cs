using System.Collections.Generic;
using WheelGame.Core;
using WheelGame.Data;

namespace WheelGame.Tests.EditMode
{
    public sealed class DistinctRewardWheelGenerator : IWheelGenerator
    {
        private readonly IReadOnlyList<RewardItemData> _rewards;
        private readonly int _bombIndex;

        public DistinctRewardWheelGenerator(IReadOnlyList<RewardItemData> rewards, int bombIndex)
        {
            _rewards = rewards;
            _bombIndex = bombIndex;
        }

        public IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneType zoneType)
        {
            var slices = new List<WheelSliceEntry>();

            for (var i = 0; i < _rewards.Count; i++)
            {
                if (i == _bombIndex && zoneType == ZoneType.Normal)
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
