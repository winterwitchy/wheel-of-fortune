using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class RewardSourceTests
    {
        private List<RewardItem> _chests;

        [SetUp]
        public void SetUp()
        {
            _chests = TestRewards.CreateMany("chest", 7);
        }

        [TestCase(1, 0, 0)]
        [TestCase(1, 2, 2)]
        [TestCase(10, 2, 2)]
        [TestCase(11, 0, 1)]
        [TestCase(51, 2, 6)]
        [TestCase(200, 0, 6)]
        public void ChestProgression_ShiftsItsWindowEveryEra(int zone, int windowIndex, int expectedIndex)
        {
            var source = new ChestProgressionSource(_chests, 3, 10, 0);

            Assert.AreSame(_chests[expectedIndex], source.GetOption(zone, windowIndex));
        }

        [TestCase(1, 3)]
        [TestCase(11, 4)]
        [TestCase(200, 6)]
        public void ChestProgression_WithShift_ReturnsTheChestAboveTheWindow(int zone, int expectedIndex)
        {
            var draft = new WheelDraft();

            new ChestProgressionSource(_chests, 1, 10, 3).AddTo(draft, zone, 1, new FakeRandomProvider());

            Assert.AreSame(_chests[expectedIndex], draft.Slices[0].Reward);
        }

        [Test]
        public void ItemPool_MultipliesFormulaAmountsAndDoublesRepeats()
        {
            var cash = TestRewards.Create("cash", true);
            var source = new ItemPoolSource(new List<RewardItem> { cash }, new AmountFormula(GrowthCurve.Quadratic, 10, 1, 25), false);
            var draft = new WheelDraft();

            source.AddTo(draft, 5, 2, new FakeRandomProvider());
            source.AddTo(draft, 5, 2, new FakeRandomProvider());

            CollectionAssertSequence(new[] { 150, 300 }, draft.Slices.Select(slice => slice.Amount));
        }

        [Test]
        public void ItemPool_WithoutFormula_GivesOneAndIgnoresTheMultiplier()
        {
            var skin = TestRewards.Create("skin", false);
            var draft = new WheelDraft();

            new ItemPoolSource(new List<RewardItem> { skin }, null, true).AddTo(draft, 30, 5, new FakeRandomProvider());

            Assert.AreEqual(1, draft.Slices[0].Amount);
        }

        [Test]
        public void UniquePool_WhenEmpty_UsesItsFallback()
        {
            var skins = TestRewards.CreateMany("skin", 2);
            var source = new ItemPoolSource(skins, null, true);
            source.SetFallback(new ChestProgressionSource(_chests, 1, 10, 3));
            var draft = new WheelDraft();

            for (var i = 0; i < 3; i++)
            {
                source.AddTo(draft, 1, 1, new FakeRandomProvider());
            }

            Assert.AreEqual(2, draft.Slices.Count(slice => skins.Contains(slice.Reward)));
            Assert.AreSame(_chests[3], draft.Slices[2].Reward);
        }

        [Test]
        public void SharedPool_PrefersUnusedItemsBeforeRepeating()
        {
            var items = TestRewards.CreateMany("item", 2);
            var source = new ItemPoolSource(items, new AmountFormula(GrowthCurve.Linear, 1, 0, 1), false);
            var draft = new WheelDraft();

            source.AddTo(draft, 4, 1, new FakeRandomProvider());
            source.AddTo(draft, 4, 1, new FakeRandomProvider());

            Assert.AreNotSame(draft.Slices[0].Reward, draft.Slices[1].Reward);
        }

        private static void CollectionAssertSequence(IEnumerable<int> expected, IEnumerable<int> actual)
        {
            Assert.AreEqual(string.Join(",", expected), string.Join(",", actual));
        }
    }
}
