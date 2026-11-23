using UnityEngine;

namespace WheelGame.Views
{
    public interface IRewardIconProvider
    {
        Sprite GetIcon(string rewardId);
    }
}
