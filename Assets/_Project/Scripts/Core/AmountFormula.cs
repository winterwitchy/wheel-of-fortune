using System;

namespace WheelGame.Core
{
    public sealed class AmountFormula
    {
        private readonly GrowthCurve _growth;
        private readonly int _divisor;
        private readonly int _offset;
        private readonly int _step;

        public AmountFormula(GrowthCurve growth, int divisor, int offset, int step)
        {
            if (divisor < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(divisor), divisor, "Divisor must be at least 1.");
            }

            if (step < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(step), step, "Step must be at least 1.");
            }

            _growth = growth;
            _divisor = divisor;
            _offset = offset;
            _step = step;
        }

        public int Evaluate(int zone)
        {
            return (Grow(zone) / _divisor + _offset) * _step;
        }

        private int Grow(int zone)
        {
            if (_growth == GrowthCurve.Quadratic)
            {
                return zone * zone;
            }

            if (_growth == GrowthCurve.SquareRoot)
            {
                return (int)Math.Sqrt(zone);
            }

            return zone;
        }
    }
}
