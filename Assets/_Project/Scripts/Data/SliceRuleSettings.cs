using System;
using UnityEngine;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class SliceRuleSettings
    {
        #region Serialized Fields

        [SerializeField] private RewardPoolDefinition _pool;
        [SerializeField, Min(0)] private int _count = 1;

        #endregion

        #region Properties

        public RewardPoolDefinition Pool
        {
            get { return _pool; }
        }

        public int Count
        {
            get { return _count; }
        }

        #endregion
    }
}
