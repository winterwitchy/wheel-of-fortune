using System.Collections.Generic;
using WheelGame.Data;

namespace WheelGame.Core
{
    public sealed class ProceduralWheelGenerator : IWheelGenerator
    {
        private enum RewardSlot
        {
            Cash,
            Gold,
            Points,
            Item
        }

        private const int RewardSlotCount = 4;

        private readonly WheelGenerationSettings _settings;
        private readonly IRandomProvider _random;

        public ProceduralWheelGenerator(WheelGenerationSettings settings, IRandomProvider random)
        {
            _settings = settings;
            _random = random;
        }

        public IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneType zoneType)
        {
            var multiplier = GetMultiplier(zoneType);
            var slices = new List<WheelSliceEntry>(WheelLayout.SliceCount);

            for (var slot = 0; slot < RewardSlotCount; slot++)
                slices.Add(CreateRewardSlot((RewardSlot)slot, zone, zoneType, multiplier));

            var bonusSlot = (RewardSlot)_random.Range(0, RewardSlotCount);
            slices.Add(CreateRewardSlot(bonusSlot, zone, zoneType, multiplier));

            slices.Add(zoneType == ZoneType.Normal ? WheelSliceEntry.CreateBomb() : CreateChest(zone, zoneType));

            while (slices.Count < WheelLayout.SliceCount)
                slices.Add(CreateChest(zone, zoneType));

            Shuffle(slices);
            return slices;
        }

        private int GetMultiplier(ZoneType zoneType)
        {
            return zoneType switch
            {
                ZoneType.Super => _settings.SuperMultiplier,
                ZoneType.Safe => _settings.SafeMultiplier,
                _ => 1
            };
        }

        private WheelSliceEntry CreateRewardSlot(RewardSlot slot, int zone, ZoneType zoneType, int multiplier)
        {
            return slot switch
            {
                RewardSlot.Cash => WheelSliceEntry.CreateReward(_settings.CashReward, _settings.CashFormula.Evaluate(zone) * multiplier),
                RewardSlot.Gold => WheelSliceEntry.CreateReward(_settings.GoldReward, _settings.GoldFormula.Evaluate(zone) * multiplier),
                RewardSlot.Points => WheelSliceEntry.CreateReward(PickRandom(_settings.PointRewards), _settings.PointFormula.Evaluate(zone) * multiplier),
                _ => WheelSliceEntry.CreateReward(PickRandom(GetItemPool(zoneType)), 1)
            };
        }

        private IReadOnlyList<RewardItemData> GetItemPool(ZoneType zoneType)
        {
            return zoneType == ZoneType.Super ? _settings.SuperItemRewards : _settings.ItemRewards;
        }

        private WheelSliceEntry CreateChest(int zone, ZoneType zoneType)
        {
            if (zoneType == ZoneType.Super)
                return WheelSliceEntry.CreateReward(_settings.SuperChestReward, 1);

            var progression = _settings.ChestProgression;
            var windowIndex = _random.Range(0, progression.WindowSize);
            return WheelSliceEntry.CreateReward(progression.GetOption(zone, windowIndex), 1);
        }

        private T PickRandom<T>(IReadOnlyList<T> items)
        {
            return items[_random.Range(0, items.Count)];
        }

        private void Shuffle(List<WheelSliceEntry> slices)
        {
            for (var i = slices.Count - 1; i > 0; i--)
            {
                var j = _random.Range(0, i + 1);
                (slices[i], slices[j]) = (slices[j], slices[i]);
            }
        }
    }
}