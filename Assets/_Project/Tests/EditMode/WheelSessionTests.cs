using System;
using NUnit.Framework;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public sealed class WheelSessionTests
    {
        private const int BombIndex = 0;
        private const int RewardIndex = 1;
        private const int RewardAmount = 10;
        private const int REVIVE_BASE_COST = 25;
        private const float REVIVE_COST_MULTIPLIER = 2f;
        private const string GOLD = "gold";
        private const int STARTING_GOLD = 1000;

        private RewardItem _reward;
        private InMemoryWallet _wallet;
        private InMemoryInventory _inventory;
        private RewardVault _vault;

        [SetUp]
        public void SetUp()
        {
            _reward = TestRewards.Create("chest_small", false);
            _wallet = CreateWallet(STARTING_GOLD);
            _inventory = new InMemoryInventory();
            _vault = new RewardVault(_wallet, _inventory);
        }

        [Test]
        public void StartRun_EntersFirstZoneReadyToSpin()
        {
            var session = CreateSession();

            session.StartRun();

            Assert.AreEqual(1, session.CurrentZone);
            Assert.AreEqual(SessionState.ReadyToSpin, session.State);
            Assert.AreEqual(WheelLayout.SLICE_COUNT, session.CurrentWheel.Count);
        }

        [Test]
        public void CompleteSpin_OnReward_CollectsRewardAndWaitsForTheClaim()
        {
            var session = CreateSession(RewardIndex);
            session.StartRun();
            session.Spin();

            session.CompleteSpin();

            Assert.AreEqual(SessionState.CollectingReward, session.State);
            Assert.AreEqual(1, session.CurrentZone);
            Assert.AreEqual(RewardAmount, session.CollectedRewards[_reward]);
            Assert.IsFalse(session.CanSpin);
            Assert.IsFalse(session.CanLeave);
        }

        [Test]
        public void FinishCollecting_AdvancesToTheNextZone()
        {
            var session = CreateSession(RewardIndex);
            session.StartRun();
            session.Spin();
            session.CompleteSpin();

            session.FinishCollecting();

            Assert.AreEqual(SessionState.ReadyToSpin, session.State);
            Assert.AreEqual(2, session.CurrentZone);
        }

        [Test]
        public void FinishCollecting_WhenNotCollecting_Throws()
        {
            var session = CreateSession();
            session.StartRun();

            Assert.Throws<InvalidOperationException>(() => session.FinishCollecting());
        }

        [Test]
        public void StartRun_RaisesRunStartedBeforeTheFirstZone()
        {
            var session = CreateSession();
            var zoneWhenRunStarted = -1;
            session.RunStarted += () => zoneWhenRunStarted = session.CurrentZone;

            session.StartRun();

            Assert.AreEqual(0, zoneWhenRunStarted);
            Assert.AreEqual(1, session.CurrentZone);
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
            Assert.Throws<InvalidOperationException>(() => session.Leave(_vault));
            Assert.AreEqual(0, _inventory.Items.Count);
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
                var goldBefore = _wallet.GetBalance(GOLD);

                Assert.AreEqual(expectedCost, session.ReviveCost);
                Assert.IsTrue(session.TryRevive(_wallet));
                Assert.AreEqual(goldBefore - expectedCost, _wallet.GetBalance(GOLD));
            }
        }

        [Test]
        public void TryRevive_WithoutEnoughGold_KeepsBombHitAndGold()
        {
            var poorWallet = CreateWallet(REVIVE_BASE_COST - 1);
            var session = CreateSession(BombIndex);
            session.StartRun();
            SpinAndComplete(session);

            Assert.IsFalse(session.CanRevive(poorWallet));
            Assert.IsFalse(session.TryRevive(poorWallet));
            Assert.AreEqual(SessionState.BombHit, session.State);
            Assert.AreEqual(REVIVE_BASE_COST - 1, poorWallet.GetBalance(GOLD));
        }

        [Test]
        public void TryRevive_WhenNotBombHit_ThrowsWithoutSpending()
        {
            var session = CreateSession();
            session.StartRun();

            Assert.IsFalse(session.CanRevive(_wallet));
            Assert.Throws<InvalidOperationException>(() => session.TryRevive(_wallet));
            Assert.AreEqual(STARTING_GOLD, _wallet.GetBalance(GOLD));
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
            Assert.IsFalse(session.CanLeave);

            session.FinishCollecting();
            Assert.IsTrue(session.CanLeave);
            Assert.AreEqual(3, session.CurrentZone);
        }

        [Test]
        public void Leave_AfterFirstReward_CashesOut()
        {
            var session = CreateSession(RewardIndex);
            session.StartRun();
            SpinAndComplete(session);

            session.Leave(_vault);

            Assert.AreEqual(SessionState.CashedOut, session.State);
        }

        [Test]
        public void Leave_DepositsEveryCollectedRewardIntoTheVault()
        {
            var session = CreateSession(RewardIndex, RewardIndex, RewardIndex);
            session.StartRun();

            for (var i = 0; i < 3; i++)
            {
                SpinAndComplete(session);
            }

            session.Leave(_vault);

            Assert.AreEqual(RewardAmount * 3, _inventory.GetCount("chest_small"));
        }

        [Test]
        public void StartRun_AfterBombWithoutLeaving_DepositsNothing()
        {
            var session = CreateSession(RewardIndex, BombIndex);
            session.StartRun();
            SpinAndComplete(session);
            SpinAndComplete(session);

            session.StartRun();

            Assert.AreEqual(0, _inventory.Items.Count);
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
                TestZones.CreateRules(),
                new FixedWheelGenerator(_reward, RewardAmount, BombIndex),
                leaveRule,
                new RevivePricing(GOLD, REVIVE_BASE_COST, REVIVE_COST_MULTIPLIER),
                new FakeRandomProvider(spinIndices));
        }

        private static InMemoryWallet CreateWallet(int gold)
        {
            var wallet = new InMemoryWallet();
            wallet.Add(GOLD, gold);
            return wallet;
        }

        private static void SpinAndComplete(WheelSession session)
        {
            session.Spin();
            session.CompleteSpin();

            if (session.State == SessionState.CollectingReward)
            {
                session.FinishCollecting();
            }
        }
    }
}