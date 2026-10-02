using System;
using UnityEngine;

namespace WheelGame.Data
{
    [Serializable]
    public sealed class AmountFormula
    {
        [SerializeField, Min(1)] private int _power = 1;
        [SerializeField, Min(1)] private int _divisor = 1;
        [SerializeField, Min(0)] private int _offset;
        [SerializeField, Min(1)] private int _step = 1;

        public AmountFormula() { }

        public AmountFormula(int power, int divisor, int offset, int step)
        {
            _power = power;
            _divisor = divisor;
            _offset = offset;
            _step = step;
        }

        public int Evaluate(int zone)
        {
            int powered = 1;
            for (int i = 0; i < _power; i++)
                powered *= zone;

            return (powered / _divisor + _offset) * _step;
        }
    }
}