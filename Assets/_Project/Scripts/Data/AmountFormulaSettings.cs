using System;
using UnityEngine;
using WheelGame.Core;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class AmountFormulaSettings
    {
        [SerializeField] private GrowthCurve _growth = GrowthCurve.Linear;
        [SerializeField, Min(1)] private int _divisor = 1;
        [SerializeField, Min(0)] private int _offset;
        [SerializeField, Min(1)] private int _step = 1;

        public AmountFormula ToFormula()
        {
            return new AmountFormula(_growth, Mathf.Max(1, _divisor), Mathf.Max(0, _offset), Mathf.Max(1, _step));
        }
    }
}
