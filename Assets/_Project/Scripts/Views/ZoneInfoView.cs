using System.Globalization;
using TMPro;
using UnityEngine;
using WheelGame.Core;

namespace WheelGame.Views
{
    public sealed class ZoneInfoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        [Header("Current Zone")]
        [SerializeField] private string _currentSafeText = "SAFE HERE";
        [SerializeField] private string _currentSuperText = "HIGHER REWARDS";

        [Header("Upcoming Zone")]
        [SerializeField] private string _upcomingSafeText = "SAFE IN {0}";
        [SerializeField] private string _upcomingSuperText = "HIGHER REWARDS IN {0}";

        public void Show(int zone, ZoneType zoneType, int nextRiskFreeZone, ZoneType nextRiskFreeZoneType)
        {
            var detail = zoneType switch
            {
                ZoneType.Safe => _currentSafeText,
                ZoneType.Super => _currentSuperText,
                _ => FormatUpcoming(nextRiskFreeZoneType, nextRiskFreeZone - zone)
            };

            _text.text = $"ZONE {zone.ToString(CultureInfo.InvariantCulture)} - {detail}";
        }

        private string FormatUpcoming(ZoneType upcomingType, int distance)
        {
            var template = upcomingType == ZoneType.Super ? _upcomingSuperText : _upcomingSafeText;
            return string.Format(CultureInfo.InvariantCulture, template, distance);
        }
    }
}