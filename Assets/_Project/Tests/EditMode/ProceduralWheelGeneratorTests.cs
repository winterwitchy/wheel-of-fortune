using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using WheelGame.Core;
using WheelGame.Data;

namespace WheelGame.Tests.EditMode
{
    public sealed class ProceduralWheelGeneratorTests
    {
        private RewardItemData _cash;
        private RewardItemData _gold;
        private RewardItemData _points;
        private RewardItemData _superChest;
        private List<RewardItemData> _chests;
        private WheelGenerationSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _cash = CreateReward();
            _gold = CreateReward();
            _points = CreateReward();
            _superChest = CreateReward();
            _chests = new List<RewardItemData> { CreateReward(), CreateReward(), CreateReward() };

            _settings = new WheelGenerationSettings(
                _cash, new AmountFormula(2, 10, 1, 25),
                _gold, new AmountFormula(1, 1, 0, 1),
                new List<RewardItemData> { _points }, new AmountFormula(1, 2, 1, 5),
                new List<RewardItemData> { CreateReward() }, new List<RewardItemData> { CreateReward() },
                new ChestProgression(_chests, 3, 30), _superChest,
                1, 5);
        }

        [Test]
        public void Generate_NormalZone_HasOneBombAndTwoChests()
        {
            var slices = CreateGenerator().Generate(1, ZoneType.Normal);

            Assert.AreEqual(WheelLayout.SliceCount, slices.Count);
            Assert.AreEqual(1, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(2, slices.Count(slice => _chests.Contains(slice.Reward)));
        }

        [Test]
        public void Generate_SafeZone_HasNoBombAndThreeChests()
        {
            var slices = CreateGenerator().Generate(5, ZoneType.Safe);

            Assert.AreEqual(0, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(3, slices.Count(slice => _chests.Contains(slice.Reward)));
        }

        [Test]
        public void Generate_SuperZone_HasThreeSuperChestsAndNoBomb()
        {
            var slices = CreateGenerator().Generate(30, ZoneType.Super);

            Assert.AreEqual(0, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(3, slices.Count(slice => slice.Reward == _superChest));
        }

        [Test]
        public void Generate_SuperZone_MultipliesCashGoldAndPoints()
        {
            var slices = CreateGenerator().Generate(30, ZoneType.Super);

            Assert.IsTrue(slices.Where(slice => slice.Reward == _cash).All(slice => slice.Amount == 2275 * 5));
            Assert.AreEqual(30 * 5, slices.Single(slice => slice.Reward == _gold).Amount);
            Assert.AreEqual(80 * 5, slices.Single(slice => slice.Reward == _points).Amount);
        }

        private ProceduralWheelGenerator CreateGenerator()
        {
            return new ProceduralWheelGenerator(_settings, new FakeRandomProvider());
        }

        private static RewardItemData CreateReward()
        {
            return ScriptableObject.CreateInstance<RewardItemData>();
        }
    }
}