using WheelGame.Data;

namespace WheelGame.Core
{
    public readonly struct SpinResult
    {
        public SpinResult(int sliceIndex, WheelSliceEntry slice)
        {
            SliceIndex = sliceIndex;
            Slice = slice;
        }

        public int SliceIndex { get; }
        public WheelSliceEntry Slice { get; }
    }
}