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

        private RewardItemData _reward;

        [SetUp]
        public void SetUp()
        {
            _reward = ScriptableObject.CreateInstance<RewardItemData>();
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

            session.Revive();

            Assert.AreEqual(SessionState.ReadyToSpin, session.State);
            Assert.AreEqual(2, session.CurrentZone);
            Assert.AreEqual(RewardAmount, session.CollectedRewards[_reward]);
        }

        [Test]
        public void Leave_AtFirstZoneWithoutSpinning_CashesOut()
        {
            var session = CreateSession();
            session.StartRun();

            session.Leave();

            Assert.AreEqual(SessionState.CashedOut, session.State);
        }

        [Test]
        public void CanLeave_BeforeAnySpin_ClosesDuringSpinAndReopensInNextZone()
        {
            var session = CreateSession(LeaveRule.BeforeAnySpin, RewardIndex);
            session.StartRun();

            Assert.IsTrue(session.CanLeave);

            session.Spin();
            Assert.IsFalse(session.CanLeave);

            session.CompleteSpin();
            Assert.IsTrue(session.CanLeave);
            Assert.AreEqual(2, session.CurrentZone);
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
            return CreateSession(LeaveRule.BeforeAnySpin, spinIndices);
        }

        private WheelSession CreateSession(LeaveRule leaveRule, params int[] spinIndices)
        {
            return new WheelSession(
                new ZoneRules(5, 30),
                new FixedWheelGenerator(_reward, RewardAmount, BombIndex),
                leaveRule,
                new FakeRandomProvider(spinIndices));
        }

        private static void SpinAndComplete(WheelSession session)
        {
            session.Spin();
            session.CompleteSpin();
        }
    }
}