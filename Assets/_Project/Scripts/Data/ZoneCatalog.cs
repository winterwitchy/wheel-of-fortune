using System.Collections.Generic;

namespace WheelGame.Data
{
    public sealed class ZoneCatalog
    {
        #region Fields

        private readonly Dictionary<string, ZoneDefinition> _zones = new Dictionary<string, ZoneDefinition>();

        #endregion

        #region Constructors

        public ZoneCatalog(IReadOnlyList<ZoneDefinition> zones)
        {
            foreach (var zone in zones)
            {
                if (zone && !string.IsNullOrEmpty(zone.Id))
                {
                    _zones[zone.Id] = zone;
                }
            }
        }

        #endregion

        #region Public Methods

        public ZoneVisuals GetVisuals(string zoneId)
        {
            return _zones[zoneId].Visuals;
        }

        #endregion
    }
}
