using UnityEngine;
using WheelGame.Core;
using WheelGame.Data;
using WheelGame.Presentation;
using WheelGame.Views;

namespace WheelGame.Infrastructure
{
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameConfig _config;
        [SerializeField] private GoldView _goldView;
        [SerializeField] private ZoneInfoView _zoneInfoView;
        [SerializeField] private WheelView _wheelView;
        [SerializeField] private WheelEffectsView _wheelEffectsView;
        [SerializeField] private RewardsPanelView _rewardsPanelView;
        [SerializeField] private BombPopupView _bombPopupView;
        [SerializeField] private CashOutPopupView _cashOutPopupView;

        private GamePresenter _presenter;

        private void Start()
        {
            var random = new UnityRandomProvider();
            var zoneRules = new ZoneRules(_config.SafeInterval, _config.SuperInterval);
            var proceduralGenerator = new ProceduralWheelGenerator(_config.Generation, random);
            var wheelGenerator = new OverriddenWheelGenerator(proceduralGenerator, _config.SliceOverrides, random);
            var session = new WheelSession(zoneRules, wheelGenerator, _config.LeaveRule, random);
            var wallet = new InMemoryWallet(_config.StartingGold);

            _presenter = new GamePresenter(
                session, wallet, _config.ReviveCost, _config.Generation.GoldReward,
                _goldView, _zoneInfoView, _wheelView, _wheelEffectsView,
                _rewardsPanelView, _bombPopupView, _cashOutPopupView);

            _presenter.Initialize();
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
        }
    }
}