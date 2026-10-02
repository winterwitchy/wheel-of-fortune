using System.Linq;
using NUnit.Framework;
using UnityEngine;
using WheelGame.Core;
using WheelGame.Data;

namespace WheelGame.Tests.EditMode
{
    public sealed class OverriddenWheelGeneratorTests
    {
        private const int DefaultBombIndex = 3;

        private RewardItemData _filler;
        private RewardItemData _overrideReward;

        [SetUp]
        public void SetUp()
        {
            _filler = ScriptableObject.CreateInstance<RewardItemData>();
            _overrideReward = ScriptableObject.CreateInstance<RewardItemData>();
        }

        [Test]
        public void Generate_RewardOverrideOnBombSlot_ReplacesSliceAndKeepsBomb()
        {
            var generator = CreateGenerator(new SliceOverride(56, DefaultBombIndex, SliceKind.Reward, _overrideReward, 500));

            var slices = generator.Generate(56, ZoneType.Normal);

            Assert.AreSame(_overrideReward, slices[DefaultBombIndex].Reward);
            Assert.AreEqual(500, slices[DefaultBombIndex].Amount);
            Assert.AreEqual(1, slices.Count(slice => slice.IsBomb));
        }

        [Test]
        public void Generate_ZoneWithoutOverrides_IsUnchanged()
        {
            var generator = CreateGenerator(new SliceOverride(56, DefaultBombIndex, SliceKind.Reward, _overrideReward, 500));

            var slices = generator.Generate(57, ZoneType.Normal);

            Assert.IsTrue(slices[DefaultBombIndex].IsBomb);
        }

        [Test]
        public void Generate_BombOverrideOnEmptySlot_AddsBomb()
        {
            var generator = CreateGenerator(new SliceOverride(56, 6, SliceKind.Bomb, null, 1));

            var slices = generator.Generate(56, ZoneType.Normal);

            Assert.IsTrue(slices[6].IsBomb);
            Assert.AreEqual(2, slices.Count(slice => slice.IsBomb));
        }

        [Test]
        public void Generate_BombOverrideOnDefaultBombSlot_KeepsSingleBomb()
        {
            var generator = CreateGenerator(new SliceOverride(56, DefaultBombIndex, SliceKind.Bomb, null, 1));

            var slices = generator.Generate(56, ZoneType.Normal);

            Assert.IsTrue(slices[DefaultBombIndex].IsBomb);
            Assert.AreEqual(1, slices.Count(slice => slice.IsBomb));
        }

        [Test]
        public void Generate_BombOverrideInSafeZone_IsIgnored()
        {
            var generator = CreateGenerator(new SliceOverride(55, 6, SliceKind.Bomb, null, 1));

            var slices = generator.Generate(55, ZoneType.Safe);

            Assert.IsFalse(slices[6].IsBomb);
        }

        private OverriddenWheelGenerator CreateGenerator(params SliceOverride[] overrides)
        {
            var inner = new FixedWheelGenerator(_filler, 1, DefaultBombIndex);
            return new OverriddenWheelGenerator(inner, overrides, new FakeRandomProvider());
        }
    }
}