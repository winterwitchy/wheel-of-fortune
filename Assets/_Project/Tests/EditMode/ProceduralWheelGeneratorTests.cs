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
        private RewardItemData _consumable;
        private RewardItemData _safeSkin;
        private RewardItemData _superSkin;
        private RewardItemData _superChest;
        private List<RewardItemData> _chests;
        private WheelGenerationSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _cash = CreateReward();
            _gold = CreateReward();
            _points = CreateReward();
            _consumable = CreateReward();
            _safeSkin = CreateReward();
            _superSkin = CreateReward();
            _superChest = CreateReward();
            _chests = new List<RewardItemData> { CreateReward(), CreateReward(), CreateReward() };

            _settings = new WheelGenerationSettings(
                _cash, new AmountFormula(GrowthCurve.Quadratic, 10, 1, 25),
                _gold, new AmountFormula(GrowthCurve.Linear, 1, 0, 1),
                new List<RewardItemData> { _points }, new AmountFormula(GrowthCurve.Linear, 2, 1, 5),
                new List<RewardItemData> { _consumable }, new AmountFormula(GrowthCurve.SquareRoot, 1, 0, 2),
                new List<RewardItemData> { _safeSkin }, new List<RewardItemData> { _superSkin },
                new ChestProgression(_chests, 3, 30), _superChest,
                1, 5);
        }

        [Test]
        public void Generate_NormalZone_HasOneBombTwoChestsAndOneConsumable()
        {
            var slices = CreateGenerator().Generate(1, ZoneType.Normal);

            Assert.AreEqual(WheelLayout.SliceCount, slices.Count);
            Assert.AreEqual(1, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(2, slices.Count(slice => _chests.Contains(slice.Reward)));
            Assert.AreEqual(2, slices.Single(slice => slice.Reward == _consumable).Amount);
        }

        [Test]
        public void Generate_SafeZone_ReplacesBombWithSafeSkin()
        {
            var slices = CreateGenerator().Generate(5, ZoneType.Safe);

            Assert.AreEqual(0, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(2, slices.Count(slice => _chests.Contains(slice.Reward)));
            Assert.AreEqual(1, slices.Count(slice => slice.Reward == _consumable));
            Assert.AreEqual(1, slices.Count(slice => slice.Reward == _safeSkin));
        }

        [Test]
        public void Generate_SuperZone_HasTwoSuperChestsAndTwoSuperSkins()
        {
            var slices = CreateGenerator().Generate(30, ZoneType.Super);

            Assert.AreEqual(0, slices.Count(slice => slice.IsBomb));
            Assert.AreEqual(2, slices.Count(slice => slice.Reward == _superChest));
            Assert.AreEqual(2, slices.Count(slice => slice.Reward == _superSkin));
            Assert.AreEqual(1, slices.Count(slice => slice.Reward == _consumable));
        }

        [Test]
        public void Generate_SuperZone_MultipliesCountableRewards()
        {
            var slices = CreateGenerator().Generate(30, ZoneType.Super);

            Assert.AreEqual(2275 * 5, slices.Single(slice => slice.Reward == _cash).Amount);
            Assert.AreEqual(30 * 5, slices.Single(slice => slice.Reward == _gold).Amount);
            Assert.AreEqual(80 * 5, slices.Single(slice => slice.Reward == _points).Amount);
            Assert.AreEqual(10 * 5, slices.Single(slice => slice.Reward == _consumable).Amount);
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