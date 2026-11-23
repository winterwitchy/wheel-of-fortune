namespace WheelGame.Core
{
    public sealed class TransientProgressStore : IProgressStore
    {
        #region Public Methods

        public bool TryLoad(out ProgressData data)
        {
            data = null;
            return false;
        }

        public void Save(ProgressData data)
        {
        }

        #endregion
    }
}
