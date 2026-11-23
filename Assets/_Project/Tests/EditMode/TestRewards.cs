using System.Collections.Generic;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public static class TestRewards
    {
        public static RewardItem Create(string id, bool isCurrency)
        {
            return new RewardItem(id, isCurrency);
        }

        public static List<RewardItem> CreateMany(string prefix, int count)
        {
            var result = new List<RewardItem>();

            for (var i = 0; i < count; i++)
            {
                result.Add(new RewardItem(prefix + "_" + i, false));
            }

            return result;
        }
    }
}
