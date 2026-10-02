using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.Views
{
    public sealed class WheelSliceView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amountText;

        public void ShowReward(Sprite icon, int amount)
        {
            _icon.sprite = icon;
            _amountText.text = AmountFormatter.Format(amount);
            _amountText.gameObject.SetActive(true);
        }

        public void ShowBomb(Sprite bombIcon)
        {
            _icon.sprite = bombIcon;
            _amountText.gameObject.SetActive(false);
        }
    }
}