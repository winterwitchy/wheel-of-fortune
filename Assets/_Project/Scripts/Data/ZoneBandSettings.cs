using System;
using System.Collections.Generic;
using UnityEngine;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class ZoneBandSettings
    {
        #region Serialized Fields

        [SerializeField, Min(1)] private int _firstZone = 1;
        [SerializeField] private List<SliceRuleSettings> _slices = new List<SliceRuleSettings>();

        #endregion

        #region Properties

        public int FirstZone
        {
            get { return _firstZone; }
        }

        public IReadOnlyList<SliceRuleSettings> Slices
        {
            get { return _slices; }
        }

        #endregion
    }
}
