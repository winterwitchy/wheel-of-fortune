using DG.Tweening;
using UnityEngine;

namespace WheelGame.Views
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _card;
        [SerializeField, Min(0f)] private float _duration = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _openScale = 0.85f;

        public void Close()
        {
            SetInteractive(false);
            DOTween.Kill(this);

            _canvasGroup
                .DOFade(0f, _duration)
                .SetId(this)
                .SetLink(gameObject)
                .OnComplete(HandleFadedOut);
        }

        private void HandleFadedOut()
        {
            gameObject.SetActive(false);
        }

        protected void Open()
        {
            gameObject.SetActive(true);
            DOTween.Kill(this);

            _canvasGroup.alpha = 0f;
            _card.localScale = Vector3.one * _openScale;
            SetInteractive(true);

            DOTween.Sequence()
                .Join(_canvasGroup.DOFade(1f, _duration))
                .Join(_card.DOScale(1f, _duration).SetEase(Ease.OutBack))
                .SetId(this)
                .SetLink(gameObject);
        }

        private void SetInteractive(bool interactive)
        {
            _canvasGroup.interactable = interactive;
            _canvasGroup.blocksRaycasts = interactive;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
#endif
    }
}