using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.Views
{
    public sealed class AmbientBackgroundView : MonoBehaviour
    {
        [SerializeField] private List<Image> _blobs = new List<Image>();
        [SerializeField, Min(0f)] private float _driftDistance = 250f;
        [SerializeField, Min(0.1f)] private float _minDuration = 6f;
        [SerializeField, Min(0.1f)] private float _maxDuration = 11f;
        [SerializeField, Range(0f, 1f)] private float _scaleVariation = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _alphaVariation = 0.4f;

        private readonly List<Vector2> _basePositions = new List<Vector2>();
        private readonly List<float> _baseAlphas = new List<float>();

        private void Awake()
        {
            foreach (var blob in _blobs)
            {
                _basePositions.Add(blob.rectTransform.anchoredPosition);
                _baseAlphas.Add(blob.color.a);
            }
        }

        private void OnEnable()
        {
            for (var i = 0; i < _blobs.Count; i++)
                Animate(_blobs[i], _basePositions[i], _baseAlphas[i]);
        }

        private void OnDisable()
        {
            foreach (var blob in _blobs)
            {
                blob.rectTransform.DOKill();
                blob.DOKill();
            }
        }

        private void Animate(Image blob, Vector2 basePosition, float baseAlpha)
        {
            var blobTransform = blob.rectTransform;
            var duration = Random.Range(_minDuration, _maxDuration);
            var offset = Random.insideUnitCircle.normalized * _driftDistance;

            blobTransform.anchoredPosition = basePosition;
            blobTransform.localScale = Vector3.one;
            var color = blob.color;
            color.a = baseAlpha;
            blob.color = color;

            blobTransform
                .DOAnchorPos(basePosition + offset, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);

            blobTransform
                .DOScale(1f + _scaleVariation, duration * 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);

            blob
                .DOFade(baseAlpha * (1f - _alphaVariation), duration * 1.3f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }
    }
}