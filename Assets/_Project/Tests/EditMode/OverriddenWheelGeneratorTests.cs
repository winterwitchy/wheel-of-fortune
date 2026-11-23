using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using WheelGame.Core;
using WheelGame.Data;

namespace WheelGame.Tests.EditMode
{
    public sealed class OverriddenWheelGeneratorTests
    {
        private const int DEFAULT_BOMB_INDEX = 3;
        private const int NORMAL_ZONE = 56;
        private const int SAFE_ZONE = 55;

        private List<RewardItemData> _rewards;
        private RewardItemData _overrideReward;

        [SetUp]
        public void SetUp()
        {
            _rewards = new List<RewardItemData>();

            for (var i = 0; i < WheelLayout.SliceCount; i++)
            {
                _rewards.Add(ScriptableObject.CreateInstance<RewardItemData>());
            }

            _overrideReward = ScriptableObject.CreateInstance<RewardItemData>();
        }

        [Test]
        public void Generate_ZoneWithoutOverrides_IsUnchanged()
        {
            var generator = CreateGenerator(RewardOverride(DEFAULT_BOMB_INDEX));

            var slices = generator.Generate(NORMAL_ZONE + 1, ZoneType.Normal);

            Assert.IsTrue(slices[DEFAULT_BOMB_INDEX].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
        }

        [Test]
        public void Generate_RewardOverrideOnBombSlot_MovesBombToUnalteredSlice()
        {
            var generator = CreateGenerator(RewardOverride(DEFAULT_BOMB_INDEX));

            var slices = generator.Generate(NORMAL_ZONE, ZoneType.Normal);

            Assert.AreSame(_overrideReward, slices[DEFAULT_BOMB_INDEX].Reward);
            Assert.AreEqual(500, slices[DEFAULT_BOMB_INDEX].Amount);
            Assert.AreEqual(1, CountBombs(slices));
            Assert.IsTrue(slices[0].IsBomb);
        }

        [Test]
        public void Generate_BombOverrideOnRewardSlot_KeepsOnlyOverrideBombAndMovesDisplacedReward()
        {
            var generator = CreateGenerator(BombOverride(6));

            var slices = generator.Generate(NORMAL_ZONE, ZoneType.Normal);

            Assert.IsTrue(slices[6].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
            Assert.AreSame(_rewards[6], slices[DEFAULT_BOMB_INDEX].Reward);
        }

        [Test]
        public void Generate_BombOverrideOnDefaultBombSlot_KeepsSingleBomb()
        {
            var generator = CreateGenerator(BombOverride(DEFAULT_BOMB_INDEX));

            var slices = generator.Generate(NORMAL_ZONE, ZoneType.Normal);

            Assert.IsTrue(slices[DEFAULT_BOMB_INDEX].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
        }

        [Test]
        public void Generate_SeveralBombOverrides_KeepsOnlyTheLastOne()
        {
            var generator = CreateGenerator(BombOverride(1), BombOverride(6));

            var slices = generator.Generate(NORMAL_ZONE, ZoneType.Normal);

            Assert.IsTrue(slices[6].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
            Assert.AreSame(_rewards[1], slices[1].Reward);
            Assert.AreSame(_rewards[6], slices[DEFAULT_BOMB_INDEX].Reward);
        }

        [Test]
        public void Generate_RewardOverridesOnEverySlice_TurnsLeastRecentlyAlteredSliceIntoBomb()
        {
            var generator = CreateGenerator(RewardOverridesInOrder(5, 0, 1, 2, 3, 4, 6, 7));

            var slices = generator.Generate(NORMAL_ZONE, ZoneType.Normal);

            Assert.IsTrue(slices[5].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
        }

        [Test]
        public void Generate_RewardOverridesOnEverySlice_UsesTheLatestAlterationOfARepeatedSlice()
        {
            var generator = CreateGenerator(RewardOverridesInOrder(5, 0, 1, 2, 3, 4, 6, 7, 5));

            var slices = generator.Generate(NORMAL_ZONE, ZoneType.Normal);

            Assert.IsTrue(slices[0].IsBomb);
            Assert.IsFalse(slices[5].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
        }

        [Test]
        public void Generate_BombOverrideInSafeZone_IsIgnored()
        {
            var generator = CreateGenerator(new SliceOverride(SAFE_ZONE, 6, SliceKind.Bomb, null, 1));

            var slices = generator.Generate(SAFE_ZONE, ZoneType.Safe);

            Assert.AreEqual(0, CountBombs(slices));
            Assert.AreSame(_rewards[6], slices[6].Reward);
        }

        [Test]
        public void Generate_InnerBombInRiskFreeZone_IsRemoved()
        {
            var inner = new FixedWheelGenerator(_rewards[0], 1, DEFAULT_BOMB_INDEX);
            var generator = new OverriddenWheelGenerator(inner, new SliceOverride[0], new WheelBombRule(new FakeRandomProvider()));

            var slices = generator.Generate(SAFE_ZONE, ZoneType.Safe);

            Assert.AreEqual(0, CountBombs(slices));
        }

        [Test]
        public void Generate_RewardOverrideWithoutReward_IsIgnored()
        {
            var generator = CreateGenerator(new SliceOverride(NORMAL_ZONE, 2, SliceKind.Reward, null, 5));

            var slices = generator.Generate(NORMAL_ZONE, ZoneType.Normal);

            Assert.AreSame(_rewards[2], slices[2].Reward);
            Assert.AreEqual(1, CountBombs(slices));
        }

        private OverriddenWheelGenerator CreateGenerator(params SliceOverride[] overrides)
        {
            var inner = new DistinctRewardWheelGenerator(_rewards, DEFAULT_BOMB_INDEX);
            return new OverriddenWheelGenerator(inner, overrides, new WheelBombRule(new FakeRandomProvider()));
        }

        private SliceOverride RewardOverride(int sliceIndex)
        {
            return new SliceOverride(NORMAL_ZONE, sliceIndex, SliceKind.Reward, _overrideReward, 500);
        }

        private static SliceOverride BombOverride(int sliceIndex)
        {
            return new SliceOverride(NORMAL_ZONE, sliceIndex, SliceKind.Bomb, null, 1);
        }

        private SliceOverride[] RewardOverridesInOrder(params int[] sliceIndices)
        {
            var result = new SliceOverride[sliceIndices.Length];

            for (var i = 0; i < sliceIndices.Length; i++)
            {
                result[i] = RewardOverride(sliceIndices[i]);
            }

            return result;
        }

        private static int CountBombs(IReadOnlyList<WheelSliceEntry> slices)
        {
            return slices.Count(slice => slice.IsBomb);
        }
    }
}
