using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WheelGame.Data
{
    [CreateAssetMenu(fileName = "reward_", menuName = "WheelGame/Reward Item")]
    public sealed class RewardItemData : ScriptableObject
    {
        #region Serialized Fields

        [SerializeField] private string _id;
        [SerializeField] private AssetReferenceAtlasedSprite _icon;
        [SerializeField] private bool _isCurrency;

        #endregion

        #region Properties

        public string Id
        {
            get { return _id; }
        }

        public AssetReferenceAtlasedSprite Icon
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

        public bool HasIcon
        {
            get
            {
                if (_icon == null)
                {
                    return false;
                }

                return _icon.RuntimeKeyIsValid();
            }
        }

        #endregion
    }
}
