using System.Collections.Generic;

namespace WheelGame.Core
{
    public interface IWheelGenerator
    {
        IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneKind zoneKind);
    }
}
