using System.Globalization;
using TMPro;
using UnityEngine;

namespace WheelGame.Views
{
    public sealed class ZoneCellView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _numberText;

        public void Show(int zone, Color color, float alpha)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            color.a = alpha;
            _numberText.text = zone.ToString(CultureInfo.InvariantCulture);
            _numberText.color = color;
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
