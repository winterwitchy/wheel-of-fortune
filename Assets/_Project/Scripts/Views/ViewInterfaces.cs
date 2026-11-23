using System;
using System.Collections.Generic;
using UnityEngine;
using WheelGame.Core;
using WheelGame.Data;

namespace WheelGame.Views
{
    public interface IWheelView
    {
        void Show(ZoneVisuals visuals, IReadOnlyList<WheelSliceEntry> wheel);

        void SetSpinInteractable(bool interactable);

        void PlaySpin(int sliceIndex);

        Vector3 GetSliceIconPosition(int sliceIndex);
    }

    public interface IWheelEffectsView
    {
        void SetZoneVisuals(ZoneVisuals visuals);

        void PlayBomb();
    }

    public interface IRewardsPanelView
    {
        void Clear();

        RectTransform PrepareClaimTarget(RewardItem reward, int shownAmount);

        void ShowClaimedAmount(RewardItem reward, int amount);

        void SetLeaveInteractable(bool interactable);
    }

    public interface IRewardClaimView
    {
        void Play(Sprite icon, int amount, Vector3 fromWorldPosition, RectTransform target);
    }

    public interface IZoneProgressView
    {
        void Initialize(Func<int, ZoneVisuals> visualsLookup, Func<string, int> nextZoneLookup);

        void Show(int zone);
    }

    public interface ICurrencyView
    {
        void SetIcon(Sprite icon);

        void Show(int amount);
    }

    public interface IBombPopupView
    {
        void Show(int reviveCost, bool canAffordRevive);

        void Close();
    }

    public interface ICashOutPopupView
    {
        void Show(IReadOnlyDictionary<RewardItem, int> rewards);

        void Close();
    }
}
