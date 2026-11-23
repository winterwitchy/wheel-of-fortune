using System.Collections.Generic;
using WheelGame.Core;

namespace WheelGame.Tests.EditMode
{
    public static class TestZones
    {
        public static readonly ZoneKind NORMAL = new ZoneKind("normal", 1, 0, true, 1, null, null);
        public static readonly ZoneKind SAFE = new ZoneKind("safe", 5, 10, false, 2, null, null);
        public static readonly ZoneKind SUPER = new ZoneKind("super", 30, 20, false, 5, null, null);

        public static ZoneRules CreateRules()
        {
            return new ZoneRules(new List<ZoneKind> { NORMAL, SAFE, SUPER });
        }
    }
}
