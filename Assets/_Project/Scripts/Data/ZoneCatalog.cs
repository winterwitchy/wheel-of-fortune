using System.Collections.Generic;

namespace WheelGame.Data
{
    public sealed class ZoneCatalog
    {
        private readonly Dictionary<string, ZoneDefinition> _zones = new Dictionary<string, ZoneDefinition>();

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

        public ZoneVisuals GetVisuals(string zoneId)
        {
            return _zones[zoneId].Visuals;
        }
    }
}
