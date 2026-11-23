namespace WheelGame.Core
{
    public interface IProgressStore
    {
        bool TryLoad(out ProgressData data);

        void Save(ProgressData data);
    }
}
