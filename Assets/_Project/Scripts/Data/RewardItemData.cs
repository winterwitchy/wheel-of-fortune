using UnityEngine;

namespace WheelGame.Data
{
    [CreateAssetMenu(fileName="reward_", menuName = "WheelGame/Reward Item")]
    public sealed class RewardItemData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private Sprite _icon;
        [SerializeField] private bool _isCurrency;

        public string Id => _id;
        public Sprite Icon => _icon;

        public bool IsCurrency
        {
            get { return _isCurrency; }
        }
    }
}