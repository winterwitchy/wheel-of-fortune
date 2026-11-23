using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WheelGame.Core;
using WheelGame.Core.Events;
using WheelGame.Data;
using WheelGame.Presentation;
using WheelGame.Views;

namespace WheelGame.Infrastructure
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        #region Constants

        private static readonly SaveMode SAVE_MODE = SaveMode.Persistent;

        private const string PROGRESS_FILE_NAME = "progress.json";

        #endregion

        #region Serialized Fields

        [SerializeField] private GameConfig _config;
        [SerializeField] private GoldView _goldView;
        [SerializeField] private ZoneProgressBarView _zoneProgressBarView;
        [SerializeField] private WheelView _wheelView;
        [SerializeField] private WheelEffectsView _wheelEffectsView;
        [SerializeField] private RewardsPanelView _rewardsPanelView;
        [SerializeField] private RewardClaimView _rewardClaimView;
        [SerializeField] private BombPopupView _bombPopupView;
        [SerializeField] private CashOutPopupView _cashOutPopupView;

        #endregion

        #region Private Fields

        private readonly List<IPresenter> _presenters = new List<IPresenter>();
        private ProgressPersistence _persistence;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            var bus = new EventBus();
            var random = new UnityRandomProvider();
            var factory = new CoreModelFactory();

            var zoneRules = new ZoneRules(factory.CreateZoneKinds(_config.Zones));
            var proceduralGenerator = new ProceduralWheelGenerator(factory.CreateBands(_config.Bands), random);
            var wheelGenerator = new OverriddenWheelGenerator(proceduralGenerator, factory.CreateOverrides(_config.SliceOverrides), new WheelBombRule(random));
            var goldId = factory.GetReward(_config.ReviveCurrency).Id;
            var revivePricing = new RevivePricing(goldId, _config.ReviveBaseCost, _config.ReviveCostMultiplier);
            var session = new WheelSession(zoneRules, wheelGenerator, _config.LeaveRule, revivePricing, random, bus);

            var wallet = new InMemoryWallet(bus);
            var inventory = new InMemoryInventory(bus);
            _persistence = new ProgressPersistence(wallet, inventory, CreateProgressStore(), bus);
            _persistence.Restore(goldId, _config.StartingGold);

            var icons = new RewardIconCatalog(factory.KnownRewards);
            var zones = new ZoneCatalog(_config.Zones);

            _wheelView.Construct(bus, icons);
            _wheelEffectsView.Construct(bus);
            _rewardsPanelView.Construct(bus, icons);
            _rewardClaimView.Construct(bus);
            _bombPopupView.Construct(bus);
            _cashOutPopupView.Construct(bus, icons);

            _presenters.Add(new CurrencyPresenter(bus, wallet, goldId, _goldView));
            _presenters.Add(new ZoneProgressPresenter(bus, session, zones, _zoneProgressBarView));
            _presenters.Add(new WheelPresenter(bus, session, zones, _wheelView, _wheelEffectsView));
            _presenters.Add(new RewardsPresenter(bus, session, icons, _rewardsPanelView, _rewardClaimView, _wheelView));
            _presenters.Add(new PopupsPresenter(bus, session, wallet, new RewardVault(wallet, inventory), _bombPopupView, _cashOutPopupView));

            foreach (var presenter in _presenters)
            {
                presenter.Initialize();
            }

            session.StartRun();
        }

        private void OnDestroy()
        {
            foreach (var presenter in _presenters)
            {
                presenter.Dispose();
            }

            _presenters.Clear();

            if (_persistence != null)
            {
                _persistence.Dispose();
            }
        }

        #endregion

        #region Private Methods

        private static IProgressStore CreateProgressStore()
        {
            if (SAVE_MODE == SaveMode.Temporary)
            {
                return new TransientProgressStore();
            }

            return new JsonFileProgressStore(Path.Combine(Application.persistentDataPath, PROGRESS_FILE_NAME));
        }

        #endregion
    }
}
