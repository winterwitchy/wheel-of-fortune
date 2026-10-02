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
            Consumable
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
                slices.Add(CreateRewardSlot((RewardSlot)slot, zone, multiplier));

            slices.Add(CreateBonusSlot(zone, zoneType, multiplier));
            slices.Add(CreateBombSlot(zoneType));

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

        private WheelSliceEntry CreateRewardSlot(RewardSlot slot, int zone, int multiplier)
        {
            return slot switch
            {
                RewardSlot.Cash => WheelSliceEntry.CreateReward(_settings.CashReward, _settings.CashFormula.Evaluate(zone) * multiplier),
                RewardSlot.Gold => WheelSliceEntry.CreateReward(_settings.GoldReward, _settings.GoldFormula.Evaluate(zone) * multiplier),
                RewardSlot.Points => WheelSliceEntry.CreateReward(PickRandom(_settings.PointRewards), _settings.PointFormula.Evaluate(zone) * multiplier),
                _ => WheelSliceEntry.CreateReward(PickRandom(_settings.ConsumableRewards), _settings.ConsumableFormula.Evaluate(zone) * multiplier)
            };
        }

        private WheelSliceEntry CreateBonusSlot(int zone, ZoneType zoneType, int multiplier)
        {
            if (zoneType == ZoneType.Super)
                return CreateSkin(_settings.SuperSkinRewards);

            var slot = (RewardSlot)_random.Range(0, RewardSlotCount);
            return CreateRewardSlot(slot, zone, multiplier);
        }

        private WheelSliceEntry CreateBombSlot(ZoneType zoneType)
        {
            return zoneType switch
            {
                ZoneType.Super => CreateSkin(_settings.SuperSkinRewards),
                ZoneType.Safe => CreateSkin(_settings.SafeSkinRewards),
                _ => WheelSliceEntry.CreateBomb()
            };
        }

        private WheelSliceEntry CreateSkin(IReadOnlyList<RewardItemData> pool)
        {
            return WheelSliceEntry.CreateReward(PickRandom(pool), 1);
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