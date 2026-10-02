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
        [SerializeField] private WheelView _wheelView;
        [SerializeField] private RewardsPanelView _rewardsPanelView;

        private GamePresenter _presenter;

        private void Start()
        {
            var random = new UnityRandomProvider();
            var zoneRules = new ZoneRules(_config.SafeInterval, _config.SuperInterval);
            var proceduralGenerator = new ProceduralWheelGenerator(_config.Generation, random);
            var wheelGenerator = new OverriddenWheelGenerator(proceduralGenerator, _config.SliceOverrides, random);
            var session = new WheelSession(zoneRules, wheelGenerator, _config.LeaveRule, random);

            _presenter = new GamePresenter(session, _wheelView, _rewardsPanelView);
            _presenter.Initialize();
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
        }
    }
}