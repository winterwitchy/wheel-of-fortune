using System.Collections.Generic;
using NUnit.Framework;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class FakeRandomProvider : IRandomProvider
    {
        private readonly Queue<int> _values;

        public FakeRandomProvider(params int[] values)
        {
            _values = new Queue<int>(values);
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            var value = _values.Count > 0 ? _values.Dequeue() : minInclusive;
            Assert.That(value, Is.InRange(minInclusive, maxExclusive - 1));
            return value;
        }
    }
}