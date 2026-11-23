using System.IO;
using UnityEngine;
using WheelGame.Core;
using WheelGame.Data;
using WheelGame.Presentation;
using WheelGame.Views;

namespace WheelGame.Infrastructure
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        private static readonly SaveMode SAVE_MODE = SaveMode.Persistent;

        private const string PROGRESS_FILE_NAME = "progress.json";

        [SerializeField] private GameConfig _config;
        [SerializeField] private GoldView _goldView;
        [SerializeField] private ZoneProgressBarView _zoneProgressBarView;
        [SerializeField] private WheelView _wheelView;
        [SerializeField] private WheelEffectsView _wheelEffectsView;
        [SerializeField] private RewardsPanelView _rewardsPanelView;
        [SerializeField] private RewardClaimView _rewardClaimView;
        [SerializeField] private BombPopupView _bombPopupView;
        [SerializeField] private CashOutPopupView _cashOutPopupView;

        private GamePresenter _presenter;
        private ProgressPersistence _persistence;

        private void Start()
        {
            var random = new UnityRandomProvider();
            var factory = new CoreModelFactory();
            var zoneRules = new ZoneRules(factory.CreateZoneKinds(_config.Zones));
            var proceduralGenerator = new ProceduralWheelGenerator(factory.CreateBands(_config.Bands), random);
            var wheelGenerator = new OverriddenWheelGenerator(proceduralGenerator, factory.CreateOverrides(_config.SliceOverrides), new WheelBombRule(random));
            var goldId = factory.GetReward(_config.ReviveCurrency).Id;
            var revivePricing = new RevivePricing(goldId, _config.ReviveBaseCost, _config.ReviveCostMultiplier);
            var session = new WheelSession(zoneRules, wheelGenerator, _config.LeaveRule, revivePricing, random);

            var wallet = new InMemoryWallet();
            var inventory = new InMemoryInventory();
            _persistence = new ProgressPersistence(wallet, inventory, CreateProgressStore());
            _persistence.Restore(goldId, _config.StartingGold);

            _presenter = new GamePresenter(
                session, wallet, new RewardVault(wallet, inventory), goldId, new ZoneCatalog(_config.Zones), new RewardIconCatalog(factory.KnownRewards),
                _goldView, _zoneProgressBarView, _wheelView, _wheelEffectsView,
                _rewardsPanelView, _rewardClaimView, _bombPopupView, _cashOutPopupView);

            _presenter.Initialize();
        }

        private void OnDestroy()
        {
            if (_presenter != null)
            {
                _presenter.Dispose();
            }

            if (_persistence != null)
            {
                _persistence.Dispose();
            }
        }

        private static IProgressStore CreateProgressStore()
        {
            if (SAVE_MODE == SaveMode.Temporary)
            {
                return new TransientProgressStore();
            }

            return new JsonFileProgressStore(Path.Combine(Application.persistentDataPath, PROGRESS_FILE_NAME));
        }
    }
}
