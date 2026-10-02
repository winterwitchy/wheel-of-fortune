using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.Views
{
    public sealed class RewardCellView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amountText;

        public void Show(Sprite icon, int amount)
        {
            _icon.sprite = icon;
            _amountText.text = AmountFormatter.Format(amount);
        }
    }
}