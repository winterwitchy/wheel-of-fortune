using UnityEngine;

namespace WheelGame.Data
{
    [CreateAssetMenu(fileName = "reward_", menuName = "WheelGame/Reward Item")]
    public sealed class RewardItemData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private Sprite _icon;
        [SerializeField] private bool _isCurrency;

        public string Id
        {
            get { return _id; }
        }

        public Sprite Icon
        {
            get { return _icon; }
        }

        public bool IsCurrency
        {
            get { return _isCurrency; }
        }

        public bool HasId
        {
            get { return !string.IsNullOrEmpty(_id); }
        }
    }
}
