using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class AmountFormula
    {
        [FormerlySerializedAs("_power")]
        [SerializeField] private GrowthCurve _growth = GrowthCurve.Linear;
        [SerializeField, Min(1)] private int _divisor = 1;
        [SerializeField, Min(0)] private int _offset;
        [SerializeField, Min(1)] private int _step = 1;

        public AmountFormula() { }

        public AmountFormula(GrowthCurve growth, int divisor, int offset, int step)
        {
            _growth = growth;
            _divisor = divisor;
            _offset = offset;
            _step = step;
        }

        public int Evaluate(int zone)
        {
            var grown = _growth switch
            {
                GrowthCurve.Quadratic => zone * zone,
                GrowthCurve.SquareRoot => (int)Math.Sqrt(zone),
                _ => zone
            };

            return (grown / _divisor + _offset) * _step;
        }
    }
}