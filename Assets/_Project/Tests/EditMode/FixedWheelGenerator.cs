using System.Collections.Generic;
using WheelGame.Core;
using WheelGame.Data;

namespace WheelGame.Tests.EditMode
{
    public sealed class FixedWheelGenerator : IWheelGenerator
    {
        private readonly RewardItemData _reward;
        private readonly int _amount;
        private readonly int _bombIndex;

        public FixedWheelGenerator(RewardItemData reward, int amount, int bombIndex)
        {
            _reward = reward;
            _amount = amount;
            _bombIndex = bombIndex;
        }

        public IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneType zoneType)
        {
            var slices = new List<WheelSliceEntry>();
            for (var i = 0; i < WheelLayout.SliceCount; i++)
                slices.Add(i == _bombIndex ? WheelSliceEntry.CreateBomb() : WheelSliceEntry.CreateReward(_reward, _amount));

            return slices;
        }
    }
}