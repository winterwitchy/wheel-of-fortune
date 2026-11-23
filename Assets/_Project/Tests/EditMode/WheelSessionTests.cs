using System;
using NUnit.Framework;
using WheelGame.Core;
using WheelGame.Core.Events;

namespace WheelGame.Tests.EditMode
{
    public sealed class WheelSessionTests
    {
        private const int BOMB_INDEX = 0;
        private const int REWARD_INDEX = 1;
        private const int REWARD_AMOUNT = 10;
        private const int REVIVE_BASE_COST = 25;
        private const float REVIVE_COST_MULTIPLIER = 2f;
        private const string GOLD = "gold";
        private const int STARTING_GOLD = 1000;

        private RewardItem _reward;
        private InMemoryWallet _wallet;
        private InMemoryInventory _inventory;
        private RewardVault _vault;
        private EventBus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _reward = TestRewards.Create("chest_small", false);
            _wallet = CreateWallet(STARTING_GOLD);
            _inventory = new InMemoryInventory(new EventBus());
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
            var session = CreateSession(REWARD_INDEX);
            session.StartRun();
            session.Spin();

            session.CompleteSpin();

            Assert.AreEqual(SessionState.CollectingReward, session.State);
            Assert.AreEqual(1, session.CurrentZone);
            Assert.AreEqual(REWARD_AMOUNT, session.CollectedRewards[_reward]);
            Assert.IsFalse(session.CanSpin);
            Assert.IsFalse(session.CanLeave);
        }

        [Test]
        public void FinishCollecting_AdvancesToTheNextZone()
        {
            var session = CreateSession(REWARD_INDEX);
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
            _bus.Subscribe<RunStartedEvent>(message => zoneWhenRunStarted = session.CurrentZone);

            session.StartRun();

            Assert.AreEqual(0, zoneWhenRunStarted);
            Assert.AreEqual(1, session.CurrentZone);
        }

        [Test]
        public void CompleteSpin_OnBomb_EntersBombHit()
        {
            var session = CreateSession(BOMB_INDEX);
            session.StartRun();

            SpinAndComplete(session);

            Assert.AreEqual(SessionState.BombHit, session.State);
            Assert.IsFalse(session.CanSpin);
            Assert.IsFalse(session.CanLeave);
        }

        [Test]
        public void Revive_AfterBomb_KeepsZoneAndRewards()
        {
            var session = CreateSession(REWARD_INDEX, BOMB_INDEX);
            session.StartRun();
            SpinAndComplete(session);
            SpinAndComplete(session);

            Assert.IsTrue(session.TryRevive(_wallet));

            Assert.AreEqual(SessionState.ReadyToSpin, session.State);
            Assert.AreEqual(2, session.CurrentZone);
            Assert.AreEqual(REWARD_AMOUNT, session.CollectedRewards[_reward]);
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
            var session = CreateSession(BOMB_INDEX);
            session.StartRun();
            SpinAndComplete(session);

            session.TryRevive(_wallet);

            Assert.AreEqual(SessionState.ReadyToSpin, session.State);
            Assert.IsFalse(session.CanLeave);
        }

        [Test]
        public void TryRevive_IncreasesCostOnEveryUse()
        {
            var session = CreateSession(BOMB_INDEX, BOMB_INDEX, BOMB_INDEX);
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
            var session = CreateSession(BOMB_INDEX);
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
            var session = CreateSession(BOMB_INDEX);
            session.StartRun();
            SpinAndComplete(session);
            session.TryRevive(_wallet);

            session.StartRun();

            Assert.AreEqual(REVIVE_BASE_COST, session.ReviveCost);
        }

        [Test]
        public void CanLeave_AfterFirstReward_ClosesDuringSpinAndReopensInNextZone()
        {
            var session = CreateSession(LeaveRule.AfterFirstReward, REWARD_INDEX, REWARD_INDEX);
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
            var session = CreateSession(REWARD_INDEX);
            session.StartRun();
            SpinAndComplete(session);

            session.Leave(_vault);

            Assert.AreEqual(SessionState.CashedOut, session.State);
        }

        [Test]
        public void Leave_DepositsEveryCollectedRewardIntoTheVault()
        {
            var session = CreateSession(REWARD_INDEX, REWARD_INDEX, REWARD_INDEX);
            session.StartRun();

            for (var i = 0; i < 3; i++)
            {
                SpinAndComplete(session);
            }

            session.Leave(_vault);

            Assert.AreEqual(REWARD_AMOUNT * 3, _inventory.GetCount("chest_small"));
        }

        [Test]
        public void StartRun_AfterBombWithoutLeaving_DepositsNothing()
        {
            var session = CreateSession(REWARD_INDEX, BOMB_INDEX);
            session.StartRun();
            SpinAndComplete(session);
            SpinAndComplete(session);

            session.StartRun();

            Assert.AreEqual(0, _inventory.Items.Count);
        }

        [Test]
        public void CanLeave_WithSafeZonesOnly_IsFalseInNormalZoneAndTrueInSafeZone()
        {
            var session = CreateSession(LeaveRule.SafeZonesOnly, REWARD_INDEX, REWARD_INDEX, REWARD_INDEX, REWARD_INDEX);
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
            var session = CreateSession(REWARD_INDEX);
            session.StartRun();
            session.Spin();

            Assert.Throws<InvalidOperationException>(() => session.Spin());
        }

        [Test]
        public void StartRun_AfterCollecting_ClearsRewards()
        {
            var session = CreateSession(REWARD_INDEX);
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
                new FixedWheelGenerator(_reward, REWARD_AMOUNT, BOMB_INDEX),
                leaveRule,
                new RevivePricing(GOLD, REVIVE_BASE_COST, REVIVE_COST_MULTIPLIER),
                new FakeRandomProvider(spinIndices),
                _bus);
        }

        private static InMemoryWallet CreateWallet(int gold)
        {
            var wallet = new InMemoryWallet(new EventBus());
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