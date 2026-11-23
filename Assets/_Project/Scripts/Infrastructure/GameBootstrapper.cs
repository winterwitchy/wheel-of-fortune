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
        [SerializeField] private BombPopupView _bombPopupView;
        [SerializeField] private CashOutPopupView _cashOutPopupView;

        private GamePresenter _presenter;
        private ProgressPersistence _persistence;

        private void Start()
        {
            var random = new UnityRandomProvider();
            var zoneRules = new ZoneRules(_config.SafeInterval, _config.SuperInterval);
            var proceduralGenerator = new ProceduralWheelGenerator(_config.Generation, random);
            var wheelGenerator = new OverriddenWheelGenerator(proceduralGenerator, _config.SliceOverrides, new WheelBombRule(random));
            var goldId = _config.Generation.GoldReward.Id;
            var revivePricing = new RevivePricing(goldId, _config.ReviveBaseCost, _config.ReviveCostMultiplier);
            var session = new WheelSession(zoneRules, wheelGenerator, _config.LeaveRule, revivePricing, random);

            var wallet = new InMemoryWallet();
            var inventory = new InMemoryInventory();
            _persistence = new ProgressPersistence(wallet, inventory, CreateProgressStore());
            _persistence.Restore(goldId, _config.StartingGold);

            _presenter = new GamePresenter(
                session, wallet, new RewardVault(wallet, inventory), goldId,
                _goldView, _zoneProgressBarView, _wheelView, _wheelEffectsView,
                _rewardsPanelView, _bombPopupView, _cashOutPopupView);

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
