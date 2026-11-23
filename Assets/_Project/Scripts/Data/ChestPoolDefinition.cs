using System.Collections.Generic;
using UnityEngine;
using WheelGame.Core;

namespace WheelGame.Data
{
    [CreateAssetMenu(fileName = "pool_chests_", menuName = "WheelGame/Reward Pools/Chest Progression")]
    public sealed class ChestPoolDefinition : RewardPoolDefinition
    {
        #region Serialized Fields

        [SerializeField] private List<RewardItemData> _sequence = new List<RewardItemData>();
        [SerializeField, Min(1)] private int _windowSize = 3;
        [SerializeField, Min(1)] private int _eraLength = 10;
        [SerializeField, Min(0)] private int _shift;

        #endregion

        #region Public Methods

        public override IRewardSource CreateSource(CoreModelFactory factory)
        {
            return new ChestProgressionSource(factory.GetRewards(_sequence), Mathf.Max(1, _windowSize), Mathf.Max(1, _eraLength), _shift);
        }

        public override void CollectValidationErrors(List<string> errors)
        {
            CollectItemErrors(_sequence, errors);
        }

        #endregion
    }
}
