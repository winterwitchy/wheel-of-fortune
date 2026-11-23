using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class OverriddenWheelGeneratorTests
    {
        private const int DEFAULT_BOMB_INDEX = 3;
        private const int NORMAL_ZONE = 56;
        private const int SAFE_ZONE = 55;

        private List<RewardItem> _rewards;
        private RewardItem _overrideReward;

        [SetUp]
        public void SetUp()
        {
            _rewards = TestRewards.CreateMany("reward", WheelLayout.SLICE_COUNT);
            _overrideReward = TestRewards.Create("override", false);
        }

        [Test]
        public void Generate_ZoneWithoutOverrides_IsUnchanged()
        {
            var generator = CreateGenerator(RewardOverride(DEFAULT_BOMB_INDEX));

            var slices = generator.Generate(NORMAL_ZONE + 1, TestZones.NORMAL);

            Assert.IsTrue(slices[DEFAULT_BOMB_INDEX].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
        }

        [Test]
        public void Generate_RewardOverrideOnBombSlot_MovesBombToUnalteredSlice()
        {
            var generator = CreateGenerator(RewardOverride(DEFAULT_BOMB_INDEX));

            var slices = generator.Generate(NORMAL_ZONE, TestZones.NORMAL);

            Assert.AreSame(_overrideReward, slices[DEFAULT_BOMB_INDEX].Reward);
            Assert.AreEqual(500, slices[DEFAULT_BOMB_INDEX].Amount);
            Assert.AreEqual(1, CountBombs(slices));
            Assert.IsTrue(slices[0].IsBomb);
        }

        [Test]
        public void Generate_BombOverrideOnRewardSlot_KeepsOnlyOverrideBombAndMovesDisplacedReward()
        {
            var generator = CreateGenerator(BombOverride(6));

            var slices = generator.Generate(NORMAL_ZONE, TestZones.NORMAL);

            Assert.IsTrue(slices[6].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
            Assert.AreSame(_rewards[6], slices[DEFAULT_BOMB_INDEX].Reward);
        }

        [Test]
        public void Generate_BombOverrideOnDefaultBombSlot_KeepsSingleBomb()
        {
            var generator = CreateGenerator(BombOverride(DEFAULT_BOMB_INDEX));

            var slices = generator.Generate(NORMAL_ZONE, TestZones.NORMAL);

            Assert.IsTrue(slices[DEFAULT_BOMB_INDEX].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
        }

        [Test]
        public void Generate_SeveralBombOverrides_KeepsOnlyTheLastOne()
        {
            var generator = CreateGenerator(BombOverride(1), BombOverride(6));

            var slices = generator.Generate(NORMAL_ZONE, TestZones.NORMAL);

            Assert.IsTrue(slices[6].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
            Assert.AreSame(_rewards[1], slices[1].Reward);
            Assert.AreSame(_rewards[6], slices[DEFAULT_BOMB_INDEX].Reward);
        }

        [Test]
        public void Generate_RewardOverridesOnEverySlice_TurnsLeastRecentlyAlteredSliceIntoBomb()
        {
            var generator = CreateGenerator(RewardOverridesInOrder(5, 0, 1, 2, 3, 4, 6, 7));

            var slices = generator.Generate(NORMAL_ZONE, TestZones.NORMAL);

            Assert.IsTrue(slices[5].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
        }

        [Test]
        public void Generate_RewardOverridesOnEverySlice_UsesTheLatestAlterationOfARepeatedSlice()
        {
            var generator = CreateGenerator(RewardOverridesInOrder(5, 0, 1, 2, 3, 4, 6, 7, 5));

            var slices = generator.Generate(NORMAL_ZONE, TestZones.NORMAL);

            Assert.IsTrue(slices[0].IsBomb);
            Assert.IsFalse(slices[5].IsBomb);
            Assert.AreEqual(1, CountBombs(slices));
        }

        [Test]
        public void Generate_BombOverrideInSafeZone_IsIgnored()
        {
            var generator = CreateGenerator(SliceOverrideRule.CreateBomb(SAFE_ZONE, 6));

            var slices = generator.Generate(SAFE_ZONE, TestZones.SAFE);

            Assert.AreEqual(0, CountBombs(slices));
            Assert.AreSame(_rewards[6], slices[6].Reward);
        }

        [Test]
        public void Generate_InnerBombInRiskFreeZone_IsRemoved()
        {
            var inner = new FixedWheelGenerator(_rewards[0], 1, DEFAULT_BOMB_INDEX);
            var generator = new OverriddenWheelGenerator(inner, new SliceOverrideRule[0], new WheelBombRule(new FakeRandomProvider()));

            var slices = generator.Generate(SAFE_ZONE, TestZones.SAFE);

            Assert.AreEqual(0, CountBombs(slices));
        }

        private OverriddenWheelGenerator CreateGenerator(params SliceOverrideRule[] overrides)
        {
            var inner = new DistinctRewardWheelGenerator(_rewards, DEFAULT_BOMB_INDEX);
            return new OverriddenWheelGenerator(inner, overrides, new WheelBombRule(new FakeRandomProvider()));
        }

        private SliceOverrideRule RewardOverride(int sliceIndex)
        {
            return SliceOverrideRule.CreateReward(NORMAL_ZONE, sliceIndex, _overrideReward, 500);
        }

        private static SliceOverrideRule BombOverride(int sliceIndex)
        {
            return SliceOverrideRule.CreateBomb(NORMAL_ZONE, sliceIndex);
        }

        private SliceOverrideRule[] RewardOverridesInOrder(params int[] sliceIndices)
        {
            var result = new SliceOverrideRule[sliceIndices.Length];

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
