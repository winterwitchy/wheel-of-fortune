using System.Collections.Generic;
using WheelGame.Data;

namespace WheelGame.Core
{
    public sealed class ProceduralWheelGenerator : IWheelGenerator
    {
        #region Nested Types

        private sealed class WheelDraft
        {
            private readonly Dictionary<RewardItemData, int> _occurrences = new Dictionary<RewardItemData, int>();

            public WheelDraft(int capacity)
            {
                Slices = new List<WheelSliceEntry>(capacity);
            }

            public List<WheelSliceEntry> Slices { get; }

            public bool HasUsed(RewardItemData reward)
            {
                return _occurrences.ContainsKey(reward);
            }

            public void AddReward(RewardItemData reward, int baseAmount)
            {
                _occurrences.TryGetValue(reward, out var occurrence);
                _occurrences[reward] = occurrence + 1;
                Slices.Add(WheelSliceEntry.CreateReward(reward, baseAmount * (occurrence + 1)));
            }

            public void AddBomb()
            {
                Slices.Add(WheelSliceEntry.CreateBomb());
            }
        }

        #endregion

        #region Fields

        private readonly WheelGenerationSettings _settings;
        private readonly IRandomProvider _random;

        #endregion

        #region Constructor

        public ProceduralWheelGenerator(WheelGenerationSettings settings, IRandomProvider random)
        {
            _settings = settings;
            _random = random;
        }

        #endregion

        #region Public Methods

        public IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneType zoneType)
        {
            var band = _settings.GetBand(zone);
            var draft = new WheelDraft(WheelLayout.SliceCount);

            if (zoneType == ZoneType.Super)
            {
                AddComposition(draft, _settings.SuperSlices, zone, band, _settings.SuperMultiplier, _settings.SuperSkinRewards, true);
            }
            else if (zoneType == ZoneType.Safe)
            {
                AddSkin(draft, _settings.SafeSkinRewards, zone, false);
                AddComposition(draft, band.Slices, zone, band, _settings.SafeMultiplier, band.SkinRewards, false);
            }
            else
            {
                draft.AddBomb();
                AddComposition(draft, band.Slices, zone, band, 1, band.SkinRewards, false);
            }

            Shuffle(draft.Slices);
            return draft.Slices;
        }

        #endregion

        #region Composition

        private void AddComposition(WheelDraft draft, SliceComposition composition, int zone, ZoneBand band, int multiplier, IReadOnlyList<RewardItemData> skinPool, bool isSuperZone)
        {
            for (var i = 0; i < composition.Cash; i++)
            {
                draft.AddReward(_settings.CashReward, _settings.CashFormula.Evaluate(zone) * multiplier);
            }

            for (var i = 0; i < composition.Gold; i++)
            {
                draft.AddReward(_settings.GoldReward, _settings.GoldFormula.Evaluate(zone) * multiplier);
            }

            for (var i = 0; i < composition.Points; i++)
            {
                draft.AddReward(DrawPreferringUnused(draft, band.PointRewards), _settings.PointFormula.Evaluate(zone) * multiplier);
            }

            for (var i = 0; i < composition.Consumables; i++)
            {
                draft.AddReward(DrawPreferringUnused(draft, band.ConsumableRewards), _settings.ConsumableFormula.Evaluate(zone) * multiplier);
            }

            for (var i = 0; i < composition.Chests; i++)
            {
                AddChest(draft, zone, isSuperZone);
            }

            for (var i = 0; i < composition.Skins; i++)
            {
                AddSkin(draft, skinPool, zone, isSuperZone);
            }
        }

        private void AddChest(WheelDraft draft, int zone, bool isSuperZone)
        {
            if (isSuperZone)
            {
                draft.AddReward(_settings.SuperChestReward, 1);
                return;
            }

            var progression = _settings.ChestProgression;
            var windowIndex = _random.Range(0, progression.WindowSize);
            draft.AddReward(progression.GetOption(zone, windowIndex), 1);
        }

        private void AddSkin(WheelDraft draft, IReadOnlyList<RewardItemData> pool, int zone, bool isSuperZone)
        {
            var unused = CollectUnused(draft, pool);

            if (unused.Count > 0)
            {
                draft.AddReward(unused[_random.Range(0, unused.Count)], 1);
                return;
            }

            if (isSuperZone)
            {
                draft.AddReward(_settings.SuperChestReward, 1);
                return;
            }

            draft.AddReward(_settings.ChestProgression.GetUpgrade(zone), 1);
        }

        #endregion

        #region Random Helpers

        private RewardItemData DrawPreferringUnused(WheelDraft draft, IReadOnlyList<RewardItemData> pool)
        {
            var unused = CollectUnused(draft, pool);

            if (unused.Count > 0)
            {
                return unused[_random.Range(0, unused.Count)];
            }

            return pool[_random.Range(0, pool.Count)];
        }

        private static List<RewardItemData> CollectUnused(WheelDraft draft, IReadOnlyList<RewardItemData> pool)
        {
            var result = new List<RewardItemData>(pool.Count);

            foreach (var reward in pool)
            {
                if (!draft.HasUsed(reward))
                {
                    result.Add(reward);
                }
            }

            return result;
        }

        private void Shuffle(List<WheelSliceEntry> slices)
        {
            for (var i = slices.Count - 1; i > 0; i--)
            {
                var j = _random.Range(0, i + 1);
                var temp = slices[i];
                slices[i] = slices[j];
                slices[j] = temp;
            }
        }

        #endregion
    }
}
