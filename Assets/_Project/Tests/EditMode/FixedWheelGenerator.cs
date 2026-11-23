using System.Collections.Generic;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class FixedWheelGenerator : IWheelGenerator
    {
        private readonly RewardItem _reward;
        private readonly int _amount;
        private readonly int _bombIndex;

        public FixedWheelGenerator(RewardItem reward, int amount, int bombIndex)
        {
            _reward = reward;
            _amount = amount;
            _bombIndex = bombIndex;
        }

        public IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneKind zoneKind)
        {
            var slices = new List<WheelSliceEntry>();
            for (var i = 0; i < WheelLayout.SLICE_COUNT; i++)
                slices.Add(i == _bombIndex ? WheelSliceEntry.CreateBomb() : WheelSliceEntry.CreateReward(_reward, _amount));

            return slices;
        }
    }
}