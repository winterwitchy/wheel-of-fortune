using System.Collections.Generic;
using WheelGame.Data;

namespace WheelGame.Core
{
    public interface IWheelGenerator
    {
        IReadOnlyList<WheelSliceEntry> Generate(int zone, ZoneType zoneType);
    }
}