using System;
using System.Collections.Generic;
using UnityEngine;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class ZoneBandSettings
    {
        [SerializeField, Min(1)] private int _firstZone = 1;
        [SerializeField] private List<SliceRuleSettings> _slices = new List<SliceRuleSettings>();

        public int FirstZone
        {
            get { return _firstZone; }
        }

        public IReadOnlyList<SliceRuleSettings> Slices
        {
            get { return _slices; }
        }
    }
}
