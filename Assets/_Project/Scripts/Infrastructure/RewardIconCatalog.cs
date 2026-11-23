using System.Collections.Generic;
using UnityEngine;
using WheelGame.Data;
using WheelGame.Views;

namespace WheelGame.Infrastructure
{
    public sealed class RewardIconCatalog : IRewardIconProvider
    {
        private readonly Dictionary<string, Sprite> _icons = new Dictionary<string, Sprite>();

        public RewardIconCatalog(IEnumerable<RewardItemData> rewards)
        {
            foreach (var reward in rewards)
            {
                if (reward && reward.HasId)
                {
                    _icons[reward.Id] = reward.Icon;
                }
            }
        }

        public Sprite GetIcon(string rewardId)
        {
            _icons.TryGetValue(rewardId, out var icon);
            return icon;
        }
    }
}
