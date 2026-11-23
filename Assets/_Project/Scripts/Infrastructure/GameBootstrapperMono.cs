using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UnityEngine;
using WheelGame.Core;
using WheelGame.Core.Events;
using WheelGame.Data;
using WheelGame.Presentation;
using WheelGame.Views;

namespace WheelGame.Infrastructure
{
    public sealed class GameBootstrapperMono : MonoBehaviour
    {
        #region Constants

        private static readonly SaveMode SAVE_MODE = SaveMode.Persistent;

        private const string PROGRESS_FILE_NAME = "progress.json";

        #endregion

        #region Serialized Fields

        [SerializeField] private GameConfig _config;
        [SerializeField] private CanvasGroup _rootCanvasGroup;
        [SerializeField, Min(0f)] private float _revealDuration = 0.3f;
        [SerializeField] private GoldViewMono _goldView;
        [SerializeField] private ZoneProgressBarViewMono _zoneProgressBarView;
        [SerializeField] private WheelViewMono _wheelView;
        [SerializeField] private WheelEffectsViewMono _wheelEffectsView;
        [SerializeField] private RewardsPanelViewMono _rewardsPanelView;
        [SerializeField] private RewardClaimViewMono _rewardClaimView;
        [SerializeField] private BombPopupViewMono _bombPopupView;
        [SerializeField] private CashOutPopupViewMono _cashOutPopupView;

        #endregion

        #region Private Fields

        private readonly List<IPresenter> _presenters = new List<IPresenter>();
        private ProgressPersistence _persistence;
        private AddressableIconProvider _icons;

        #endregion

        #region Unity Callbacks

        private IEnumerator Start()
        {
            SetInterfaceVisible(false);

            var configErrors = _config.GetValidationErrors();

            if (configErrors.Count > 0)
            {
                foreach (var error in configErrors)
                {
                    Debug.LogError($"{_config.name}: {error}", _config);
                }

                yield break;
            }

            Application.targetFrameRate = _config.TargetFrameRate;

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

            _icons = new AddressableIconProvider();
            yield return _icons.Load(factory.KnownRewards);

            var icons = _icons;
            var zones = new ZoneCatalog(_config.Zones);

            _wheelView.Construct(bus, icons, _config.WheelSpin);
            _wheelEffectsView.Construct(bus);
            _rewardsPanelView.Construct(bus, icons);
            _rewardClaimView.Construct(bus);
            _bombPopupView.Construct(bus);
            _cashOutPopupView.Construct(bus, icons);

            _presenters.Add(new CurrencyPresenter(bus, wallet, goldId, icons, _goldView));
            _presenters.Add(new ZoneProgressPresenter(bus, session, zones, _zoneProgressBarView));
            _presenters.Add(new WheelPresenter(bus, session, zones, _wheelView, _wheelEffectsView));
            _presenters.Add(new RewardsPresenter(bus, session, icons, _rewardsPanelView, _rewardClaimView, _wheelView));
            _presenters.Add(new PopupsPresenter(bus, session, wallet, new RewardVault(wallet, inventory), _bombPopupView, _cashOutPopupView));

            foreach (var presenter in _presenters)
            {
                presenter.Initialize();
            }

            session.StartRun();
            SetInterfaceVisible(true);
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

            if (_icons != null)
            {
                _icons.Dispose();
            }
        }

        #endregion

        #region Private Methods

        private void SetInterfaceVisible(bool visible)
        {
            _rootCanvasGroup.interactable = visible;
            _rootCanvasGroup.blocksRaycasts = visible;

            if (!visible)
            {
                _rootCanvasGroup.alpha = 0f;
                return;
            }

            _rootCanvasGroup.DOFade(1f, _revealDuration).SetLink(gameObject);
        }

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
