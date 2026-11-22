using System;
using NUnit.Framework;
using UnityEngine;
using WheelGame.Core;
using WheelGame.Data;

namespace WheelGame.Tests.EditMode
{
    public sealed class WheelSessionTests
    {
        private const int BombIndex = 0;
        private const int RewardIndex = 1;
        private const int RewardAmount = 10;
        private const int REVIVE_BASE_COST = 25;
        private const float REVIVE_COST_MULTIPLIER = 2f;

        private RewardItemData _reward;
        private InMemoryWallet _wallet;

        [SetUp]
        public void SetUp()
        {
            _reward = ScriptableObject.CreateInstance<RewardItemData>();
            _wallet = new InMemoryWallet(1000);
        }

        [Test]
        public void StartRun_EntersFirstZoneReadyToSpin()
        {
            var session = CreateSession();

            session.StartRun();

            Assert.AreEqual(1, session.CurrentZone);
            Assert.AreEqual(SessionState.ReadyToSpin, session.State);
            Assert.AreEqual(WheelLayout.SliceCount, session.CurrentWheel.Count);
        }

        [Test]
        public void CompleteSpin_OnReward_CollectsRewardAndAdvancesZone()
        {
            var session = CreateSession(RewardIndex);
            session.StartRun();

            SpinAndComplete(session);

            Assert.AreEqual(2, session.CurrentZone);
            Assert.AreEqual(RewardAmount, session.CollectedRewards[_reward]);
        }

        [Test]
        public void CompleteSpin_OnBomb_EntersBombHit()
        {
            var session = CreateSession(BombIndex);
            session.StartRun();

            SpinAndComplete(session);

            Assert.AreEqual(SessionState.BombHit, session.State);
            Assert.IsFalse(session.CanSpin);
            Assert.IsFalse(session.CanLeave);
        }

        [Test]
        public void Revive_AfterBomb_KeepsZoneAndRewards()
        {
            var session = CreateSession(RewardIndex, BombIndex);
            session.StartRun();
            SpinAndComplete(session);
            SpinAndComplete(session);

            Assert.IsTrue(session.TryRevive(_wallet));

            Assert.AreEqual(SessionState.ReadyToSpin, session.State);
            Assert.AreEqual(2, session.CurrentZone);
            Assert.AreEqual(RewardAmount, session.CollectedRewards[_reward]);
        }

        [Test]
        public void CanLeave_BeforeAnyReward_IsFalse()
        {
            var session = CreateSession();
            session.StartRun();

            Assert.IsFalse(session.CanLeave);
            Assert.Throws<InvalidOperationException>(() => session.Leave());
        }

        [Test]
        public void CanLeave_AfterRevivingWithoutRewards_IsFalse()
        {
            var session = CreateSession(BombIndex);
            session.StartRun();
            SpinAndComplete(session);

            session.TryRevive(_wallet);

            Assert.AreEqual(SessionState.ReadyToSpin, session.State);
            Assert.IsFalse(session.CanLeave);
        }

        [Test]
        public void TryRevive_IncreasesCostOnEveryUse()
        {
            var session = CreateSession(BombIndex, BombIndex, BombIndex);
            session.StartRun();

            for (var expectedCost = REVIVE_BASE_COST; expectedCost <= REVIVE_BASE_COST * 4; expectedCost *= 2)
            {
                SpinAndComplete(session);
                var goldBefore = _wallet.Gold;

                Assert.AreEqual(expectedCost, session.ReviveCost);
                Assert.IsTrue(session.TryRevive(_wallet));
                Assert.AreEqual(goldBefore - expectedCost, _wallet.Gold);
            }
        }

        [Test]
        public void TryRevive_WithoutEnoughGold_KeepsBombHitAndGold()
        {
            var poorWallet = new InMemoryWallet(REVIVE_BASE_COST - 1);
            var session = CreateSession(BombIndex);
            session.StartRun();
            SpinAndComplete(session);

            Assert.IsFalse(session.CanRevive(poorWallet));
            Assert.IsFalse(session.TryRevive(poorWallet));
            Assert.AreEqual(SessionState.BombHit, session.State);
            Assert.AreEqual(REVIVE_BASE_COST - 1, poorWallet.Gold);
        }

        [Test]
        public void TryRevive_WhenNotBombHit_ThrowsWithoutSpending()
        {
            var session = CreateSession();
            session.StartRun();

            Assert.IsFalse(session.CanRevive(_wallet));
            Assert.Throws<InvalidOperationException>(() => session.TryRevive(_wallet));
            Assert.AreEqual(1000, _wallet.Gold);
        }

        [Test]
        public void StartRun_ResetsReviveCost()
        {
            var session = CreateSession(BombIndex);
            session.StartRun();
            SpinAndComplete(session);
            session.TryRevive(_wallet);

            session.StartRun();

            Assert.AreEqual(REVIVE_BASE_COST, session.ReviveCost);
        }

        [Test]
        public void CanLeave_AfterFirstReward_ClosesDuringSpinAndReopensInNextZone()
        {
            var session = CreateSession(LeaveRule.AfterFirstReward, RewardIndex, RewardIndex);
            session.StartRun();
            SpinAndComplete(session);

            Assert.IsTrue(session.CanLeave);

            session.Spin();
            Assert.IsFalse(session.CanLeave);

            session.CompleteSpin();
            Assert.IsTrue(session.CanLeave);
            Assert.AreEqual(3, session.CurrentZone);
        }

        [Test]
        public void Leave_AfterFirstReward_CashesOut()
        {
            var session = CreateSession(RewardIndex);
            session.StartRun();
            SpinAndComplete(session);

            session.Leave();

            Assert.AreEqual(SessionState.CashedOut, session.State);
        }

        [Test]
        public void CanLeave_WithSafeZonesOnly_IsFalseInNormalZoneAndTrueInSafeZone()
        {
            var session = CreateSession(LeaveRule.SafeZonesOnly, RewardIndex, RewardIndex, RewardIndex, RewardIndex);
            session.StartRun();

            Assert.IsFalse(session.CanLeave);

            for (var i = 0; i < 4; i++)
                SpinAndComplete(session);

            Assert.AreEqual(5, session.CurrentZone);
            Assert.IsTrue(session.CanLeave);
        }

        [Test]
        public void Spin_WhileSpinning_Throws()
        {
            var session = CreateSession(RewardIndex);
            session.StartRun();
            session.Spin();

            Assert.Throws<InvalidOperationException>(() => session.Spin());
        }

        [Test]
        public void StartRun_AfterCollecting_ClearsRewards()
        {
            var session = CreateSession(RewardIndex);
            session.StartRun();
            SpinAndComplete(session);

            session.StartRun();

            Assert.AreEqual(1, session.CurrentZone);
            Assert.AreEqual(0, session.CollectedRewards.Count);
        }

        private WheelSession CreateSession(params int[] spinIndices)
        {
            return CreateSession(LeaveRule.AfterFirstReward, spinIndices);
        }

        private WheelSession CreateSession(LeaveRule leaveRule, params int[] spinIndices)
        {
            return new WheelSession(
                new ZoneRules(5, 30),
                new FixedWheelGenerator(_reward, RewardAmount, BombIndex),
                leaveRule,
                new RevivePricing(REVIVE_BASE_COST, REVIVE_COST_MULTIPLIER),
                new FakeRandomProvider(spinIndices));
        }

        private static void SpinAndComplete(WheelSession session)
        {
            session.Spin();
            session.CompleteSpin();
        }
    }
}