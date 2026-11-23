using System.Collections.Generic;
using WheelGame.Core;

namespace WheelGame.Data
{
    public sealed class CoreModelFactory
    {
        #region Fields

        private readonly Dictionary<RewardItemData, RewardItem> _rewards = new Dictionary<RewardItemData, RewardItem>();
        private readonly Dictionary<RewardPoolDefinition, IRewardSource> _sources = new Dictionary<RewardPoolDefinition, IRewardSource>();

        #endregion

        #region Properties

        public IReadOnlyCollection<RewardItemData> KnownRewards
        {
            get { return _rewards.Keys; }
        }

        #endregion

        #region Public Methods

        public RewardItem GetReward(RewardItemData data)
        {
            if (!_rewards.TryGetValue(data, out var reward))
            {
                reward = new RewardItem(data.Id, data.IsCurrency);
                _rewards.Add(data, reward);
            }

            return reward;
        }

        public IReadOnlyList<RewardItem> GetRewards(IReadOnlyList<RewardItemData> items)
        {
            var result = new List<RewardItem>(items.Count);

            foreach (var item in items)
            {
                if (item && item.HasId)
                {
                    result.Add(GetReward(item));
                }
            }

            return result;
        }

        public IRewardSource GetSource(RewardPoolDefinition pool)
        {
            if (_sources.TryGetValue(pool, out var source))
            {
                return source;
            }

            source = pool.CreateSource(this);
            _sources.Add(pool, source);
            pool.LinkSource(source, this);
            return source;
        }

        public List<SliceRule> CreateSliceRules(IReadOnlyList<SliceRuleSettings> settings)
        {
            var result = new List<SliceRule>(settings.Count);

            foreach (var setting in settings)
            {
                if (setting.Pool && setting.Count > 0)
                {
                    result.Add(new SliceRule(GetSource(setting.Pool), setting.Count));
                }
            }

            return result;
        }

        public List<ZoneKind> CreateZoneKinds(IReadOnlyList<ZoneDefinition> definitions)
        {
            var result = new List<ZoneKind>(definitions.Count);

            foreach (var definition in definitions)
            {
                if (definition)
                {
                    result.Add(CreateZoneKind(definition));
                }
            }

            return result;
        }

        public ZoneKind CreateZoneKind(ZoneDefinition definition)
        {
            IRewardSource bombSlotSource = null;

            if (!definition.HasBomb && definition.BombSlotPool)
            {
                bombSlotSource = GetSource(definition.BombSlotPool);
            }

            List<SliceRule> composition = null;

            if (definition.HasOwnSlices)
            {
                composition = CreateSliceRules(definition.Slices);
            }

            return new ZoneKind(definition.Id, definition.Interval, definition.Priority, definition.HasBomb, definition.Multiplier, bombSlotSource, composition);
        }

        public List<ZoneBand> CreateBands(IReadOnlyList<ZoneBandSettings> settings)
        {
            var result = new List<ZoneBand>(settings.Count);

            foreach (var band in settings)
            {
                result.Add(new ZoneBand(band.FirstZone, CreateSliceRules(band.Slices)));
            }

            return result;
        }

        public List<SliceOverrideRule> CreateOverrides(IReadOnlyList<SliceOverride> overrides)
        {
            var result = new List<SliceOverrideRule>(overrides.Count);

            foreach (var sliceOverride in overrides)
            {
                if (!sliceOverride.IsValid)
                {
                    continue;
                }

                if (sliceOverride.IsBomb)
                {
                    result.Add(SliceOverrideRule.CreateBomb(sliceOverride.Zone, sliceOverride.SliceIndex));
                }
                else
                {
                    result.Add(SliceOverrideRule.CreateReward(sliceOverride.Zone, sliceOverride.SliceIndex, GetReward(sliceOverride.Reward), sliceOverride.Amount));
                }
            }

            return result;
        }

        #endregion
    }
}
