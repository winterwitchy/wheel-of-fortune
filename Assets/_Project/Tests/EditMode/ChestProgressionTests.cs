using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WheelGame.Data;

namespace WheelGame.Tests.EditMode
{
    public sealed class ChestProgressionTests
    {
        private List<RewardItemData> _sequence;
        private ChestProgression _progression;

        [SetUp]
        public void SetUp()
        {
            _sequence = new List<RewardItemData>();
            for (var i = 0; i < 7; i++)
                _sequence.Add(ScriptableObject.CreateInstance<RewardItemData>());

            _progression = new ChestProgression(_sequence, 3, 30);
        }

        [TestCase(1, 0, 0)]
        [TestCase(1, 2, 2)]
        [TestCase(29, 2, 2)]
        [TestCase(31, 0, 1)]
        [TestCase(151, 0, 5)]
        [TestCase(151, 2, 6)]
        [TestCase(200, 0, 6)]
        public void GetOption_ReturnsExpectedChest(int zone, int windowIndex, int expectedSequenceIndex)
        {
            Assert.AreSame(_sequence[expectedSequenceIndex], _progression.GetOption(zone, windowIndex));
        }
    }
}